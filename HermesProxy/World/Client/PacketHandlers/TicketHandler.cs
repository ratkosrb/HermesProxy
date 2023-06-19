using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_GM_TICKET_SYSTEM_STATUS)]
        void HandleGmTicketSystemStatus(WorldPacket packet)
        {
            GMTicketSystemStatusPkt status = new GMTicketSystemStatusPkt();
            status.Status = packet.ReadInt32();
            SendPacketToClient(status);
        }

        [PacketHandler(Opcode.SMSG_GM_TICKET_DELETE_TICKET)]
        void HandleGmTicketDeleteTicket(WorldPacket packet)
        {
            GMTicketCaseStatus tickets = new GMTicketCaseStatus();
            packet.ReadUInt32(); // response
            SendPacketToClient(tickets);
        }

        [PacketHandler(Opcode.SMSG_GM_TICKET_CREATE)]
        void HandleGmTicketCreate(WorldPacket packet)
        {
            uint response = packet.ReadUInt32();
            if (response == 2)
            {
                WorldPacket reply = new WorldPacket(Opcode.CMSG_GM_TICKET_GET_TICKET);
                SendPacketToServer(reply);
            }
        }

        [PacketHandler(Opcode.SMSG_GM_TICKET_GET_TICKET)]
        void HandleGmTicketGetTicket(WorldPacket packet)
        {
            GMTicketCaseStatus tickets = new GMTicketCaseStatus();
            
            uint status = packet.ReadUInt32();
            if (status == 6)
            {
                GMTicketCaseStatus.GMTicketCase ticket = new GMTicketCaseStatus.GMTicketCase();
                ticket.CaseID = 1;
                ticket.CfgRealmID = 1;
                ticket.CharacterID = GetSession().GameState.CurrentPlayerGuid.GetCounter();
                ticket.Url = "";
                ticket.WaitTimeOverrideMessage = packet.ReadCString();
                packet.ReadUInt8(); // type
                packet.ReadFloat(); // last modified time of this ticket
                packet.ReadFloat(); // last modified time of any ticket
                ticket.WaitTimeOverrideMinutes = (int)packet.ReadFloat();
                ticket.CaseStatus = packet.ReadUInt8();
                ticket.CaseOpened = packet.ReadUInt8();
                tickets.Cases.Add(ticket);
            }

            SendPacketToClient(tickets);
        }
    }
}
