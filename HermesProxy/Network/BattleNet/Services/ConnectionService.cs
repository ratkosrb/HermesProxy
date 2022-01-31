using System;

using Bgs.Protocol.Connection.V1;

using HermesProxy.Framework.Constants;
using HermesProxy.Framework.Util;
using HermesProxy.Network.BattleNet.Services;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        [BattlenetService(ServiceHash.ConnectionService, 1)]
        public BattlenetRpcErrorCode HandleConnectRequest(ConnectRequest request, ConnectResponse response)
        {
            if (request.ClientId != null)
                response.ClientId.MergeFrom(request.ClientId);

            response.ServerId = new()
            {
                Label = (uint)Environment.ProcessId,
                Epoch = (uint)Time.UnixTime
            };
            response.ServerTime = (ulong)Time.UnixTimeMilliseconds;
            response.UseBindlessRpc = request.UseBindlessRpc;

            return BattlenetRpcErrorCode.Ok;
        }

        [BattlenetService(ServiceHash.ConnectionService, 5)]
        public BattlenetRpcErrorCode HandleKeepAlive(Bgs.Protocol.NoData request)
        {
            return BattlenetRpcErrorCode.Ok;
        }

        [BattlenetService(ServiceHash.ConnectionService, 7)]
        BattlenetRpcErrorCode HandleRequestDisconnect(DisconnectRequest request)
        {
            var disconnectNotification = new DisconnectNotification();
            disconnectNotification.ErrorCode = request.ErrorCode;
            _ = SendRequest(ServiceHash.ConnectionService, 4, disconnectNotification);

            _ = CloseSocket();
            return BattlenetRpcErrorCode.Ok;
        }
    }
}
