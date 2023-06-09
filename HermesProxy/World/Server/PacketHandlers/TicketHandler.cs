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
        [PacketHandler(Opcode.CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT)]
        void HandleSupportTicketSubmitComplaint(SupportTicketSubmitComplaint complaint)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GM_TICKET_CREATE);

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                GMTicketCategory category;
                switch ((CliComplaintType)complaint.ComplaintType)
                {
                    case CliComplaintType.Name:
                    case CliComplaintType.Cheating:
                        category = GMTicketCategory.Character;
                        break;
                    case CliComplaintType.Spamming:
                    case CliComplaintType.Language:
                        category = GMTicketCategory.BehaviorHarassment;
                        break;
                    case CliComplaintType.GuildName:
                        category = GMTicketCategory.Guild;
                        break;
                    case CliComplaintType.PetName:
                        category = GMTicketCategory.NonQuestCreep;
                        break;
                    default:
                        category = GMTicketCategory.Character;
                        break;
                }

                packet.WriteUInt8((byte)category);
            }

            packet.WriteUInt32(complaint.Header.MapID);
            packet.WriteVector3(complaint.Header.Position);

            string message = "";
            if (!String.IsNullOrEmpty(complaint.Note))
                message = complaint.Note + "\n\nAdditional Information\n";
            message += $"Report reason: {(CliComplaintType)complaint.ComplaintType}\n";
            if (!complaint.TargetCharacterGUID.IsEmpty())
                message += $"Player: {GetSession().GameState.GetPlayerName(complaint.TargetCharacterGUID)} (Guid {complaint.TargetCharacterGUID.GetCounter()})\n";
            if (complaint.GuildInfo != null)
                message += $"Guild: {complaint.GuildInfo.GuildName}\n";
            if (complaint.PetInfo != null)
                message += $"Pet: {complaint.PetInfo.PetName}\n";
            foreach (var line in complaint.ChatLog.Lines)
                message += $"Chat message: {line.Text}\n";

            packet.WriteCString(message);

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            {
                packet.WriteUInt32(0); // unknown
                packet.WriteUInt32(0); // chatDataLineCount
                packet.WriteUInt32(0); // chatDataSizeInflated
            }
            else
                packet.WriteCString(""); // unused by server

            SendPacketToServer(packet);
        }
    }
}
