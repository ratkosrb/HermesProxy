using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_TAXI_NODE_STATUS_QUERY)]
        [PacketHandler(Opcode.CMSG_TAXI_QUERY_AVAILABLE_NODES)]
        void HandleTaxiNodesQuery(InteractWithNPC interact)
        {
            WorldPacket packet = new WorldPacket(interact.GetUniversalOpcode());
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ENABLE_TAXI_NODE)]
        void HandleEnableTaxiNode(InteractWithNPC interact)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_TALK_TO_GOSSIP);
            packet.WriteGuid(interact.CreatureGUID.To64());
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_ACTIVATE_TAXI)]
        void HandleActivateTaxi(ActivateTaxi taxi)
        {
            // direct path exist
            if (GameData.TaxiPathExist(GetSession().GameState.CurrentTaxiNode, taxi.Node))
            {
                WorldPacket packet = new WorldPacket(Opcode.CMSG_ACTIVATE_TAXI);
                packet.WriteGuid(taxi.FlightMaster.To64());
                packet.WriteUInt32(GetSession().GameState.CurrentTaxiNode);
                packet.WriteUInt32(taxi.Node);
                SendPacketToServer(packet);
            }
            else // find shortest path
            {
                HashSet<uint> path = GameData.GetTaxiPath(GetSession().GameState.CurrentTaxiNode, taxi.Node, GetSession().GameState.UsableTaxiNodes);
                if (path.Count <= 1) // no nodes found
                    return;

                WorldPacket packet = new WorldPacket(Opcode.CMSG_ACTIVATE_TAXI_EXPRESS);
                packet.WriteGuid(taxi.FlightMaster.To64());
                packet.WriteUInt32(0);                // total cost, not used
                packet.WriteUInt32((uint)path.Count); // node count
                foreach (uint itr in path)
                    packet.WriteUInt32(itr);
                SendPacketToServer(packet);
            }
            GetSession().GameState.IsWaitingForTaxiStart = true;
        }
    }
}
