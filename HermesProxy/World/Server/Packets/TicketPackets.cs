/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using Framework.Constants;
using Framework.GameMath;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using System;
using System.Collections.Generic;

namespace HermesProxy.World.Server.Packets
{
    public class GMTicketSystemStatusPkt : ServerPacket
    {
        public GMTicketSystemStatusPkt() : base(Opcode.SMSG_GM_TICKET_SYSTEM_STATUS) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Status);
        }

        public int Status;
    }

    public class GMTicketGetCaseStatus : ClientPacket
    {
        public GMTicketGetCaseStatus(WorldPacket packet) : base(packet) { }

        public override void Read() { }
    }

    public class GMTicketCaseStatus : ServerPacket
    {
        public GMTicketCaseStatus() : base(Opcode.SMSG_GM_TICKET_CASE_STATUS) { }

        public override void Write()
        {
            _worldPacket.WriteInt32(Cases.Count);

            foreach (var c in Cases)
            {
                _worldPacket.WriteInt32(c.CaseID);
                _worldPacket.WriteInt64(c.CaseOpened);
                _worldPacket.WriteInt32(c.CaseStatus);
                _worldPacket.WriteUInt16(c.CfgRealmID);
                _worldPacket.WriteUInt64(c.CharacterID);
                _worldPacket.WriteInt32(c.WaitTimeOverrideMinutes);

                _worldPacket.WriteBits(c.Url.GetByteCount(), 11);
                _worldPacket.WriteBits(c.WaitTimeOverrideMessage.GetByteCount(), 10);

                _worldPacket.WriteString(c.Url);
                _worldPacket.WriteString(c.WaitTimeOverrideMessage);
            }
        }

        public List<GMTicketCase> Cases = new();

        public struct GMTicketCase
        {
            public int CaseID;
            public long CaseOpened;
            public int CaseStatus;
            public ushort CfgRealmID;
            public ulong CharacterID;
            public int WaitTimeOverrideMinutes;
            public string Url;
            public string WaitTimeOverrideMessage;
        }
    }

    class SupportTicketSubmitBug : ClientPacket
    {
        public SupportTicketSubmitBug(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Header.Read(_worldPacket);
            uint noteLen = _worldPacket.ReadBits<uint>(10);
            Note = _worldPacket.ReadString(noteLen);
        }

        public SupportTicketHeader Header;
        public string Note;
    }

    public class SupportTicketSubmitComplaint : ClientPacket
    {
        public SupportTicketSubmitComplaint(WorldPacket packet) : base(packet) { }

        public override void Read()
        {
            Header.Read(_worldPacket);
            TargetCharacterGUID = _worldPacket.ReadPackedGuid128();
            ChatLog.Read(_worldPacket);
            ComplaintType = _worldPacket.ReadBits<byte>(5);

            uint noteLength = _worldPacket.ReadBits<uint>(10);
            bool hasMailInfo = _worldPacket.HasBit();
            bool hasCalendarInfo = _worldPacket.HasBit();
            bool hasPetInfo = _worldPacket.HasBit();
            bool hasGuildInfo = _worldPacket.HasBit();
            bool hasLFGListSearchResult = _worldPacket.HasBit();
            bool hasLFGListApplicant = _worldPacket.HasBit();
            bool hasClubMessage = _worldPacket.HasBit();
            bool hasClubFinderResult = _worldPacket.HasBit();
            bool hasUnk910 = _worldPacket.HasBit();

            _worldPacket.ResetBitPos();

            if (hasClubMessage)
            {
                SupportTicketCommunityMessage communityMessage = new();
                communityMessage.IsPlayerUsingVoice = _worldPacket.HasBit();
                CommunityMessage = communityMessage;
                _worldPacket.ResetBitPos();
            }

            HorusChatLog.Read(_worldPacket);

            Note = _worldPacket.ReadString(noteLength);

            if (hasMailInfo)
            {
                MailInfo = new();
                MailInfo.Read(_worldPacket);
            }

            if (hasCalendarInfo)
            {
                CalenderInfo = new();
                CalenderInfo.Read(_worldPacket);
            }

            if (hasPetInfo)
            {
                PetInfo = new();
                PetInfo.Read(_worldPacket);
            }

            if (hasGuildInfo)
            {
                GuildInfo = new();
                GuildInfo.Read(_worldPacket);
            }

            if (hasLFGListSearchResult)
            {
                LFGListSearchResult = new();
                LFGListSearchResult.Read(_worldPacket);
            }

            if (hasLFGListApplicant)
            {
                LFGListApplicant = new();
                LFGListApplicant.Read(_worldPacket);
            }

            if (hasClubFinderResult)
            {
                ClubFinderResult = new();
                ClubFinderResult.Read(_worldPacket);
            }

            if (hasUnk910)
            {
                Unused910 = new();
                Unused910.Read(_worldPacket);
            }
        }

        public SupportTicketHeader Header;
        public SupportTicketChatLog ChatLog = new SupportTicketChatLog();
        public WowGuid128 TargetCharacterGUID;
        public byte ComplaintType;
        public string Note;
        public SupportTicketHorusChatLog HorusChatLog = new SupportTicketHorusChatLog();
        public SupportTicketMailInfo MailInfo;
        public SupportTicketCalendarEventInfo CalenderInfo;
        public SupportTicketPetInfo PetInfo;
        public SupportTicketGuildInfo GuildInfo;
        public SupportTicketLFGListSearchResult LFGListSearchResult;
        public SupportTicketLFGListApplicant LFGListApplicant;
        public SupportTicketCommunityMessage CommunityMessage;
        public SupportTicketClubFinderResult ClubFinderResult;
        public SupportTicketUnused910 Unused910;

        public struct SupportTicketChatLine
        {
            public long Timestamp;
            public string Text;

            public SupportTicketChatLine(WorldPacket data)
            {
                Timestamp = data.ReadInt64();
                Text = data.ReadString(data.ReadBits<uint>(12));
            }

            public SupportTicketChatLine(long timestamp, string text)
            {
                Timestamp = timestamp;
                Text = text;
            }

            public void Read(WorldPacket data)
            {
                Timestamp = data.ReadUInt32();
                Text = data.ReadString(data.ReadBits<uint>(12));
            }
        }

        public class SupportTicketChatLog
        {
            public void Read(WorldPacket data)
            {
                uint linesCount = data.ReadUInt32();
                bool hasReportLineIndex = data.HasBit();

                data.ResetBitPos();

                for (uint i = 0; i < linesCount; i++)
                    Lines.Add(new SupportTicketChatLine(data));

                if (hasReportLineIndex)
                    ReportLineIndex = data.ReadUInt32();
            }

            public List<SupportTicketChatLine> Lines = new();
            public uint? ReportLineIndex;
        }

        public struct SupportTicketHorusChatLine
        {
            public void Read(WorldPacket data)
            {
                Timestamp = data.ReadInt64();
                AuthorGUID = data.ReadPackedGuid128();

                bool hasClubID = data.HasBit();
                bool hasChannelGUID = data.HasBit();
                bool hasRealmAddress = data.HasBit();
                bool hasSlashCmd = data.HasBit();
                uint textLength = data.ReadBits<uint>(12);

                if (hasClubID)
                    ClubID = data.ReadUInt64();

                if (hasChannelGUID)
                    ChannelGUID = data.ReadPackedGuid128();

                if (hasRealmAddress)
                {
                    SenderRealm senderRealm = new();
                    senderRealm.VirtualRealmAddress = data.ReadUInt32();
                    senderRealm.field_4 = data.ReadUInt16();
                    senderRealm.field_6 = data.ReadUInt8();
                    RealmAddress = senderRealm;
                }

                if (hasSlashCmd)
                    SlashCmd = data.ReadInt32();

                Text = data.ReadString(textLength);
            }

            public struct SenderRealm
            {
                public uint VirtualRealmAddress;
                public ushort field_4;
                public byte field_6;
            }

            public long Timestamp;
            public WowGuid128 AuthorGUID;
            public ulong? ClubID;
            public WowGuid128 ChannelGUID;
            public SenderRealm? RealmAddress;
            public int? SlashCmd;
            public string Text;
        }

        public class SupportTicketHorusChatLog
        {
            public List<SupportTicketHorusChatLine> Lines = new();

            public void Read(WorldPacket data)
            {
                uint linesCount = data.ReadUInt32();
                data.ResetBitPos();

                for (uint i = 0; i < linesCount; i++)
                {
                    var chatLine = new SupportTicketHorusChatLine();
                    chatLine.Read(data);
                    Lines.Add(chatLine);
                }
            }
        }

        public class SupportTicketMailInfo
        {
            public void Read(WorldPacket data)
            {
                MailID = data.ReadInt32();
                uint bodyLength = data.ReadBits<uint>(13);
                uint subjectLength = data.ReadBits<uint>(9);

                MailBody = data.ReadString(bodyLength);
                MailSubject = data.ReadString(subjectLength);
            }

            public int MailID;
            public string MailSubject;
            public string MailBody;
        }

        public class SupportTicketCalendarEventInfo
        {
            public void Read(WorldPacket data)
            {
                EventID = data.ReadUInt64();
                InviteID = data.ReadUInt64();

                EventTitle = data.ReadString(data.ReadBits<byte>(8));
            }

            public ulong EventID;
            public ulong InviteID;
            public string EventTitle;
        }

        public class SupportTicketPetInfo
        {
            public void Read(WorldPacket data)
            {
                PetID = data.ReadPackedGuid128();

                PetName = data.ReadString(data.ReadBits<byte>(8));
            }

            public WowGuid128 PetID;
            public string PetName;
        }

        public class SupportTicketGuildInfo
        {
            public void Read(WorldPacket data)
            {
                byte nameLength = data.ReadBits<byte>(8); // 7 or 8 ?
                GuildID = data.ReadPackedGuid128();

                GuildName = data.ReadString(nameLength);
            }

            public WowGuid128 GuildID;
            public string GuildName;
        }

        public class SupportTicketLFGListSearchResult
        {
            public void Read(WorldPacket data)
            {
                RideTicket = new RideTicket();
                RideTicket.Read(data);

                GroupFinderActivityID = data.ReadUInt32();
                LastTitleAuthorGuid = data.ReadPackedGuid128();
                LastDescriptionAuthorGuid = data.ReadPackedGuid128();
                LastVoiceChatAuthorGuid = data.ReadPackedGuid128();
                ListingCreatorGuid = data.ReadPackedGuid128();
                Unknown735 = data.ReadPackedGuid128();

                byte titleLength = data.ReadBits<byte>(10);
                byte descriptionLength = data.ReadBits<byte>(11);
                byte voiceChatLength = data.ReadBits<byte>(8);

                Title = data.ReadString(titleLength);
                Description = data.ReadString(descriptionLength);
                VoiceChat = data.ReadString(voiceChatLength);
            }

            public RideTicket RideTicket;
            public uint GroupFinderActivityID;
            public WowGuid128 LastTitleAuthorGuid;
            public WowGuid128 LastDescriptionAuthorGuid;
            public WowGuid128 LastVoiceChatAuthorGuid;
            public WowGuid128 ListingCreatorGuid;
            public WowGuid128 Unknown735;
            public string Title;
            public string Description;
            public string VoiceChat;
        }

        public class SupportTicketLFGListApplicant
        {
            public void Read(WorldPacket data)
            {
                RideTicket = new RideTicket();
                RideTicket.Read(data);

                Comment = data.ReadString(data.ReadBits<uint>(9));
            }

            public RideTicket RideTicket;
            public string Comment;
        }

        public class SupportTicketCommunityMessage
        {
            public bool IsPlayerUsingVoice;
        }

        public class SupportTicketClubFinderResult
        {
            public ulong ClubFinderPostingID;
            public ulong ClubID;
            public WowGuid128 ClubFinderGUID;
            public string ClubName;

            public void Read(WorldPacket data)
            {
                ClubFinderPostingID = data.ReadUInt64();
                ClubID = data.ReadUInt64();
                ClubFinderGUID = data.ReadPackedGuid128();
                ClubName = data.ReadString(data.ReadBits<uint>(12));
            }
        }

        public class SupportTicketUnused910
        {
            public string field_0;
            public WowGuid128 field_104;

            public void Read(WorldPacket data)
            {
                uint field_0Length = data.ReadBits<uint>(7);
                field_104 = data.ReadPackedGuid128();
                field_0 = data.ReadString(field_0Length);
            }
        }
    }

    //Structs
    public struct SupportTicketHeader
    {
        public void Read(WorldPacket packet)
        {
            MapID = packet.ReadUInt32();
            Position = packet.ReadVector3();
            Facing = packet.ReadFloat();
        }

        public uint MapID;
        public Vector3 Position;
        public float Facing;
    }
}
