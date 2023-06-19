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
        [PacketHandler(Opcode.CMSG_GM_TICKET_GET_SYSTEM_STATUS)]
        void HandleGmTicektGetSystemStatus(EmptyClientPacket status)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GM_TICKET_GET_SYSTEM_STATUS);
            SendPacketToServer(packet);
        }

        [PacketHandler(Opcode.CMSG_GM_TICKET_GET_CASE_STATUS)]
        void HandleGmTicketGetCaseStatus(EmptyClientPacket status)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GM_TICKET_GET_TICKET);
            SendPacketToServer(packet);
        }

        void SendTicketCreate(SupportTicketHeader header, GMTicketCategory category, string message)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_GM_TICKET_CREATE);

            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
                packet.WriteUInt8((byte)category);

            packet.WriteUInt32(header.MapID);
            packet.WriteVector3(header.Position);

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

        [PacketHandler(Opcode.CMSG_SUPPORT_TICKET_SUBMIT_COMPLAINT)]
        void HandleSupportTicketSubmitComplaint(SupportTicketSubmitComplaint complaint)
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

            SendTicketCreate(complaint.Header, category, message);
        }

        [PacketHandler(Opcode.CMSG_SUPPORT_TICKET_SUBMIT_BUG)]
        void HandleSupportTicketSubmitBug(SupportTicketSubmitBug bug)
        {
            CliBugType type = CliBugType.Bug;
            string note = "";
            string info = "";

            // additional data is encoded in string format
            string[] sections = bug.Note.Split(',');
            for (int i = 0; i < sections.Length; i++)
            {
                switch (i)
                {
                    case 0: // garbage [*C&^$#@]
                        break;
                    case 1: // type
                        type = (CliBugType)Int32.Parse(sections[i]);
                        info += $"Type: {type}\n";
                        break;
                    case 2: // level
                        info += $"Level: {sections[i]}\n";
                        break;
                    case 3: // faction
                        info += $"Faction: {sections[i]}\n";
                        break;
                    case 4: // race
                        Race race = (Race)Int32.Parse(sections[i]);
                        info += $"Race: {race}\n";
                        break;
                    case 5: // gender
                        Gender gender = (Gender)(Int32.Parse(sections[i]) - 2);
                        info += $"Gender: {gender}\n";
                        break;
                    case 6: // class
                        Class classId = (Class)Int32.Parse(sections[i]);
                        info += $"Class: {classId}\n";
                        break;
                    case 7: // parameter or note
                        switch (type)
                        {
                            case CliBugType.Creature:
                            case CliBugType.Quest:
                            case CliBugType.Spell:
                            case CliBugType.Item:
                            case CliBugType.Talent:
                                info += $"{type}: {sections[i]}\n";
                                break;
                            case CliBugType.Skill:
                                info += $"Skill Name: {sections[i]}\n";
                                break;
                            default:
                                note += sections[i];
                                break;
                        }
                        break;
                    case 8: // skill value or note
                        if (type == CliBugType.Skill)
                            info += $"Skill Value: {sections[i]}\n";
                        else
                            note += sections[i];
                        break;
                    default: // note
                        note += sections[i];
                        break;
                }
            }

            GMTicketCategory category;
            switch (type)
            {
                case CliBugType.Creature:
                    category = GMTicketCategory.NonQuestCreep;
                    break;
                case CliBugType.Quest:
                    category = GMTicketCategory.QuestNPC;
                    break;
                case CliBugType.Item:
                    category = GMTicketCategory.Item;
                    break;
                case CliBugType.Spell:
                case CliBugType.Talent:
                case CliBugType.Skill:
                    category = GMTicketCategory.Character;
                    break;
                default:
                {
                    string lower = note.ToLower();
                    if (lower.Contains("stuck"))
                        category = GMTicketCategory.Stuck;
                    else if (lower.Contains("lag") ||
                             lower.Contains("fps") ||
                             lower.Contains("stutter") ||
                             lower.Contains("screen") ||
                             lower.Contains("graphics") ||
                             lower.Contains("sound"))
                        category = GMTicketCategory.Technical;
                    else if (lower.Contains("account") ||
                             lower.Contains("mute") ||
                             lower.Contains("banned") ||
                             lower.Contains("unban"))
                        category = GMTicketCategory.AccountBilling;
                    else
                        category = GMTicketCategory.Character;
                    break;
                }
            }

            string message = "";
            if (!String.IsNullOrEmpty(note))
                message = note + "\n\nAdditional Information\n";
            message += info;

            SendTicketCreate(bug.Header, category, message);
        }
    }
}
