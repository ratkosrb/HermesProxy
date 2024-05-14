﻿using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Generic;
using static HermesProxy.World.Server.Packets.QueryGuildInfoResponse.GuildInfo;

namespace HermesProxy.World.Client
{
    public partial class WorldClient
    {
        // Handlers for SMSG opcodes coming the legacy world server
        [PacketHandler(Opcode.SMSG_GUILD_COMMAND_RESULT)]
        void HandleGuildCommandResult(WorldPacket packet)
        {
            GuildCommandResult result = new();
            result.Command = (GuildCommandType)packet.ReadUInt32();
            result.Name = packet.ReadCString();
            result.Result = (GuildCommandError)packet.ReadUInt32();
            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.SMSG_GUILD_EVENT)]
        void HandleGuildEvent(WorldPacket packet)
        {
            GuildEventType eventType = (GuildEventType)packet.ReadUInt8();

            var size = packet.ReadUInt8();
            string[] strings = new string[size];
            for (var i = 0; i < size; i++)
                strings[i] = packet.ReadCString();

            WowGuid128 guid = WowGuid128.Empty;
            if (packet.CanRead())
                guid = packet.ReadGuid().To128(GetSession().GameState);

            switch (eventType)
            {
                case GuildEventType.Promotion:
                case GuildEventType.Demotion:
                {
                    WowGuid128 officer = GetSession().GameState.GetPlayerGuidByName(strings[0]);
                    WowGuid128 player = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                    uint rankId = GetSession().GetGuildRankIdByName(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), strings[2]);
                    if (officer != null && player != null)
                    {
                        GuildSendRankChange promote = new GuildSendRankChange();
                        promote.Officer = officer;
                        promote.Other = player;
                        promote.Promote = eventType == GuildEventType.Promotion;
                        promote.RankID = rankId;
                        SendPacketToClient(promote);
                    }
                    break;
                }
                case GuildEventType.MOTD:
                {
                    GuildEventMotd motd = new GuildEventMotd();
                    motd.MotdText = strings[0];
                    SendPacketToClient(motd);
                    break;
                }
                case GuildEventType.PlayerJoined:
                {
                    GuildEventPlayerJoined joined = new GuildEventPlayerJoined();
                    joined.Guid = guid;
                    joined.VirtualRealmAddress = GetSession().RealmId.GetAddress();
                    joined.Name = strings[0];
                    SendPacketToClient(joined);
                    break;
                }
                case GuildEventType.PlayerLeft:
                {
                    GuildEventPlayerLeft left = new GuildEventPlayerLeft();
                    left.Removed = false;
                    left.LeaverGUID = guid;
                    left.LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress();
                    left.LeaverName = strings[0];
                    SendPacketToClient(left);
                    break;
                }
                case GuildEventType.PlayerRemoved:
                {
                    GuildEventPlayerLeft removed = new GuildEventPlayerLeft();
                    removed.Removed = true;
                    removed.LeaverGUID = guid;
                    removed.LeaverVirtualRealmAddress = GetSession().RealmId.GetAddress();
                    removed.LeaverName = strings[0];
                    removed.RemoverGUID = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                    removed.RemoverVirtualRealmAddress = GetSession().RealmId.GetAddress();
                    removed.RemoverName = strings[1];
                    SendPacketToClient(removed);
                    break;
                }
                case GuildEventType.LeaderIs:
                {
                    break;
                }
                case GuildEventType.LeaderChanged:
                {
                    WowGuid128 oldLeader = GetSession().GameState.GetPlayerGuidByName(strings[0]);
                    WowGuid128 newLeader = GetSession().GameState.GetPlayerGuidByName(strings[1]);
                    if (oldLeader != null && newLeader != null)
                    {
                        GuildEventNewLeader leader = new GuildEventNewLeader();
                        leader.OldLeaderGUID = oldLeader;
                        leader.OldLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress();
                        leader.OldLeaderName = strings[0];
                        leader.NewLeaderGUID = newLeader;
                        leader.NewLeaderVirtualRealmAddress = GetSession().RealmId.GetAddress();
                        leader.NewLeaderName = strings[1];
                        SendPacketToClient(leader);
                    }
                    break;
                }
                case GuildEventType.Disbanded:
                {
                    GuildEventDisbanded disband = new GuildEventDisbanded();
                    SendPacketToClient(disband);
                    break;
                }
                case GuildEventType.TabardChange:
                {
                    break;
                }
                case GuildEventType.RankUpdated:
                {
                    GuildEventRanksUpdated ranks = new GuildEventRanksUpdated();
                    SendPacketToClient(ranks);
                    break;
                }
                case GuildEventType.Unk11:
                {
                    break;
                }
                case GuildEventType.PlayerSignedOn:
                case GuildEventType.PlayerSignedOff:
                {
                    GuildEventPresenceChange presence = new GuildEventPresenceChange();
                    presence.Guid = guid;
                    presence.VirtualRealmAddress = GetSession().RealmId.GetAddress();
                    presence.LoggedOn = eventType == GuildEventType.PlayerSignedOn;
                    presence.Name = strings[0];
                    SendPacketToClient(presence);
                    break;
                }
                case GuildEventType.BankBagSlotsChanged:
                {
                    break;
                }
                case GuildEventType.BankTabPurchased:
                {
                    GuildEventTabAdded tab = new GuildEventTabAdded();
                    SendPacketToClient(tab);
                    break;
                }
                case GuildEventType.BankTabUpdated:
                {
                    GuildEventTabModified tab = new GuildEventTabModified();
                    tab.Name = strings[0];
                    tab.Icon = strings[1];
                    SendPacketToClient(tab);
                    break;
                }
                case GuildEventType.BankMoneyUpdate:
                {
                    GuildEventBankMoneyChanged money = new GuildEventBankMoneyChanged();
                    money.Money = (ulong)Int32.Parse(strings[0], System.Globalization.NumberStyles.HexNumber);
                    SendPacketToClient(money);
                    break;
                }
                case GuildEventType.BankMoneyWithdraw:
                {
                    break;
                }
                case GuildEventType.BankTextChanged:
                {
                    GuildEventTabTextChanged tab = new GuildEventTabTextChanged();
                    SendPacketToClient(tab);
                    break;
                }
            }
        }

        [PacketHandler(Opcode.SMSG_QUERY_GUILD_INFO_RESPONSE)]
        void HandleQueryGuildInfoResponse(WorldPacket packet)
        {
            QueryGuildInfoResponse guild = new();
            uint guildId = packet.ReadUInt32();
            guild.GuildGUID = WowGuid128.Create(HighGuidType703.Guild, guildId);
            guild.PlayerGuid = GetSession().GameState.LastGuildQueryGuid != null ? GetSession().GameState.LastGuildQueryGuid : GetSession().GameState.CurrentPlayerGuid;
            guild.HasGuildInfo = true;
            guild.Info = new QueryGuildInfoResponse.GuildInfo();
            guild.Info.GuildGuid = guild.GuildGUID;
            guild.Info.VirtualRealmAddress = GetSession().RealmId.GetAddress();

            guild.Info.GuildName = packet.ReadCString();
            GetSession().StoreGuildGuidAndName(guild.GuildGUID, guild.Info.GuildName);

            List<string> ranks = new List<string>();
            for (uint i = 0; i < 10; i++)
            {
                string rankName = packet.ReadCString();
                if (!String.IsNullOrEmpty(rankName))
                {
                    RankInfo rank = new RankInfo();
                    rank.RankID = i;
                    rank.RankOrder = i;
                    rank.RankName = rankName;
                    ranks.Add(rankName);
                    guild.Info.Ranks.Add(rank);
                }
            }
            GetSession().StoreGuildRankNames(guildId, ranks);

            guild.Info.EmblemStyle = packet.ReadUInt32();
            guild.Info.EmblemColor = packet.ReadUInt32();
            guild.Info.BorderStyle = packet.ReadUInt32();
            guild.Info.BorderColor = packet.ReadUInt32();
            guild.Info.BackgroundColor = packet.ReadUInt32();

            SendPacketToClient(guild);
        }

        [PacketHandler(Opcode.SMSG_GUILD_INFO)]
        void HandleGuildInfo(WorldPacket packet)
        {
            packet.ReadCString(); // Guild Name

            if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                GetSession().GameState.CurrentGuildCreateTime = packet.ReadPackedTime();
            else
            {
                int day = packet.ReadInt32();
                int month = packet.ReadInt32();
                int year = packet.ReadInt32();

                DateTime date;
                try
                {
                    date = new DateTime(year, month, day);
                    GetSession().GameState.CurrentGuildCreateTime = (uint)Time.DateTimeToUnixTime(date);
                }
                catch
                {
                    Log.Print(LogType.Error, $"Invalid guild create date: {day}-{month}-{year}");
                }
            }

            packet.ReadUInt32(); // Players Count

            GetSession().GameState.CurrentGuildNumAccounts = packet.ReadUInt32();
        }

        [PacketHandler(Opcode.SMSG_GUILD_ROSTER)]
        void HandleGuildRoster(WorldPacket packet)
        {
            GuildRoster guild = new();
            var membersCount = packet.ReadUInt32();

            if (GetSession().GameState.CurrentGuildNumAccounts != 0)
                guild.NumAccounts = GetSession().GameState.CurrentGuildNumAccounts;
            else
                guild.NumAccounts = membersCount;

            guild.WelcomeText = packet.ReadCString();
            guild.InfoText = packet.ReadCString();

            if (GetSession().GameState.CurrentGuildCreateTime != 0)
                guild.CreateDate = GetSession().GameState.CurrentGuildCreateTime;
            else
                guild.CreateDate = (uint)Time.UnixTime;

            var ranksCount = packet.ReadInt32();
            if (ranksCount > 0)
            {
                GuildRanks ranks = new GuildRanks();
                for (byte i = 0; i < ranksCount; i++)
                {
                    GuildRankData rank = new GuildRankData();
                    rank.RankID = i;
                    rank.RankOrder = i;
                    rank.RankName = GetSession().GetGuildRankNameById(GetSession().GameState.GetPlayerGuildId(GetSession().GameState.CurrentPlayerGuid), i);
                    rank.Flags = packet.ReadUInt32();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
                    {
                        rank.WithdrawGoldLimit = packet.ReadInt32();

                        for (var j = 0; j < GuildConst.MaxBankTabs; j++)
                        {
                            rank.TabFlags[j] = packet.ReadUInt32();
                            rank.TabWithdrawItemLimit[j] = packet.ReadUInt32();
                        }
                    }
                    ranks.Ranks.Add(rank);
                }
                SendPacketToClient(ranks);
            }
            

            for (var i = 0; i < membersCount; i++)
            {
                GuildRosterMemberData member = new GuildRosterMemberData();
                PlayerCache cache = new PlayerCache();
                member.Guid = packet.ReadGuid().To128(GetSession().GameState);
                member.VirtualRealmAddress = GetSession().RealmId.GetAddress();
                member.Status = packet.ReadUInt8();
                member.Name = cache.Name = packet.ReadCString();
                member.RankID = packet.ReadInt32();
                member.Level = cache.Level = packet.ReadUInt8();
                member.ClassID = cache.ClassId =(Class)packet.ReadUInt8();
                if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
                    member.SexID = cache.SexId = (Gender)packet.ReadUInt8();
                GetSession().GameState.UpdatePlayerCache(member.Guid, cache);
                member.AreaID = packet.ReadInt32();

                if (member.Status == 0)
                    member.LastSave = packet.ReadFloat();
                else
                    member.Authenticated = true;

                member.Note = packet.ReadCString();
                member.OfficerNote = packet.ReadCString();
                guild.MemberData.Add(member);
            }
            SendPacketToClient(guild);
        }

        [PacketHandler(Opcode.SMSG_GUILD_INVITE)]
        void HandleGuildInvite(WorldPacket packet)
        {
            GuildInvite invite = new();
            invite.InviterName = packet.ReadCString();
            invite.InviterVirtualRealmAddress = GetSession().RealmId.GetAddress();
            invite.GuildName = packet.ReadCString();
            invite.GuildVirtualRealmAddress = GetSession().RealmId.GetAddress();
            invite.GuildGUID = GetSession().GetGuildGuid(invite.GuildName);
            SendPacketToClient(invite);
        }

        [PacketHandler(Opcode.MSG_TABARDVENDOR_ACTIVATE)]
        void HandleTabardVendorActivate(WorldPacket packet)
        {
            PlayerTabardVendorActivate activate = new();
            activate.DesignerGUID = packet.ReadGuid().To128(GetSession().GameState);
            SendPacketToClient(activate);
        }

        [PacketHandler(Opcode.MSG_SAVE_GUILD_EMBLEM)]
        void HandleSaveGuildEmblem(WorldPacket packet)
        {
            PlayerSaveGuildEmblem emblem = new();
            emblem.Error = (GuildEmblemError)packet.ReadUInt32();
            SendPacketToClient(emblem);
        }

        [PacketHandler(Opcode.SMSG_GUILD_INVITE_DECLINED)]
        void HandleGuildInviteDeclined(WorldPacket packet)
        {
            GuildInviteDeclined invite = new();
            invite.InviterName = packet.ReadCString();
            invite.InviterVirtualRealmAddress = GetSession().RealmId.GetAddress();
            SendPacketToClient(invite);
        }

        [PacketHandler(Opcode.SMSG_GUILD_BANK_QUERY_RESULTS)]
        void HandleGuildBankQueryResults(WorldPacket packet)
        {
            GuildBankQueryResults result = new();
            result.Money = packet.ReadUInt64();
            result.Tab = packet.ReadUInt8();
            result.WithdrawalsRemaining = packet.ReadInt32();

            bool hasTabs = false;
            if (packet.ReadBool() && result.Tab == 0)
            {
                hasTabs = true;
                var size = packet.ReadUInt8();
                for (var i = 0; i < size; i++)
                {
                    GuildBankTabInfo tabInfo = new GuildBankTabInfo();
                    tabInfo.TabIndex = i;
                    tabInfo.Name = packet.ReadCString();
                    tabInfo.Icon = packet.ReadCString();
                    result.TabInfo.Add(tabInfo);
                }
            }

            var slots = packet.ReadUInt8();
            for (var i = 0; i < slots; i++)
            {
                GuildBankItemInfo itemInfo = new GuildBankItemInfo();
                itemInfo.Slot = packet.ReadUInt8();
                int entry = packet.ReadInt32();
                if (entry > 0)
                {
                    itemInfo.Item.ItemID = (uint)entry;
                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_3_0_10958))
                        itemInfo.Flags = packet.ReadUInt32();

                    itemInfo.Item.RandomPropertiesID = packet.ReadUInt32();
                    if (itemInfo.Item.RandomPropertiesID != 0)
                        itemInfo.Item.RandomPropertiesSeed = packet.ReadUInt32();

                    if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                        itemInfo.Count = packet.ReadInt32();
                    else
                        itemInfo.Count = packet.ReadUInt8();

                    itemInfo.EnchantmentID = packet.ReadInt32();
                    itemInfo.Charges = packet.ReadUInt8();

                    var enchantments = packet.ReadUInt8();
                    for (var j = 0; j < enchantments; j++)
                    {
                        byte slot = packet.ReadUInt8();
                        uint enchantId = packet.ReadUInt32();
                        if (enchantId != 0)
                        {
                            uint itemId = GameData.GetGemFromEnchantId(enchantId);
                            if (itemId != 0)
                            {
                                ItemGemData gem = new ItemGemData();
                                gem.Slot = slot;
                                gem.Item.ItemID = itemId;
                                itemInfo.SocketEnchant.Add(gem);
                            }
                        }
                    } 
                }
                result.ItemInfo.Add(itemInfo);
            }

            result.FullUpdate = (hasTabs && slots > 0);

            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.MSG_QUERY_GUILD_BANK_TEXT)]
        void HandleQueryGuildBankText(WorldPacket packet)
        {
            GuildBankTextQueryResult result = new();
            result.Tab = packet.ReadUInt8();
            result.Text = packet.ReadCString();
            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.MSG_GUILD_BANK_LOG_QUERY)]
        void HandleGuildBankLongQuery(WorldPacket packet)
        {
            const int maxTabs = 6;

            GuildBankLogQueryResults result = new();
            result.Tab = packet.ReadUInt8();
            byte logSize = packet.ReadUInt8();
            for (byte i = 0; i < logSize; i++)
            {
                GuildBankLogEntry logEntry = new GuildBankLogEntry();
                logEntry.EntryType = packet.ReadInt8();
                logEntry.PlayerGUID = packet.ReadGuid().To128(GetSession().GameState);
                
                if (result.Tab != maxTabs)
                {
                    logEntry.ItemID = packet.ReadInt32();
                    logEntry.Count = packet.ReadUInt8();
                    if ((GuildBankEventType)logEntry.EntryType == GuildBankEventType.MoveItem ||
                        (GuildBankEventType)logEntry.EntryType == GuildBankEventType.MoveItem2)
                        logEntry.OtherTab = packet.ReadInt8();
                }
                else
                    logEntry.Money = packet.ReadUInt32();

                logEntry.TimeOffset = packet.ReadUInt32();
                result.Entry.Add(logEntry);
            }
            SendPacketToClient(result);
        }

        [PacketHandler(Opcode.MSG_GUILD_BANK_MONEY_WITHDRAWN)]
        void HandleGuildBankMoneyWithdrawn(WorldPacket packet)
        {
            GuildBankRemainingWithdrawMoney result = new();
            result.RemainingWithdrawMoney = packet.ReadUInt32();
            SendPacketToClient(result);
        }
    }
}
