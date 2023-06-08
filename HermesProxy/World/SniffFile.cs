﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World
{
    public class SniffFile
    {
        public SniffFile(string fileName, bool isLegacyVersion)
        {
            string path = "PacketsLog";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            _gameVersion = isLegacyVersion ? (ushort)LegacyVersion.BuildInt : (ushort)ModernVersion.BuildInt;
            _isLegacyVersion = isLegacyVersion;

            string file = fileName + "_" + _gameVersion + "_" + Time.UnixTime + ".pkt";
            path = Path.Combine(path, file);
            _fileWriter = new System.IO.BinaryWriter(File.Open(path, FileMode.Create));
        }
        BinaryWriter _fileWriter;
        ushort _gameVersion;
        bool _isLegacyVersion;
        System.Threading.Mutex _mutex = new System.Threading.Mutex();

        public void WriteHeader()
        {
            _fileWriter.Write('P');
            _fileWriter.Write('K');
            _fileWriter.Write('T');
            UInt16 sniffVersion = 0x201;
            _fileWriter.Write(sniffVersion);
            _fileWriter.Write(_gameVersion);

            for (int i = 0; i < 40; i++)
            {
                byte zero = 0;
                _fileWriter.Write(zero);
            }
        }

        public void WritePacket(uint opcode, bool isFromClient, byte[] data)
        {
            _mutex.WaitOne();
            byte direction = !isFromClient ? (byte)0xff : (byte)0x0;
            _fileWriter.Write(direction);

            uint unixtime = (uint)Time.UnixTime;
            _fileWriter.Write(unixtime);
            _fileWriter.Write(Environment.TickCount);

            if (isFromClient)
            {
                if (!_isLegacyVersion)
                {
                    uint packetSize = (uint)(data.Length - 2 + sizeof(uint));
                    _fileWriter.Write(packetSize);
                    _fileWriter.Write(opcode);

                    for (int i = 2; i < data.Length; i++)
                        _fileWriter.Write(data[i]);
                }
                else
                {
                    uint packetSize = (uint)(data.Length + sizeof(uint));
                    _fileWriter.Write(packetSize);
                    _fileWriter.Write(opcode);

                    _fileWriter.Write(data);
                }
            }
            else
            {
                if (!_isLegacyVersion)
                {
                    uint packetSize = (uint)data.Length + sizeof(ushort);
                    _fileWriter.Write(packetSize);

                    ushort opcode2 = (ushort)opcode;
                    _fileWriter.Write(opcode2);
                }
                else
                {
                    uint packetSize = (uint)data.Length;
                    _fileWriter.Write(packetSize);

                    // data buffer includes opcode
                }
                
                _fileWriter.Write(data);
            }
            _mutex.ReleaseMutex();
        }

        public void CloseFile()
        {
            _fileWriter.Close();
        }
    }
}
