using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using Google.Protobuf;

using HermesProxy.Framework.Constants;
using HermesProxy.Framework.IO.Packet;
using HermesProxy.Framework.Logging;
using HermesProxy.Framework.Util;
using HermesProxy.Network.BattleNet.REST;
using HermesProxy.Network.BattleNet.Services;
using HermesProxy.Network.Realm;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        public delegate BattlenetRpcErrorCode ClientRequestHandler(Dictionary<string, Variant> parameters, ClientResponse response);

        readonly Socket _socket;
        readonly SslStream _sslStream;
        readonly byte[] _buffer = new byte[4096];

        bool _authed;
        uint _requestToken = 0;
        byte[] _clientSecret;

        string _locale = "";
        string _os = "";
        uint _build = 0;

        public BattlenetSession(Socket socket, X509Certificate2 cert)
        {
            if (_sslStream != null)
                throw new InvalidOperationException("There is already a BattlenetSession initialized!");

            _socket = socket;
            _clientSecret = new byte[32];

            _sslStream = new SslStream(new NetworkStream(socket), false);
            _sslStream.AuthenticateAsServer(cert, false, SslProtocols.Tls, false);
        }

        /// <summary>
        /// Handles any incoming <see cref="BattlenetHandler"/>
        /// </summary>
        public async Task HandleIncomingConnection()
        {
            while (true)
            {
                if (_sslStream == null)
                    return;

                var receivedLen = await _sslStream.ReadAsync(_buffer);
                if (receivedLen > 0)
                {
                    var data = new byte[receivedLen];
                    Buffer.BlockCopy(_buffer, 0, data, 0, receivedLen);

                    var inputStream = new CodedInputStream(data, 0, data.Length);
                    while (!inputStream.IsAtEnd)
                    {
                        try
                        {
                            var header = new Header();
                            inputStream.ReadMessage(header);

                            if (header.ServiceId != 0xFE && header.ServiceHash != 0)
                            {
                                var handler = ServiceHandler.GetHandler((ServiceHash)header.ServiceHash, header.MethodId);
                                if (handler != null)
                                    await handler.Invoke(this, header.Token, inputStream);
                                else
                                {
                                    Log.Print(LogType.Error, $"Session ({GetRemoteEndpoint()}) tried to call not implemented MethodID: {header.MethodId} for ServiceHash: {(ServiceHash)header.ServiceHash} (0x{header.ServiceHash:X})");
                                    await SendResponse(header.Token, BattlenetRpcErrorCode.RpcNotImplemented);
                                }
                            }
                        }
                        catch /*(Exception ex)*/
                        {
                            // Log.Print(LogType.Error, ex);

                            await CloseSocket();
                            return;
                        }
                    }
                }

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Sends a <see cref="IMessage"/> to the client.
        /// </summary>
        public async Task SendResponse(uint token, IMessage message)
        {
            var header = new Header
            {
                Token = token,
                ServiceId = 0xFE,
                Size = (uint)message.CalculateSize()
            };

            var headerSize = BitConverter.GetBytes((ushort)header.CalculateSize());
            Array.Reverse(headerSize);

            var writer = new PacketWriter();
            writer.WriteBytes(headerSize, 2);
            writer.WriteBytes(header.ToByteArray());
            writer.WriteBytes(message.ToByteArray());

            // Send the data through the sslStream
            await SendData(writer.GetData());
        }

        /// <summary>
        /// Sends a <see cref="BattlenetRpcErrorCode"/> to the client.
        /// </summary>
        public async Task SendResponse(uint token, BattlenetRpcErrorCode errorCode)
        {
            var header = new Header
            {
                Token = token,
                Status = (uint)errorCode,
                ServiceId = 0xFE
            };

            var headerSize = BitConverter.GetBytes((ushort)header.CalculateSize());
            Array.Reverse(headerSize);

            var writer = new PacketWriter();
            writer.WriteBytes(headerSize, 2);
            writer.WriteBytes(header.ToByteArray());

            // Send the data through the sslStream
            await SendData(writer.GetData());
        }

        /// <summary>
        /// Sends a <see cref="IMessage"/> request to the client from the sslstream
        /// </summary>
        public async Task SendRequest(ServiceHash hash, uint methodId, IMessage message)
        {
            var header = new Header
            {
                ServiceId = 0,
                ServiceHash = (uint)hash,
                MethodId = methodId,
                Size = (uint)message.CalculateSize(),
                Token = _requestToken++
            };

            var headerSize = BitConverter.GetBytes((ushort)header.CalculateSize());
            Array.Reverse(headerSize);

            var writer = new PacketWriter();
            writer.WriteBytes(headerSize);
            writer.WriteBytes(header.ToByteArray());
            writer.WriteBytes(message.ToByteArray());

            // Send the data through the sslStream
            await SendData(writer.GetData());
        }

        private BattlenetRpcErrorCode GetLastPlayedCharacter(Dictionary<string, Variant> parameters, ClientResponse response)
            => _authed ? BattlenetRpcErrorCode.Ok : BattlenetRpcErrorCode.Denied;

        private Variant GetParam(Dictionary<string, Variant> parameters, string paramName) => parameters[paramName];

        private async Task SendData(byte[] data) => await _sslStream.WriteAsync(data);

        /// <summary>
        /// Returns the <see cref="Socket"/> instance <see cref="EndPoint"/>.
        /// </summary>
        public string GetRemoteEndpoint() => $"{_socket.RemoteEndPoint}";

        public IPEndPoint GetRemoteIpEndPoint()
        {
            return (IPEndPoint)_socket.RemoteEndPoint;
        }

        /// <summary>
        /// Closes the <see cref="SslStream"/> instance.
        /// </summary>
        public async Task CloseSocket()
        {
            await _sslStream.ShutdownAsync();
            _sslStream?.Close();
        }
    }
}
