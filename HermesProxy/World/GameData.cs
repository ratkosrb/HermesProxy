﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using Framework.Logging;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using Microsoft.VisualBasic.FileIO;

namespace HermesProxy.World
{
    public static class GameData
    {
        // From CSV
        public static SortedDictionary<uint, BroadcastText> BroadcastTextStore = new SortedDictionary<uint, BroadcastText>();
        public static Dictionary<uint, ItemDisplayData> ItemDisplayDataStore = new Dictionary<uint, ItemDisplayData>();
        public static Dictionary<uint, Battleground> Battlegrounds = new Dictionary<uint, Battleground>();
        public static Dictionary<uint, ChatChannel> ChatChannels = new Dictionary<uint, ChatChannel>();
        public static Dictionary<uint, Dictionary<uint, byte>> ItemEffects = new Dictionary<uint, Dictionary<uint, byte>>();
        public static Dictionary<uint, uint> ItemEnchantVisuals = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> SpellVisuals = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> LearnSpells = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> TotemSpells = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> Gems = new Dictionary<uint, uint>();
        public static Dictionary<uint, float> UnitDisplayScales = new Dictionary<uint, float>();
        public static Dictionary<uint, uint> TransportPeriods = new Dictionary<uint, uint>();
        public static Dictionary<uint, string> AreaNames = new Dictionary<uint, string>();
        public static Dictionary<uint, uint> RaceFaction = new Dictionary<uint, uint>();
        public static Dictionary<uint, uint> ItemDisplayIdToFileDataId = new Dictionary<uint, uint>();
        public static HashSet<uint> DispellSpells = new HashSet<uint>();
        public static HashSet<uint> StackableAuras = new HashSet<uint>();
        public static HashSet<uint> MountAuras = new HashSet<uint>();
        public static HashSet<uint> NextMeleeSpells = new HashSet<uint>();
        public static HashSet<uint> AutoRepeatSpells = new HashSet<uint>();
        public static HashSet<uint> AuraSpells = new HashSet<uint>();
        public static Dictionary<uint, TaxiPath> TaxiPaths = new Dictionary<uint, TaxiPath>();
        public static int[,] TaxiNodesGraph = new int[250,250];

        // From Server
        public static Dictionary<uint, ItemTemplate> ItemTemplates = new Dictionary<uint, ItemTemplate>();
        public static Dictionary<uint, CreatureTemplate> CreatureTemplates = new Dictionary<uint, CreatureTemplate>();
        public static Dictionary<uint, QuestTemplate> QuestTemplates = new Dictionary<uint, QuestTemplate>();
        public static Dictionary<uint, string> ItemNames = new Dictionary<uint, string>();

        #region GettersAndSetters
        public static void StoreItemName(uint entry, string name)
        {
            if (ItemNames.ContainsKey(entry))
                ItemNames[entry] = name;
            else
                ItemNames.Add(entry, name);
        }

        public static string GetItemName(uint entry)
        {
            string data;
            if (ItemNames.TryGetValue(entry, out data))
                return data;

            ItemTemplate template = GetItemTemplate(entry);
            if (template != null)
                return template.Name[0];

            return "";
        }

        public static void StoreItemTemplate(uint entry, ItemTemplate template)
        {
            if (ItemTemplates.ContainsKey(entry))
                ItemTemplates[entry] = template;
            else
                ItemTemplates.Add(entry, template);
        }

        public static ItemTemplate GetItemTemplate(uint entry)
        {
            ItemTemplate data;
            if (ItemTemplates.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static void StoreQuestTemplate(uint entry, QuestTemplate template)
        {
            if (QuestTemplates.ContainsKey(entry))
                QuestTemplates[entry] = template;
            else
                QuestTemplates.Add(entry, template);
        }

        public static QuestTemplate GetQuestTemplate(uint entry)
        {
            QuestTemplate data;
            if (QuestTemplates.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static QuestObjective GetQuestObjectiveForItem(uint entry)
        {
            foreach (var quest in QuestTemplates)
            {
                foreach (var objective in quest.Value.Objectives)
                {
                    if (objective.ObjectID == entry &&
                        objective.Type == QuestObjectiveType.Item)
                        return objective;
                }
            }
            return null;
        }

        public static void StoreCreatureTemplate(uint entry, CreatureTemplate template)
        {
            if (CreatureTemplates.ContainsKey(entry))
                CreatureTemplates[entry] = template;
            else
                CreatureTemplates.Add(entry, template);
        }

        public static CreatureTemplate GetCreatureTemplate(uint entry)
        {
            CreatureTemplate data;
            if (CreatureTemplates.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static ItemDisplayData GetItemDisplayData(uint entry)
        {
            ItemDisplayData data;
            if (ItemDisplayDataStore.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static uint GetItemIdWithDisplayId(uint displayId)
        {
            foreach (var item in ItemDisplayDataStore)
            {
                if (item.Value.DisplayId == displayId)
                    return item.Key;
            }
            return 0;
        }

        public static uint GetFileDataIdForItemDisplayId(uint displayId)
        {
            uint fileDataId;
            if (ItemDisplayIdToFileDataId.TryGetValue(displayId, out fileDataId))
                return fileDataId;
            return 0;
        }

        public static void SaveItemEffectSlot(uint itemId, uint spellId, byte slot)
        {
            if (ItemEffects.ContainsKey(itemId))
            {
                if (ItemEffects[itemId].ContainsKey(spellId))
                    ItemEffects[itemId][spellId] = slot;
                else
                    ItemEffects[itemId].Add(spellId, slot);
            }
            else
            {
                Dictionary<uint, byte> dict = new Dictionary<uint, byte>();
                dict.Add(spellId, slot);
                ItemEffects.Add(itemId, dict);
            }
        }

        public static byte GetItemEffectSlot(uint itemId, uint spellId)
        {
            if (ItemEffects.ContainsKey(itemId) &&
                ItemEffects[itemId].ContainsKey(spellId))
                return ItemEffects[itemId][spellId];
            return 0;
        }

        public static uint GetItemEnchantVisual(uint enchantId)
        {
            uint visualId;
            if (ItemEnchantVisuals.TryGetValue(enchantId, out visualId))
                return visualId;
            return 0;
        }

        public static uint GetSpellVisual(uint spellId)
        {
            uint visual;
            if (SpellVisuals.TryGetValue(spellId, out visual))
                return visual;
            return 0;
        }

        public static int GetTotemSlotForSpell(uint spellId)
        {
            uint slot;
            if (TotemSpells.TryGetValue(spellId, out slot))
                return (int)slot;
            return -1;
        }

        public static uint GetRealSpell(uint learnSpellId)
        {
            uint realSpellId;
            if (LearnSpells.TryGetValue(learnSpellId, out realSpellId))
                return realSpellId;
            return learnSpellId;
        }

        public static uint GetGemFromEnchantId(uint enchantId)
        {
            uint itemId;
            if (Gems.TryGetValue(enchantId, out itemId))
                return itemId;
            return 0;
        }

        public static uint GetEnchantIdFromGem(uint itemId)
        {
            foreach (var itr in Gems)
            {
                if (itr.Value == itemId)
                    return itr.Key;
            }
            return 0;
        }

        public static float GetUnitDisplayScale(uint displayId)
        {
            float scale;
            if (UnitDisplayScales.TryGetValue(displayId, out scale))
                return scale;
            return 1.0f;
        }

        public static uint GetTransportPeriod(uint entry)
        {
            uint period;
            if (TransportPeriods.TryGetValue(entry, out period))
                return period;
            return 0;
        }

        public static string GetAreaName(uint id)
        {
            string name;
            if (AreaNames.TryGetValue(id, out name))
                return name;
            return "";
        }

        public static uint GetFactionForRace(uint race)
        {
            uint faction;
            if (RaceFaction.TryGetValue(race, out faction))
                return faction;
            return 1;
        }

        public static uint GetBattlegroundIdFromMapId(uint mapId)
        {
            foreach (var bg in Battlegrounds)
            {
                if (bg.Value.MapIds.Contains(mapId))
                    return bg.Key;
            }
            return 0;
        }

        public static uint GetMapIdFromBattlegroundId(uint bgId)
        {
            Battleground bg;
            if (Battlegrounds.TryGetValue(bgId, out bg))
                return bg.MapIds[0];
            return 0;
        }

        public static uint GetChatChannelIdFromName(string name)
        {
            foreach (var channel in ChatChannels)
            {
                if (name.Contains(channel.Value.Name))
                    return channel.Key;
            }
            return 0;
        }

        public static List<ChatChannel> GetChatChannelsWithFlags(ChannelFlags flags)
        {
            List<ChatChannel> channels = new List<ChatChannel>();
            foreach (var channel in ChatChannels)
            {
                if ((channel.Value.Flags & flags) == flags)
                    channels.Add(channel.Value);
            }
            return channels;
        }

        public static bool IsAllianceRace(Race raceId)
        {
            switch (raceId)
            {
                case Race.Human:
                case Race.Dwarf:
                case Race.NightElf:
                case Race.Gnome:
                case Race.Draenei:
                case Race.Worgen:
                    return true;
            }
            return false;
        }

        public static BroadcastText GetBroadcastText(uint entry)
        {
            BroadcastText data;
            if (BroadcastTextStore.TryGetValue(entry, out data))
                return data;
            return null;
        }

        public static uint GetBroadcastTextId(string maleText, string femaleText, uint language, ushort[] emoteDelays, ushort[] emotes)
        {
            foreach (var itr in BroadcastTextStore)
            {
                if (((!String.IsNullOrEmpty(maleText) && itr.Value.MaleText == maleText) ||
                     (!String.IsNullOrEmpty(femaleText) && itr.Value.FemaleText == femaleText)) &&
                    itr.Value.Language == language &&
                    Enumerable.SequenceEqual(itr.Value.EmoteDelays, emoteDelays) &&
                    Enumerable.SequenceEqual(itr.Value.Emotes, emotes))
                {
                    return itr.Key;
                }
            }

            BroadcastText broadcastText = new();
            broadcastText.Entry = BroadcastTextStore.Keys.Last() + 1;
            broadcastText.MaleText = maleText;
            broadcastText.FemaleText = femaleText;
            broadcastText.Language = language;
            broadcastText.EmoteDelays = emoteDelays;
            broadcastText.Emotes = emotes;
            BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
            return broadcastText.Entry;
        }
        #endregion
        #region Loading
        // Loading code
        public static void LoadEverything()
        {
            Log.Print(LogType.Storage, "Loading data files...");
            LoadBroadcastTexts();
            LoadItemTemplates();
            LoadBattlegrounds();
            LoadChatChannels();
            LoadItemEnchantVisuals();
            LoadSpellVisuals();
            LoadLearnSpells();
            LoadTotemSpells();
            LoadGems();
            LoadUnitDisplayScales();
            LoadTransports();
            LoadAreaNames();
            LoadRaceFaction();
            LoadDispellSpells();
            LoadStackableAuras();
            LoadMountAuras();
            LoadMeleeSpells();
            LoadAutoRepeatSpells();
            LoadAuraSpells();
            LoadTaxiPaths();
            LoadTaxiPathNodesGraph();
            LoadItemDisplayIdToFileDataId();
            LoadHotfixes();
            Log.Print(LogType.Storage, "Finished loading data.");
        }

        public static void LoadBroadcastTexts()
        {
            var path = Path.Combine("CSV", $"BroadcastTexts{LegacyVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    BroadcastText broadcastText = new BroadcastText();
                    broadcastText.Entry = UInt32.Parse(fields[0]);
                    broadcastText.MaleText = fields[1].TrimEnd().Replace("\0", "").Replace("~", "\n");
                    broadcastText.FemaleText = fields[2].TrimEnd().Replace("\0", "").Replace("~", "\n");
                    broadcastText.Language = UInt32.Parse(fields[3]);
                    broadcastText.Emotes[0] = UInt16.Parse(fields[4]);
                    broadcastText.Emotes[1] = UInt16.Parse(fields[5]);
                    broadcastText.Emotes[2] = UInt16.Parse(fields[6]);
                    broadcastText.EmoteDelays[0] = UInt16.Parse(fields[7]);
                    broadcastText.EmoteDelays[1] = UInt16.Parse(fields[8]);
                    broadcastText.EmoteDelays[2] = UInt16.Parse(fields[9]);
                    BroadcastTextStore.Add(broadcastText.Entry, broadcastText);
                }
            }
        }

        public static void LoadItemTemplates()
        {
            var path = Path.Combine("CSV", $"Items{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    ItemDisplayData item = new ItemDisplayData();
                    item.Entry = UInt32.Parse(fields[0]);
                    item.DisplayId = UInt32.Parse(fields[1]);
                    item.InventoryType = Byte.Parse(fields[2]);
                    ItemDisplayDataStore.Add(item.Entry, item);
                }
            }
        }

        public static void LoadBattlegrounds()
        {
            var path = Path.Combine("CSV", "Battlegrounds.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    Battleground bg = new Battleground();
                    uint bgId = UInt32.Parse(fields[0]);
                    bg.IsArena = Byte.Parse(fields[1]) != 0;
                    for (int i = 0; i < 6; i++)
                    {
                        uint mapId = UInt32.Parse(fields[2 + i]);
                        if (mapId != 0)
                            bg.MapIds.Add(mapId);
                    }
                    System.Diagnostics.Trace.Assert(bg.MapIds.Count != 0);
                    Battlegrounds.Add(bgId, bg);
                }
            }
        }

        public static void LoadChatChannels()
        {
            var path = Path.Combine("CSV", "ChatChannels.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    ChatChannel channel = new ChatChannel();
                    channel.Id = UInt32.Parse(fields[0]);
                    channel.Flags = (ChannelFlags)UInt32.Parse(fields[1]);
                    channel.Name = fields[2];
                    ChatChannels.Add(channel.Id, channel);
                }
            }
        }

        public static void LoadItemEnchantVisuals()
        {
            var path = Path.Combine("CSV", $"ItemEnchantVisuals{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint enchantId = UInt32.Parse(fields[0]);
                    uint visualId = UInt32.Parse(fields[1]);
                    ItemEnchantVisuals.Add(enchantId, visualId);
                }
            }
        }

        public static void LoadSpellVisuals()
        {
            var path = Path.Combine("CSV", $"SpellVisuals{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    uint visualId = UInt32.Parse(fields[1]);
                    SpellVisuals.Add(spellId, visualId);
                }
            }
        }

        public static void LoadLearnSpells()
        {
            var path = Path.Combine("CSV", "LearnSpells.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint learnSpellId = UInt32.Parse(fields[0]);
                    uint realSpellId = UInt32.Parse(fields[1]);
                    if (!LearnSpells.ContainsKey(learnSpellId))
                        LearnSpells.Add(learnSpellId, realSpellId);
                }
            }
        }

        public static void LoadTotemSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"TotemSpells.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    uint totemSlot = UInt32.Parse(fields[1]);
                    TotemSpells.Add(spellId, totemSlot);
                }
            }
        }

        public static void LoadGems()
        {
            if (ModernVersion.ExpansionVersion <= 1)
                return;

            var path = Path.Combine("CSV", $"Gems{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint enchantId = UInt32.Parse(fields[0]);
                    uint itemId = UInt32.Parse(fields[1]);
                    Gems.Add(enchantId, itemId);
                }
            }
        }

        public static void LoadUnitDisplayScales()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", "UnitDisplayScales.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint displayId = UInt32.Parse(fields[0]);
                    float scale = Single.Parse(fields[1]);
                    UnitDisplayScales.Add(displayId, scale);
                }
            }
        }

        public static void LoadTransports()
        {
            var path = Path.Combine("CSV", $"Transports{LegacyVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint entry = UInt32.Parse(fields[0]);
                    uint period = UInt32.Parse(fields[1]);
                    TransportPeriods.Add(entry, period);
                }
            }
        }

        public static void LoadAreaNames()
        {
            var path = Path.Combine("CSV", $"AreaNames.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    string name = fields[1];
                    AreaNames.Add(id, name);
                }
            }
        }

        public static void LoadRaceFaction()
        {
            var path = Path.Combine("CSV", $"RaceFaction.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    uint faction = UInt32.Parse(fields[1]);
                    RaceFaction.Add(id, faction);
                }
            }
        }

        public static void LoadDispellSpells()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", "DispellSpells.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    DispellSpells.Add(spellId);
                }
            }
        }

        public static void LoadStackableAuras()
        {
            if (LegacyVersion.ExpansionVersion > 2)
                return;

            var path = Path.Combine("CSV", $"StackableAuras{LegacyVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    StackableAuras.Add(spellId);
                }
            }
        }

        public static void LoadMountAuras()
        {
            if (LegacyVersion.ExpansionVersion > 1)
                return;

            var path = Path.Combine("CSV", $"MountAuras.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    MountAuras.Add(spellId);
                }
            }
        }

        public static void LoadMeleeSpells()
        {
            var path = Path.Combine("CSV", $"MeleeSpells{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    NextMeleeSpells.Add(spellId);
                }
            }
        }

        public static void LoadAutoRepeatSpells()
        {
            var path = Path.Combine("CSV", $"AutoRepeatSpells{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    AutoRepeatSpells.Add(spellId);
                }
            }
        }
        public static void LoadAuraSpells()
        {
            var path = Path.Combine("CSV", $"AuraSpells{LegacyVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint spellId = UInt32.Parse(fields[0]);
                    AuraSpells.Add(spellId);
                }
            }
        }
        public static void LoadTaxiPaths()
        {
            var path = Path.Combine("CSV", $"TaxiPath{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    TaxiPath taxiPath = new TaxiPath();
                    taxiPath.Id = UInt32.Parse(fields[0]);
                    taxiPath.From = UInt32.Parse(fields[1]);
                    taxiPath.To = UInt32.Parse(fields[2]);
                    taxiPath.Cost = Int32.Parse(fields[3]);
                    TaxiPaths.Add(counter, taxiPath);
                    counter++;
                }
            }
        }
        public static void LoadTaxiPathNodesGraph()
        {
            // Load TaxiNodes (used in calculating first and last parts of path)
            Dictionary<uint, TaxiNode> TaxiNodes = new Dictionary<uint, TaxiNode>();
            var pathNodes = Path.Combine("CSV", $"TaxiNodes{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(pathNodes))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    TaxiNode taxiNode = new TaxiNode();
                    taxiNode.Id = UInt32.Parse(fields[0]);
                    taxiNode.mapId = UInt32.Parse(fields[1]);
                    taxiNode.x = float.Parse(fields[2]);
                    taxiNode.y = float.Parse(fields[3]);
                    taxiNode.z = float.Parse(fields[4]);
                    TaxiNodes.Add(taxiNode.Id, taxiNode);
                }
            }
            // Load TaxiPathNode (used in calculating rest of path)
            Dictionary<uint, TaxiPathNode> TaxiPathNodes = new Dictionary<uint, TaxiPathNode>();
            var pathPathNodes = Path.Combine("CSV", $"TaxiPathNode{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(pathPathNodes))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    TaxiPathNode taxiPathNode = new TaxiPathNode();
                    taxiPathNode.Id = UInt32.Parse(fields[0]);
                    taxiPathNode.pathId = UInt32.Parse(fields[1]);
                    taxiPathNode.nodeIndex = UInt32.Parse(fields[2]);
                    taxiPathNode.mapId = UInt32.Parse(fields[3]);
                    taxiPathNode.x = float.Parse(fields[4]);
                    taxiPathNode.y = float.Parse(fields[5]);
                    taxiPathNode.z = float.Parse(fields[6]);
                    taxiPathNode.flags = UInt32.Parse(fields[7]);
                    taxiPathNode.delay = UInt32.Parse(fields[8]);
                    TaxiPathNodes.Add(taxiPathNode.Id, taxiPathNode);
                }
            }
            // calculate distances between nodes
            for (uint i = 0; i < TaxiPaths.Count; i++)
            {
                if (TaxiPaths.ContainsKey(i))
                {
                    float dist = 0.0f;
                    TaxiPath taxiPath = TaxiPaths[i];
                    TaxiNode nodeFrom = TaxiNodes[TaxiPaths[i].From];
                    TaxiNode nodeTo = TaxiNodes[TaxiPaths[i].To];

                    if (nodeFrom.x == 0 && nodeFrom.x == 0 && nodeFrom.z == 0)
                        continue;
                    if (nodeTo.x == 0 && nodeTo.x == 0 && nodeTo.z == 0)
                        continue;

                    // save all node ids of this path
                    HashSet<uint> pathNodeList = new HashSet<uint>();
                    foreach (var itr in TaxiPathNodes)
                    {
                        TaxiPathNode pNode = itr.Value;
                        if (pNode.pathId != taxiPath.Id)
                            continue;
                        pathNodeList.Add(pNode.Id);
                    }
                    // sort ids by node index
                    IEnumerable<uint> query = pathNodeList.OrderBy(node => TaxiPathNodes[node].nodeIndex);
                    uint curNode = 0;
                    foreach (var itr in query)
                    {
                        TaxiPathNode pNode = TaxiPathNodes[itr];
                        // calculate distance from start node
                        if (pNode.nodeIndex == 0)
                        {
                            dist += (float)Math.Sqrt(Math.Pow(nodeFrom.x - pNode.x, 2) + Math.Pow(nodeFrom.y - pNode.y, 2));
                            continue;
                        }
                        // set previous node
                        if (curNode == 0)
                        {
                            curNode = pNode.Id;
                            continue;
                        }
                        // calculate distance to previous node
                        if (curNode != 0)
                        {
                            TaxiPathNode prevNode = TaxiPathNodes[curNode];
                            curNode = pNode.Id;
                            if (prevNode.mapId != pNode.mapId)
                                continue;

                            dist += (float)Math.Sqrt(Math.Pow(prevNode.x - pNode.x, 2) + Math.Pow(prevNode.y - pNode.y, 2));
                        }
                    }
                    // calculate distance to last node
                    if (curNode != 0) // should not happen
                    {
                        TaxiPathNode lastNode = TaxiPathNodes[curNode];
                        dist += (float)Math.Sqrt(Math.Pow(nodeTo.x - lastNode.x, 2) + Math.Pow(nodeTo.y - lastNode.y, 2));
                    }
                    TaxiNodesGraph[TaxiPaths[i].From, TaxiPaths[i].To] = dist > 0 ? (int)dist : 0;
                }
            }
        }

        public static void LoadItemDisplayIdToFileDataId()
        {
            var path = Path.Combine("CSV", $"ItemDisplayIdToFileDataId.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint displayId = UInt32.Parse(fields[0]);
                    uint fileDataId = UInt32.Parse(fields[1]);
                    ItemDisplayIdToFileDataId.Add(displayId, fileDataId);
                }
            }
        }
        #endregion
        #region HotFixes
        // Stores
        public const uint HotfixAreaTriggerBegin = 100000;
        public const uint HotfixSkillLineBegin = 110000;
        public const uint HotfixSkillRaceClassInfoBegin = 120000;
        public const uint HotfixSkillLineAbilityBegin = 130000;
        public const uint HotfixSpellBegin = 140000;
        public const uint HotfixSpellNameBegin = 150000;
        public const uint HotfixSpellLevelsBegin = 160000;
        public const uint HotfixSpellAuraOptionsBegin = 170000;
        public const uint HotfixSpellMiscBegin = 180000;
        public const uint HotfixSpellEffectBegin = 190000;
        public const uint HotfixSpellXSpellVisualBegin = 200000;
        public const uint HotfixItemBegin = 210000;
        public const uint HotfixItemSparseBegin = 220000;
        public const uint HotfixCreatureDisplayInfoBegin = 230000;
        public const uint HotfixCreatureDisplayInfoExtraBegin = 240000;
        public const uint HotfixCreatureDisplayInfoOptionBegin = 250000;
        public static Dictionary<uint, HotfixRecord> Hotfixes = new Dictionary<uint, HotfixRecord>();
        public static void LoadHotfixes()
        {
            LoadAreaTriggerHotfixes();
            LoadSkillLineHotfixes();
            LoadSkillRaceClassInfoHotfixes();
            LoadSkillLineAbilityHotfixes();
            LoadSpellHotfixes();
            LoadSpellNameHotfixes();
            LoadSpellLevelsHotfixes();
            LoadSpellAuraOptionsHotfixes();
            LoadSpellMiscHotfixes();
            LoadSpellEffectHotfixes();
            LoadSpellXSpellVisualHotfixes();
            LoadItemSparseHotfixes();
            LoadCreatureDisplayInfoHotfixes();
            LoadCreatureDisplayInfoExtraHotfixes();
            LoadCreatureDisplayInfoOptionHotfixes();
        }
        
        public static void LoadAreaTriggerHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"AreaTrigger{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    AreaTrigger at = new AreaTrigger();
                    at.Message = fields[0];
                    at.PositionX = float.Parse(fields[1]);
                    at.PositionY = float.Parse(fields[2]);
                    at.PositionZ = float.Parse(fields[3]);
                    at.Id = UInt32.Parse(fields[4]);
                    at.MapId = UInt16.Parse(fields[5]);
                    at.PhaseUseFlags = Byte.Parse(fields[6]);
                    at.PhaseId = UInt16.Parse(fields[7]);
                    at.PhaseGroupId = UInt16.Parse(fields[8]);
                    at.Radius = float.Parse(fields[9]);
                    at.BoxLength = float.Parse(fields[10]);
                    at.BoxWidth = float.Parse(fields[11]);
                    at.BoxHeight = float.Parse(fields[12]);
                    at.BoxYaw = float.Parse(fields[13]);
                    at.ShapeType = Byte.Parse(fields[14]);
                    at.ShapeId = UInt16.Parse(fields[15]);
                    at.ActionSetId = UInt16.Parse(fields[16]);
                    at.Flags = Byte.Parse(fields[17]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.AreaTrigger;
                    record.HotfixId = HotfixAreaTriggerBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = at.Id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(at.Message);
                    record.HotfixContent.WriteFloat(at.PositionX);
                    record.HotfixContent.WriteFloat(at.PositionY);
                    record.HotfixContent.WriteFloat(at.PositionZ);
                    record.HotfixContent.WriteUInt32(at.Id);
                    record.HotfixContent.WriteUInt16(at.MapId);
                    record.HotfixContent.WriteUInt8(at.PhaseUseFlags);
                    record.HotfixContent.WriteUInt16(at.PhaseId);
                    record.HotfixContent.WriteUInt16(at.PhaseGroupId);
                    record.HotfixContent.WriteFloat(at.Radius);
                    record.HotfixContent.WriteFloat(at.BoxLength);
                    record.HotfixContent.WriteFloat(at.BoxWidth);
                    record.HotfixContent.WriteFloat(at.BoxHeight);
                    record.HotfixContent.WriteFloat(at.BoxYaw);
                    record.HotfixContent.WriteUInt8(at.ShapeType);
                    record.HotfixContent.WriteUInt16(at.ShapeId);
                    record.HotfixContent.WriteUInt16(at.ActionSetId);
                    record.HotfixContent.WriteUInt8(at.Flags);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSkillLineHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillLine{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    string displayName = fields[0];
                    string alternateVerb = fields[1];
                    string description = fields[2];
                    string hordeDisplayName = fields[3];
                    string neutralDisplayName = fields[4];
                    uint id = UInt32.Parse(fields[5]);
                    byte categoryID = Byte.Parse(fields[6]);
                    uint spellIconFileID = UInt32.Parse(fields[7]);
                    byte canLink = Byte.Parse(fields[8]);
                    uint parentSkillLineID = UInt32.Parse(fields[9]);
                    uint parentTierIndex = UInt32.Parse(fields[10]);
                    ushort flags = UInt16.Parse(fields[11]);
                    uint spellBookSpellID = UInt32.Parse(fields[12]);
                    
                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SkillLine;
                    record.HotfixId = HotfixSkillLineBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(displayName);
                    record.HotfixContent.WriteCString(alternateVerb);
                    record.HotfixContent.WriteCString(description);
                    record.HotfixContent.WriteCString(hordeDisplayName);
                    record.HotfixContent.WriteCString(neutralDisplayName);
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt8(categoryID);
                    record.HotfixContent.WriteUInt32(spellIconFileID);
                    record.HotfixContent.WriteUInt8(canLink);
                    record.HotfixContent.WriteUInt32(parentSkillLineID);
                    record.HotfixContent.WriteUInt32(parentTierIndex);
                    record.HotfixContent.WriteUInt16(flags);
                    record.HotfixContent.WriteUInt32(spellBookSpellID);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSkillRaceClassInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillRaceClassInfo{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    ulong raceMask = UInt64.Parse(fields[1]);
                    ushort skillId = UInt16.Parse(fields[2]);
                    uint classMask = UInt32.Parse(fields[3]);
                    ushort flags = UInt16.Parse(fields[4]);
                    byte availability = Byte.Parse(fields[5]);
                    byte minLevel = Byte.Parse(fields[6]);
                    ushort skillTierId = UInt16.Parse(fields[7]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SkillRaceClassInfo;
                    record.HotfixId = HotfixSkillRaceClassInfoBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt64(raceMask);
                    record.HotfixContent.WriteUInt16(skillId);
                    record.HotfixContent.WriteUInt32(classMask);
                    record.HotfixContent.WriteUInt16(flags);
                    record.HotfixContent.WriteUInt8(availability);
                    record.HotfixContent.WriteUInt8(minLevel);
                    record.HotfixContent.WriteUInt16(skillTierId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSkillLineAbilityHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SkillLineAbility{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    ulong raceMask = UInt64.Parse(fields[0]);
                    uint id = UInt32.Parse(fields[1]);
                    ushort skillId = UInt16.Parse(fields[2]);
                    uint spellId = UInt32.Parse(fields[3]);
                    ushort minSkillLineRank = UInt16.Parse(fields[4]);
                    uint classMask = UInt32.Parse(fields[5]);
                    uint supercedesSpellId = UInt32.Parse(fields[6]);
                    byte acquireMethod = Byte.Parse(fields[7]);
                    ushort trivialSkillLineRankHigh = UInt16.Parse(fields[8]);
                    ushort trivialSkillLineRankLow = UInt16.Parse(fields[9]);
                    byte flags = Byte.Parse(fields[10]);
                    byte numSkillUps = Byte.Parse(fields[11]);
                    ushort uniqueBit = UInt16.Parse(fields[12]);
                    ushort tradeSkillCategoryId = UInt16.Parse(fields[13]);
                    ushort skillUpSkillLineId = UInt16.Parse(fields[14]);
                    uint characterPoints1 = UInt32.Parse(fields[15]);
                    uint characterPoints2 = UInt32.Parse(fields[16]);


                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SkillLineAbility;
                    record.HotfixId = HotfixSkillLineAbilityBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt64(raceMask);
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt16(skillId);
                    record.HotfixContent.WriteUInt32(spellId);
                    record.HotfixContent.WriteUInt16(minSkillLineRank);
                    record.HotfixContent.WriteUInt32(classMask);
                    record.HotfixContent.WriteUInt32(supercedesSpellId);
                    record.HotfixContent.WriteUInt8(acquireMethod);
                    record.HotfixContent.WriteUInt16(trivialSkillLineRankHigh);
                    record.HotfixContent.WriteUInt16(trivialSkillLineRankLow);
                    record.HotfixContent.WriteUInt8(flags);
                    record.HotfixContent.WriteUInt8(numSkillUps);
                    record.HotfixContent.WriteUInt16(uniqueBit);
                    record.HotfixContent.WriteUInt16(tradeSkillCategoryId);
                    record.HotfixContent.WriteUInt16(skillUpSkillLineId);
                    record.HotfixContent.WriteUInt32(characterPoints1);
                    record.HotfixContent.WriteUInt32(characterPoints2);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"Spell{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    string nameSubText = fields[1];
                    string description = fields[2];
                    string auraDescription = fields[3];

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.Spell;
                    record.HotfixId = HotfixSpellBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(nameSubText);
                    record.HotfixContent.WriteCString(description);
                    record.HotfixContent.WriteCString(auraDescription);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellNameHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellName{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    string name = fields[1];

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellName;
                    record.HotfixId = HotfixSpellNameBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteCString(name);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellLevelsHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellLevels{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    byte difficultyId = Byte.Parse(fields[1]);
                    ushort baseLevel = UInt16.Parse(fields[2]);
                    ushort maxLevel = UInt16.Parse(fields[3]);
                    ushort spellLevel = UInt16.Parse(fields[4]);
                    byte maxPassiveAuraLevel = Byte.Parse(fields[5]);
                    uint spellId = UInt32.Parse(fields[6]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellLevels;
                    record.HotfixId = HotfixSpellLevelsBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt16(baseLevel);
                    record.HotfixContent.WriteUInt16(maxLevel);
                    record.HotfixContent.WriteUInt16(spellLevel);
                    record.HotfixContent.WriteUInt8(maxPassiveAuraLevel);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellAuraOptionsHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellAuraOptions{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    byte difficultyId = Byte.Parse(fields[1]);
                    uint cumulatievAura = UInt32.Parse(fields[2]);
                    uint procCategoryRecovery = UInt32.Parse(fields[3]);
                    byte procChance = Byte.Parse(fields[4]);
                    uint procCharges = UInt32.Parse(fields[5]);
                    ushort spellProcsPerMinuteId = UInt16.Parse(fields[6]);
                    uint procTypeMask0 = UInt32.Parse(fields[7]);
                    uint procTypeMask1 = UInt32.Parse(fields[8]);
                    uint spellId = UInt32.Parse(fields[9]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellAuraOptions;
                    record.HotfixId = HotfixSpellAuraOptionsBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt32(cumulatievAura);
                    record.HotfixContent.WriteUInt32(procCategoryRecovery);
                    record.HotfixContent.WriteUInt8(procChance);
                    record.HotfixContent.WriteUInt32(procCharges);
                    record.HotfixContent.WriteUInt16(spellProcsPerMinuteId);
                    record.HotfixContent.WriteUInt32(procTypeMask0);
                    record.HotfixContent.WriteUInt32(procTypeMask1);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellMiscHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellMisc{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    byte difficultyId = Byte.Parse(fields[1]);
                    ushort castingTimeIndex = UInt16.Parse(fields[2]);
                    ushort durationIndex = UInt16.Parse(fields[3]);
                    ushort rangeIndex = UInt16.Parse(fields[4]);
                    byte schoolMask = Byte.Parse(fields[5]);
                    float speed = Single.Parse(fields[6]);
                    float launchDelay = Single.Parse(fields[7]);
                    float minDuration = Single.Parse(fields[8]);
                    uint spellIconFileDataId = UInt32.Parse(fields[9]);
                    uint activeIconFileDataId = UInt32.Parse(fields[10]);
                    uint attributes1 = UInt32.Parse(fields[11]);
                    uint attributes2 = UInt32.Parse(fields[12]);
                    uint attributes3 = UInt32.Parse(fields[13]);
                    uint attributes4 = UInt32.Parse(fields[14]);
                    uint attributes5 = UInt32.Parse(fields[15]);
                    uint attributes6 = UInt32.Parse(fields[16]);
                    uint attributes7 = UInt32.Parse(fields[17]);
                    uint attributes8 = UInt32.Parse(fields[18]);
                    uint attributes9 = UInt32.Parse(fields[19]);
                    uint attributes10 = UInt32.Parse(fields[20]);
                    uint attributes11 = UInt32.Parse(fields[21]);
                    uint attributes12 = UInt32.Parse(fields[22]);
                    uint attributes13 = UInt32.Parse(fields[23]);
                    uint attributes14 = UInt32.Parse(fields[24]);
                    uint spellId = UInt32.Parse(fields[25]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellMisc;
                    record.HotfixId = HotfixSpellMiscBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt16(castingTimeIndex);
                    record.HotfixContent.WriteUInt16(durationIndex);
                    record.HotfixContent.WriteUInt16(rangeIndex);
                    record.HotfixContent.WriteUInt8(schoolMask);
                    record.HotfixContent.WriteFloat(speed);
                    record.HotfixContent.WriteFloat(launchDelay);
                    record.HotfixContent.WriteFloat(minDuration);
                    record.HotfixContent.WriteUInt32(spellIconFileDataId);
                    record.HotfixContent.WriteUInt32(activeIconFileDataId);
                    record.HotfixContent.WriteUInt32(attributes1);
                    record.HotfixContent.WriteUInt32(attributes2);
                    record.HotfixContent.WriteUInt32(attributes3);
                    record.HotfixContent.WriteUInt32(attributes4);
                    record.HotfixContent.WriteUInt32(attributes5);
                    record.HotfixContent.WriteUInt32(attributes6);
                    record.HotfixContent.WriteUInt32(attributes7);
                    record.HotfixContent.WriteUInt32(attributes8);
                    record.HotfixContent.WriteUInt32(attributes9);
                    record.HotfixContent.WriteUInt32(attributes10);
                    record.HotfixContent.WriteUInt32(attributes11);
                    record.HotfixContent.WriteUInt32(attributes12);
                    record.HotfixContent.WriteUInt32(attributes13);
                    record.HotfixContent.WriteUInt32(attributes14);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellEffectHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellEffect{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    uint difficultyId = UInt32.Parse(fields[1]);
                    uint effectIndex = UInt32.Parse(fields[2]);
                    uint effect = UInt32.Parse(fields[3]);
                    float effectAmplitude = Single.Parse(fields[4]);
                    uint effectAttributes = UInt32.Parse(fields[5]);
                    short effectAura = Int16.Parse(fields[6]);
                    int effectAuraPeriod = Int32.Parse(fields[7]);
                    int effectBasePoints = Int32.Parse(fields[8]);
                    float effectBonusCoefficient = Single.Parse(fields[9]);
                    float effectChainAmplitude = Single.Parse(fields[10]);
                    int effectChainTargets = Int32.Parse(fields[11]);
                    int effectDieSides = Int32.Parse(fields[12]);
                    int effectItemType = Int32.Parse(fields[13]);
                    int effectMechanic = Int32.Parse(fields[14]);
                    float effectPointsPerResource = Single.Parse(fields[15]);
                    float effectPosFacing = Single.Parse(fields[16]);
                    float effectRealPointsPerLevel = Single.Parse(fields[17]);
                    int EffectTriggerSpell = Int32.Parse(fields[18]);
                    float bonusCoefficientFromAP = Single.Parse(fields[19]);
                    float pvpMultiplier = Single.Parse(fields[20]);
                    float coefficient = Single.Parse(fields[21]);
                    float variance = Single.Parse(fields[22]);
                    float resourceCoefficient = Single.Parse(fields[23]);
                    float groupSizeBasePointsCoefficient = Single.Parse(fields[24]);
                    int effectMiscValue1 = Int32.Parse(fields[25]);
                    int effectMiscValue2 = Int32.Parse(fields[26]);
                    uint effectRadiusIndex1 = UInt32.Parse(fields[27]);
                    uint effectRadiusIndex2 = UInt32.Parse(fields[28]);
                    int effectSpellClassMask1 = Int32.Parse(fields[29]);
                    int effectSpellClassMask2 = Int32.Parse(fields[30]);
                    int effectSpellClassMask3 = Int32.Parse(fields[31]);
                    int effectSpellClassMask4 = Int32.Parse(fields[32]);
                    short implicitTarget1 = Int16.Parse(fields[33]);
                    short implicitTarget2 = Int16.Parse(fields[34]);
                    uint spellId = UInt32.Parse(fields[35]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellEffect;
                    record.HotfixId = HotfixSpellEffectBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(difficultyId);
                    record.HotfixContent.WriteUInt32(effectIndex);
                    record.HotfixContent.WriteUInt32(effect);
                    record.HotfixContent.WriteFloat(effectAmplitude);
                    record.HotfixContent.WriteUInt32(effectAttributes);
                    record.HotfixContent.WriteInt16(effectAura);
                    record.HotfixContent.WriteInt32(effectAuraPeriod);
                    record.HotfixContent.WriteInt32(effectBasePoints);
                    record.HotfixContent.WriteFloat(effectBonusCoefficient);
                    record.HotfixContent.WriteFloat(effectChainAmplitude);
                    record.HotfixContent.WriteInt32(effectChainTargets);
                    record.HotfixContent.WriteInt32(effectDieSides);
                    record.HotfixContent.WriteInt32(effectItemType);
                    record.HotfixContent.WriteInt32(effectMechanic);
                    record.HotfixContent.WriteFloat(effectPointsPerResource);
                    record.HotfixContent.WriteFloat(effectPosFacing);
                    record.HotfixContent.WriteFloat(effectRealPointsPerLevel);
                    record.HotfixContent.WriteInt32(EffectTriggerSpell);
                    record.HotfixContent.WriteFloat(bonusCoefficientFromAP);
                    record.HotfixContent.WriteFloat(pvpMultiplier);
                    record.HotfixContent.WriteFloat(coefficient);
                    record.HotfixContent.WriteFloat(variance);
                    record.HotfixContent.WriteFloat(resourceCoefficient);
                    record.HotfixContent.WriteFloat(groupSizeBasePointsCoefficient);
                    record.HotfixContent.WriteInt32(effectMiscValue1);
                    record.HotfixContent.WriteInt32(effectMiscValue2);
                    record.HotfixContent.WriteUInt32(effectRadiusIndex1);
                    record.HotfixContent.WriteUInt32(effectRadiusIndex2);
                    record.HotfixContent.WriteInt32(effectSpellClassMask1);
                    record.HotfixContent.WriteInt32(effectSpellClassMask2);
                    record.HotfixContent.WriteInt32(effectSpellClassMask3);
                    record.HotfixContent.WriteInt32(effectSpellClassMask4);
                    record.HotfixContent.WriteInt16(implicitTarget1);
                    record.HotfixContent.WriteInt16(implicitTarget2);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadSpellXSpellVisualHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"SpellXSpellVisual{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    byte difficultyId = Byte.Parse(fields[1]);
                    uint spellVisualId = UInt32.Parse(fields[2]);
                    float probability = Single.Parse(fields[3]);
                    byte flags = Byte.Parse(fields[4]);
                    byte priority = Byte.Parse(fields[5]);
                    int spellIconFileId = Int32.Parse(fields[6]);
                    int activeIconFileId = Int32.Parse(fields[7]);
                    ushort viewerUnitConditionId = UInt16.Parse(fields[8]);
                    uint viewerPlayerConditionId = UInt32.Parse(fields[9]);
                    ushort casterUnitConditionId = UInt16.Parse(fields[10]);
                    uint casterPlayerConditionId = UInt32.Parse(fields[11]);
                    uint spellId = UInt32.Parse(fields[12]);

                    if (SpellVisuals.ContainsKey(spellId))
                        SpellVisuals[spellId] = id;
                    else
                        SpellVisuals.Add(spellId, id);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.SpellXSpellVisual;
                    record.HotfixId = HotfixSpellXSpellVisualBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt8(difficultyId);
                    record.HotfixContent.WriteUInt32(spellVisualId);
                    record.HotfixContent.WriteFloat(probability);
                    record.HotfixContent.WriteUInt8(flags);
                    record.HotfixContent.WriteUInt8(priority);
                    record.HotfixContent.WriteInt32(spellIconFileId);
                    record.HotfixContent.WriteInt32(activeIconFileId);
                    record.HotfixContent.WriteUInt16(viewerUnitConditionId);
                    record.HotfixContent.WriteUInt32(viewerPlayerConditionId);
                    record.HotfixContent.WriteUInt16(casterUnitConditionId);
                    record.HotfixContent.WriteUInt32(casterPlayerConditionId);
                    record.HotfixContent.WriteUInt32(spellId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadItemSparseHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemSparse{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    long allowableRace = Int64.Parse(fields[1]);
                    string description = fields[2];
                    string name4 = fields[3];
                    string name3 = fields[4];
                    string name2 = fields[5];
                    string name1 = fields[6];
                    float dmgVariance = Single.Parse(fields[7]);
                    uint durationInInventory = UInt32.Parse(fields[8]);
                    float qualityModifier = Single.Parse(fields[9]);
                    uint bagFamily = UInt32.Parse(fields[10]);
                    float rangeMod = Single.Parse(fields[11]);
                    float statPercentageOfSocket1 = Single.Parse(fields[12]);
                    float statPercentageOfSocket2 = Single.Parse(fields[13]);
                    float statPercentageOfSocket3 = Single.Parse(fields[14]);
                    float statPercentageOfSocket4 = Single.Parse(fields[15]);
                    float statPercentageOfSocket5 = Single.Parse(fields[16]);
                    float statPercentageOfSocket6 = Single.Parse(fields[17]);
                    float statPercentageOfSocket7 = Single.Parse(fields[18]);
                    float statPercentageOfSocket8 = Single.Parse(fields[19]);
                    float statPercentageOfSocket9 = Single.Parse(fields[20]);
                    float statPercentageOfSocket10 = Single.Parse(fields[21]);
                    int statPercentEditor1 = Int32.Parse(fields[22]);
                    int statPercentEditor2 = Int32.Parse(fields[23]);
                    int statPercentEditor3 = Int32.Parse(fields[24]);
                    int statPercentEditor4 = Int32.Parse(fields[25]);
                    int statPercentEditor5 = Int32.Parse(fields[26]);
                    int statPercentEditor6 = Int32.Parse(fields[27]);
                    int statPercentEditor7 = Int32.Parse(fields[28]);
                    int statPercentEditor8 = Int32.Parse(fields[29]);
                    int statPercentEditor9 = Int32.Parse(fields[30]);
                    int statPercentEditor10 = Int32.Parse(fields[31]);
                    int stackable = Int32.Parse(fields[32]);
                    int maxCount = Int32.Parse(fields[33]);
                    uint requiredAbility = UInt32.Parse(fields[34]);
                    uint sellPrice = UInt32.Parse(fields[35]);
                    uint buyPrice = UInt32.Parse(fields[36]);
                    uint vendorStackCount = UInt32.Parse(fields[37]);
                    float priceVariance = Single.Parse(fields[38]);
                    float priceRandomValue = Single.Parse(fields[39]);
                    int flags1 = Int32.Parse(fields[40]);
                    int flags2 = Int32.Parse(fields[41]);
                    int flags3 = Int32.Parse(fields[42]);
                    int flags4 = Int32.Parse(fields[43]);
                    int oppositeFactionItemId = Int32.Parse(fields[44]);
                    uint maxDurability = UInt32.Parse(fields[45]);
                    ushort itemNameDescriptionId = UInt16.Parse(fields[46]);
                    ushort requiredTransmogHoliday = UInt16.Parse(fields[47]);
                    ushort requiredHoliday = UInt16.Parse(fields[48]);
                    ushort limitCategory = UInt16.Parse(fields[49]);
                    ushort gemProperties = UInt16.Parse(fields[50]);
                    ushort socketMatchEnchantmentId = UInt16.Parse(fields[51]);
                    ushort totemCategoryId = UInt16.Parse(fields[52]);
                    ushort instanceBound = UInt16.Parse(fields[53]);
                    ushort zoneBound1 = UInt16.Parse(fields[54]);
                    ushort zoneBound2 = UInt16.Parse(fields[55]);
                    ushort itemSet = UInt16.Parse(fields[56]);
                    ushort lockId = UInt16.Parse(fields[57]);
                    ushort startQuestId = UInt16.Parse(fields[58]);
                    ushort pageText = UInt16.Parse(fields[59]);
                    ushort delay = UInt16.Parse(fields[60]);
                    ushort requiredReputationId = UInt16.Parse(fields[61]);
                    ushort requiredSkillRank = UInt16.Parse(fields[62]);
                    ushort requiredSkill = UInt16.Parse(fields[63]);
                    ushort itemLevel = UInt16.Parse(fields[64]);
                    short allowableClass = Int16.Parse(fields[65]);
                    ushort itemRandomSuffixGroupId = UInt16.Parse(fields[66]);
                    ushort randomProperty = UInt16.Parse(fields[67]);
                    ushort damageMin1 = UInt16.Parse(fields[68]);
                    ushort damageMin2 = UInt16.Parse(fields[69]);
                    ushort damageMin3 = UInt16.Parse(fields[70]);
                    ushort damageMin4 = UInt16.Parse(fields[71]);
                    ushort damageMin5 = UInt16.Parse(fields[72]);
                    ushort damageMax1 = UInt16.Parse(fields[73]);
                    ushort damageMax2 = UInt16.Parse(fields[74]);
                    ushort damageMax3 = UInt16.Parse(fields[75]);
                    ushort damageMax4 = UInt16.Parse(fields[76]);
                    ushort damageMax5 = UInt16.Parse(fields[77]);
                    short armor = Int16.Parse(fields[78]);
                    short holyResistance = Int16.Parse(fields[79]);
                    short fireResistance = Int16.Parse(fields[80]);
                    short natureResistance = Int16.Parse(fields[81]);
                    short frostResistance = Int16.Parse(fields[82]);
                    short shadowResistance = Int16.Parse(fields[83]);
                    short arcaneResistance = Int16.Parse(fields[84]);
                    ushort scalingStatDistributionId = UInt16.Parse(fields[85]);
                    byte expansionId = Byte.Parse(fields[86]);
                    byte artifactId = Byte.Parse(fields[87]);
                    byte spellWeight = Byte.Parse(fields[88]);
                    byte spellWeightCategory = Byte.Parse(fields[89]);
                    byte socketType1 = Byte.Parse(fields[90]);
                    byte socketType2 = Byte.Parse(fields[91]);
                    byte socketType3 = Byte.Parse(fields[92]);
                    byte sheatheType = Byte.Parse(fields[93]);
                    byte material = Byte.Parse(fields[94]);
                    byte pageMaterial = Byte.Parse(fields[95]);
                    byte pageLanguage = Byte.Parse(fields[96]);
                    byte bonding = Byte.Parse(fields[97]);
                    byte damageType = Byte.Parse(fields[98]);
                    sbyte statType1 = SByte.Parse(fields[99]);
                    sbyte statType2 = SByte.Parse(fields[100]);
                    sbyte statType3 = SByte.Parse(fields[101]);
                    sbyte statType4 = SByte.Parse(fields[102]);
                    sbyte statType5 = SByte.Parse(fields[103]);
                    sbyte statType6 = SByte.Parse(fields[104]);
                    sbyte statType7 = SByte.Parse(fields[105]);
                    sbyte statType8 = SByte.Parse(fields[106]);
                    sbyte statType9 = SByte.Parse(fields[107]);
                    sbyte statType10 = SByte.Parse(fields[108]);
                    byte containerSlots = Byte.Parse(fields[109]);
                    byte requiredReputationRank = Byte.Parse(fields[110]);
                    byte requiredCityRank = Byte.Parse(fields[111]);
                    byte requiredHonorRank = Byte.Parse(fields[112]);
                    byte inventoryType = Byte.Parse(fields[113]);
                    byte overallQualityId = Byte.Parse(fields[114]);
                    byte ammoType = Byte.Parse(fields[115]);
                    sbyte statValue1 = SByte.Parse(fields[116]);
                    sbyte statValue2 = SByte.Parse(fields[117]);
                    sbyte statValue3 = SByte.Parse(fields[118]);
                    sbyte statValue4 = SByte.Parse(fields[119]);
                    sbyte statValue5 = SByte.Parse(fields[120]);
                    sbyte statValue6 = SByte.Parse(fields[121]);
                    sbyte statValue7 = SByte.Parse(fields[122]);
                    sbyte statValue8 = SByte.Parse(fields[123]);
                    sbyte statValue9 = SByte.Parse(fields[124]);
                    sbyte statValue10 = SByte.Parse(fields[125]);
                    sbyte requiredLevel = SByte.Parse(fields[126]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.ItemSparse;
                    record.HotfixId = HotfixItemSparseBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteInt64(allowableRace);
                    record.HotfixContent.WriteCString(description);
                    record.HotfixContent.WriteCString(name4);
                    record.HotfixContent.WriteCString(name3);
                    record.HotfixContent.WriteCString(name2);
                    record.HotfixContent.WriteCString(name1);
                    record.HotfixContent.WriteFloat(dmgVariance);
                    record.HotfixContent.WriteUInt32(durationInInventory);
                    record.HotfixContent.WriteFloat(qualityModifier);
                    record.HotfixContent.WriteUInt32(bagFamily);
                    record.HotfixContent.WriteFloat(rangeMod);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket1);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket2);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket3);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket4);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket5);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket6);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket7);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket8);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket9);
                    record.HotfixContent.WriteFloat(statPercentageOfSocket10);
                    record.HotfixContent.WriteInt32(statPercentEditor1);
                    record.HotfixContent.WriteInt32(statPercentEditor2);
                    record.HotfixContent.WriteInt32(statPercentEditor3);
                    record.HotfixContent.WriteInt32(statPercentEditor4);
                    record.HotfixContent.WriteInt32(statPercentEditor5);
                    record.HotfixContent.WriteInt32(statPercentEditor6);
                    record.HotfixContent.WriteInt32(statPercentEditor7);
                    record.HotfixContent.WriteInt32(statPercentEditor8);
                    record.HotfixContent.WriteInt32(statPercentEditor9);
                    record.HotfixContent.WriteInt32(statPercentEditor10);
                    record.HotfixContent.WriteInt32(stackable);
                    record.HotfixContent.WriteInt32(maxCount);
                    record.HotfixContent.WriteUInt32(requiredAbility);
                    record.HotfixContent.WriteUInt32(sellPrice);
                    record.HotfixContent.WriteUInt32(buyPrice);
                    record.HotfixContent.WriteUInt32(vendorStackCount);
                    record.HotfixContent.WriteFloat(priceVariance);
                    record.HotfixContent.WriteFloat(priceRandomValue);
                    record.HotfixContent.WriteInt32(flags1);
                    record.HotfixContent.WriteInt32(flags2);
                    record.HotfixContent.WriteInt32(flags3);
                    record.HotfixContent.WriteInt32(flags4);
                    record.HotfixContent.WriteInt32(oppositeFactionItemId);
                    record.HotfixContent.WriteUInt32(maxDurability);
                    record.HotfixContent.WriteUInt16(itemNameDescriptionId);
                    record.HotfixContent.WriteUInt16(requiredTransmogHoliday);
                    record.HotfixContent.WriteUInt16(requiredHoliday);
                    record.HotfixContent.WriteUInt16(limitCategory);
                    record.HotfixContent.WriteUInt16(gemProperties);
                    record.HotfixContent.WriteUInt16(socketMatchEnchantmentId);
                    record.HotfixContent.WriteUInt16(totemCategoryId);
                    record.HotfixContent.WriteUInt16(instanceBound);
                    record.HotfixContent.WriteUInt16(zoneBound1);
                    record.HotfixContent.WriteUInt16(zoneBound2);
                    record.HotfixContent.WriteUInt16(itemSet);
                    record.HotfixContent.WriteUInt16(lockId);
                    record.HotfixContent.WriteUInt16(startQuestId);
                    record.HotfixContent.WriteUInt16(pageText);
                    record.HotfixContent.WriteUInt16(delay);
                    record.HotfixContent.WriteUInt16(requiredReputationId);
                    record.HotfixContent.WriteUInt16(requiredSkillRank);
                    record.HotfixContent.WriteUInt16(requiredSkill);
                    record.HotfixContent.WriteUInt16(itemLevel);
                    record.HotfixContent.WriteInt16(allowableClass);
                    record.HotfixContent.WriteUInt16(itemRandomSuffixGroupId);
                    record.HotfixContent.WriteUInt16(randomProperty);
                    record.HotfixContent.WriteUInt16(damageMin1);
                    record.HotfixContent.WriteUInt16(damageMin2);
                    record.HotfixContent.WriteUInt16(damageMin3);
                    record.HotfixContent.WriteUInt16(damageMin4);
                    record.HotfixContent.WriteUInt16(damageMin5);
                    record.HotfixContent.WriteUInt16(damageMax1);
                    record.HotfixContent.WriteUInt16(damageMax2);
                    record.HotfixContent.WriteUInt16(damageMax3);
                    record.HotfixContent.WriteUInt16(damageMax4);
                    record.HotfixContent.WriteUInt16(damageMax5);
                    record.HotfixContent.WriteInt16(armor);
                    record.HotfixContent.WriteInt16(holyResistance);
                    record.HotfixContent.WriteInt16(fireResistance);
                    record.HotfixContent.WriteInt16(natureResistance);
                    record.HotfixContent.WriteInt16(frostResistance);
                    record.HotfixContent.WriteInt16(shadowResistance);
                    record.HotfixContent.WriteInt16(arcaneResistance);
                    record.HotfixContent.WriteUInt16(scalingStatDistributionId);
                    record.HotfixContent.WriteUInt8(expansionId);
                    record.HotfixContent.WriteUInt8(artifactId);
                    record.HotfixContent.WriteUInt8(spellWeight);
                    record.HotfixContent.WriteUInt8(spellWeightCategory);
                    record.HotfixContent.WriteUInt8(socketType1);
                    record.HotfixContent.WriteUInt8(socketType2);
                    record.HotfixContent.WriteUInt8(socketType3);
                    record.HotfixContent.WriteUInt8(sheatheType);
                    record.HotfixContent.WriteUInt8(material);
                    record.HotfixContent.WriteUInt8(pageMaterial);
                    record.HotfixContent.WriteUInt8(pageLanguage);
                    record.HotfixContent.WriteUInt8(bonding);
                    record.HotfixContent.WriteUInt8(damageType);
                    record.HotfixContent.WriteInt8(statType1);
                    record.HotfixContent.WriteInt8(statType2);
                    record.HotfixContent.WriteInt8(statType3);
                    record.HotfixContent.WriteInt8(statType4);
                    record.HotfixContent.WriteInt8(statType5);
                    record.HotfixContent.WriteInt8(statType6);
                    record.HotfixContent.WriteInt8(statType7);
                    record.HotfixContent.WriteInt8(statType8);
                    record.HotfixContent.WriteInt8(statType9);
                    record.HotfixContent.WriteInt8(statType10);
                    record.HotfixContent.WriteUInt8(containerSlots);
                    record.HotfixContent.WriteUInt8(requiredReputationRank);
                    record.HotfixContent.WriteUInt8(requiredCityRank);
                    record.HotfixContent.WriteUInt8(requiredHonorRank);
                    record.HotfixContent.WriteUInt8(inventoryType);
                    record.HotfixContent.WriteUInt8(overallQualityId);
                    record.HotfixContent.WriteUInt8(ammoType);
                    record.HotfixContent.WriteInt8(statValue1);
                    record.HotfixContent.WriteInt8(statValue2);
                    record.HotfixContent.WriteInt8(statValue3);
                    record.HotfixContent.WriteInt8(statValue4);
                    record.HotfixContent.WriteInt8(statValue5);
                    record.HotfixContent.WriteInt8(statValue6);
                    record.HotfixContent.WriteInt8(statValue7);
                    record.HotfixContent.WriteInt8(statValue8);
                    record.HotfixContent.WriteInt8(statValue9);
                    record.HotfixContent.WriteInt8(statValue10);
                    record.HotfixContent.WriteInt8(requiredLevel);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }

        // For use in SMSG_DB_REPLY
        public static void WriteItemSparseHotfix(ItemTemplate item, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteInt64(item.AllowedRaces);
            buffer.WriteCString(item.Description);
            buffer.WriteCString(item.Name[3]);
            buffer.WriteCString(item.Name[2]);
            buffer.WriteCString(item.Name[1]);
            buffer.WriteCString(item.Name[0]);
            buffer.WriteFloat(1);
            buffer.WriteUInt32(item.Duration);
            buffer.WriteFloat(0);
            buffer.WriteUInt32(item.BagFamily);
            buffer.WriteFloat(item.RangedMod);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteFloat(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(item.MaxStackSize);
            buffer.WriteInt32(item.MaxCount);
            buffer.WriteUInt32(item.RequiredSpell);
            buffer.WriteUInt32(item.SellPrice);
            buffer.WriteUInt32(item.BuyPrice);
            buffer.WriteUInt32(item.BuyCount);
            buffer.WriteFloat(1);
            buffer.WriteFloat(1);
            buffer.WriteUInt32(item.Flags);
            buffer.WriteUInt32(item.FlagsExtra);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteInt32(0);
            buffer.WriteUInt32(item.MaxDurability);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16((ushort)item.HolidayID);
            buffer.WriteUInt16((ushort)item.ItemLimitCategory);
            buffer.WriteUInt16((ushort)item.GemProperties);
            buffer.WriteUInt16((ushort)item.SocketBonus);
            buffer.WriteUInt16((ushort)item.TotemCategory);
            buffer.WriteUInt16((ushort)item.MapID);
            buffer.WriteUInt16((ushort)item.AreaID);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16((ushort)item.ItemSet);
            buffer.WriteUInt16((ushort)item.LockId);
            buffer.WriteUInt16((ushort)item.StartQuestId);
            buffer.WriteUInt16((ushort)item.PageText);
            buffer.WriteUInt16((ushort)item.Delay);
            buffer.WriteUInt16((ushort)item.RequiredRepFaction);
            buffer.WriteUInt16((ushort)item.RequiredSkillLevel);
            buffer.WriteUInt16((ushort)item.RequiredSkillId);
            buffer.WriteUInt16((ushort)item.ItemLevel);
            buffer.WriteInt16((short)item.AllowedClasses);
            buffer.WriteUInt16((ushort)item.RandomSuffix);
            buffer.WriteUInt16((ushort)item.RandomProperty);
            buffer.WriteUInt16((ushort)item.DamageMins[0]);
            buffer.WriteUInt16((ushort)item.DamageMins[1]);
            buffer.WriteUInt16((ushort)item.DamageMins[2]);
            buffer.WriteUInt16((ushort)item.DamageMins[3]);
            buffer.WriteUInt16((ushort)item.DamageMins[4]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
            buffer.WriteInt16((short)item.Armor);
            buffer.WriteInt16((short)item.HolyResistance);
            buffer.WriteInt16((short)item.FireResistance);
            buffer.WriteInt16((short)item.NatureResistance);
            buffer.WriteInt16((short)item.FrostResistance);
            buffer.WriteInt16((short)item.ShadowResistance);
            buffer.WriteInt16((short)item.ArcaneResistance);
            buffer.WriteUInt16((ushort)item.ScalingStatDistribution);
            buffer.WriteUInt8(254);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8(0);
            buffer.WriteUInt8((byte)item.ItemSocketColors[0]);
            buffer.WriteUInt8((byte)item.ItemSocketColors[1]);
            buffer.WriteUInt8((byte)item.ItemSocketColors[2]);
            buffer.WriteUInt8((byte)item.SheathType);
            buffer.WriteUInt8((byte)item.Material);
            buffer.WriteUInt8((byte)item.PageMaterial);
            buffer.WriteUInt8((byte)item.Language);
            buffer.WriteUInt8((byte)item.Bonding);
            buffer.WriteUInt8((byte)item.DamageTypes[0]);
            buffer.WriteInt8((sbyte)item.StatTypes[0]);
            buffer.WriteInt8((sbyte)item.StatTypes[1]);
            buffer.WriteInt8((sbyte)item.StatTypes[2]);
            buffer.WriteInt8((sbyte)item.StatTypes[3]);
            buffer.WriteInt8((sbyte)item.StatTypes[4]);
            buffer.WriteInt8((sbyte)item.StatTypes[5]);
            buffer.WriteInt8((sbyte)item.StatTypes[6]);
            buffer.WriteInt8((sbyte)item.StatTypes[7]);
            buffer.WriteInt8((sbyte)item.StatTypes[8]);
            buffer.WriteInt8((sbyte)item.StatTypes[9]);
            buffer.WriteUInt8((byte)item.ContainerSlots);
            buffer.WriteUInt8((byte)item.RequiredRepValue);
            buffer.WriteUInt8((byte)item.RequiredCityRank);
            buffer.WriteUInt8((byte)item.RequiredHonorRank);
            buffer.WriteUInt8((byte)item.InventoryType);
            buffer.WriteUInt8((byte)item.Quality);
            buffer.WriteUInt8((byte)item.AmmoType);
            buffer.WriteInt8((sbyte)item.StatValues[0]);
            buffer.WriteInt8((sbyte)item.StatValues[1]);
            buffer.WriteInt8((sbyte)item.StatValues[2]);
            buffer.WriteInt8((sbyte)item.StatValues[3]);
            buffer.WriteInt8((sbyte)item.StatValues[4]);
            buffer.WriteInt8((sbyte)item.StatValues[5]);
            buffer.WriteInt8((sbyte)item.StatValues[6]);
            buffer.WriteInt8((sbyte)item.StatValues[7]);
            buffer.WriteInt8((sbyte)item.StatValues[8]);
            buffer.WriteInt8((sbyte)item.StatValues[9]);
            buffer.WriteInt8((sbyte)item.RequiredLevel);
        }

        public static void LoadItemHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"ItemSparse{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    byte classId = Byte.Parse(fields[1]);
                    byte subclassId = Byte.Parse(fields[2]);
                    byte material = Byte.Parse(fields[3]);
                    sbyte inventoryType = SByte.Parse(fields[4]);
                    int requiredLevel = Int32.Parse(fields[5]);
                    byte sheatheType = Byte.Parse(fields[6]);
                    ushort randomSelect = UInt16.Parse(fields[7]);
                    ushort itemRandomSuffixGroupId = UInt16.Parse(fields[8]);
                    sbyte soundOverrideSubclassId = SByte.Parse(fields[9]);
                    ushort scalingStatDistributionId = UInt16.Parse(fields[10]);
                    int iconFileDataId = Int32.Parse(fields[11]);
                    byte itemGroupSoundsId = Byte.Parse(fields[12]);
                    int contentTuningId = Int32.Parse(fields[13]);
                    uint maxDurability = UInt32.Parse(fields[14]);
                    byte ammunitionType = Byte.Parse(fields[15]);
                    byte damageType1 = Byte.Parse(fields[16]);
                    byte damageType2 = Byte.Parse(fields[17]);
                    byte damageType3 = Byte.Parse(fields[18]);
                    byte damageType4 = Byte.Parse(fields[19]);
                    byte damageType5 = Byte.Parse(fields[20]);
                    short resistances1 = Int16.Parse(fields[21]);
                    short resistances2 = Int16.Parse(fields[22]);
                    short resistances3 = Int16.Parse(fields[23]);
                    short resistances4 = Int16.Parse(fields[24]);
                    short resistances5 = Int16.Parse(fields[25]);
                    short resistances6 = Int16.Parse(fields[26]);
                    short resistances7 = Int16.Parse(fields[27]);
                    ushort minDamage1 = UInt16.Parse(fields[28]);
                    ushort minDamage2 = UInt16.Parse(fields[29]);
                    ushort minDamage3 = UInt16.Parse(fields[30]);
                    ushort minDamage4 = UInt16.Parse(fields[31]);
                    ushort minDamage5 = UInt16.Parse(fields[32]);
                    ushort maxDamage1 = UInt16.Parse(fields[33]);
                    ushort maxDamage2 = UInt16.Parse(fields[34]);
                    ushort maxDamage3 = UInt16.Parse(fields[35]);
                    ushort maxDamage4 = UInt16.Parse(fields[36]);
                    ushort maxDamage5 = UInt16.Parse(fields[37]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.Item;
                    record.HotfixId = HotfixItemBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteUInt8(classId);
                    record.HotfixContent.WriteUInt8(subclassId);
                    record.HotfixContent.WriteUInt8(material);
                    record.HotfixContent.WriteInt8(inventoryType);
                    record.HotfixContent.WriteInt32(requiredLevel);
                    record.HotfixContent.WriteUInt8(sheatheType);
                    record.HotfixContent.WriteUInt16(randomSelect);
                    record.HotfixContent.WriteUInt16(itemRandomSuffixGroupId);
                    record.HotfixContent.WriteInt8(soundOverrideSubclassId);
                    record.HotfixContent.WriteUInt16(scalingStatDistributionId);
                    record.HotfixContent.WriteInt32(iconFileDataId);
                    record.HotfixContent.WriteUInt8(itemGroupSoundsId);
                    record.HotfixContent.WriteInt32(contentTuningId);
                    record.HotfixContent.WriteUInt32(maxDurability);
                    record.HotfixContent.WriteUInt8(ammunitionType);
                    record.HotfixContent.WriteUInt8(damageType1);
                    record.HotfixContent.WriteUInt8(damageType2);
                    record.HotfixContent.WriteUInt8(damageType3);
                    record.HotfixContent.WriteUInt8(damageType4);
                    record.HotfixContent.WriteUInt8(damageType5);
                    record.HotfixContent.WriteInt16(resistances1);
                    record.HotfixContent.WriteInt16(resistances2);
                    record.HotfixContent.WriteInt16(resistances3);
                    record.HotfixContent.WriteInt16(resistances4);
                    record.HotfixContent.WriteInt16(resistances5);
                    record.HotfixContent.WriteInt16(resistances6);
                    record.HotfixContent.WriteInt16(resistances7);
                    record.HotfixContent.WriteUInt16(minDamage1);
                    record.HotfixContent.WriteUInt16(minDamage2);
                    record.HotfixContent.WriteUInt16(minDamage3);
                    record.HotfixContent.WriteUInt16(minDamage4);
                    record.HotfixContent.WriteUInt16(minDamage5);
                    record.HotfixContent.WriteUInt16(maxDamage1);
                    record.HotfixContent.WriteUInt16(maxDamage2);
                    record.HotfixContent.WriteUInt16(maxDamage3);
                    record.HotfixContent.WriteUInt16(maxDamage4);
                    record.HotfixContent.WriteUInt16(maxDamage5);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }

        public static void WriteItemHotfix(ItemTemplate item, Framework.IO.ByteBuffer buffer)
        {
            buffer.WriteUInt8((byte)item.Class);
            buffer.WriteUInt8((byte)item.SubClass);
            buffer.WriteUInt8((byte)item.Material);
            buffer.WriteInt8((sbyte)item.InventoryType);
            buffer.WriteInt32((int)item.RequiredLevel);
            buffer.WriteUInt8((byte)item.SheathType);
            buffer.WriteUInt16((ushort)item.RandomProperty);
            buffer.WriteUInt16((ushort)item.RandomSuffix);
            buffer.WriteInt8(-1);
            buffer.WriteUInt16(0);
            buffer.WriteInt32((int)GameData.GetFileDataIdForItemDisplayId(item.DisplayID));
            buffer.WriteUInt8(0);
            buffer.WriteInt32(0);
            buffer.WriteUInt32(item.MaxDurability);
            buffer.WriteUInt8((byte)item.AmmoType);
            buffer.WriteUInt8((byte)item.DamageTypes[0]);
            buffer.WriteUInt8((byte)item.DamageTypes[1]);
            buffer.WriteUInt8((byte)item.DamageTypes[2]);
            buffer.WriteUInt8((byte)item.DamageTypes[3]);
            buffer.WriteUInt8((byte)item.DamageTypes[4]);
            buffer.WriteInt16((short)item.Armor);
            buffer.WriteInt16((short)item.HolyResistance);
            buffer.WriteInt16((short)item.FireResistance);
            buffer.WriteInt16((short)item.NatureResistance);
            buffer.WriteInt16((short)item.FrostResistance);
            buffer.WriteInt16((short)item.ShadowResistance);
            buffer.WriteInt16((short)item.ArcaneResistance);
            buffer.WriteUInt16((ushort)item.DamageMins[0]);
            buffer.WriteUInt16((ushort)item.DamageMins[1]);
            buffer.WriteUInt16((ushort)item.DamageMins[2]);
            buffer.WriteUInt16((ushort)item.DamageMins[3]);
            buffer.WriteUInt16((ushort)item.DamageMins[4]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[0]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[1]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[2]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[3]);
            buffer.WriteUInt16((ushort)item.DamageMaxs[4]);
        }

        public static void LoadCreatureDisplayInfoHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfo{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    ushort modelId = UInt16.Parse(fields[1]);
                    ushort soundId = UInt16.Parse(fields[2]);
                    sbyte sizeClass = SByte.Parse(fields[3]);
                    float creatureModelScale = Single.Parse(fields[4]);
                    byte creatureModelAlpha = Byte.Parse(fields[5]);
                    byte bloodId = Byte.Parse(fields[6]);
                    int extendedDisplayInfoId = Int32.Parse(fields[7]);
                    ushort nPCSoundId = UInt16.Parse(fields[8]);
                    ushort particleColorId = UInt16.Parse(fields[9]);
                    int portraitCreatureDisplayInfoId = Int32.Parse(fields[10]);
                    int portraitTextureFileDataId = Int32.Parse(fields[11]);
                    ushort objectEffectPackageId = UInt16.Parse(fields[12]);
                    ushort animReplacementSetId = UInt16.Parse(fields[13]);
                    byte flags = Byte.Parse(fields[14]);
                    int stateSpellVisualKitId = Int32.Parse(fields[15]);
                    float playerOverrideScale = Single.Parse(fields[16]);
                    float petInstanceScale = Single.Parse(fields[17]);
                    sbyte unarmedWeaponType = SByte.Parse(fields[18]);
                    int mountPoofSpellVisualKitId = Int32.Parse(fields[19]);
                    int dissolveEffectId = Int32.Parse(fields[20]);
                    sbyte gender = SByte.Parse(fields[21]);
                    int dissolveOutEffectId = Int32.Parse(fields[22]);
                    sbyte creatureModelMinLod = SByte.Parse(fields[23]);
                    int textureVariationFileDataId1 = Int32.Parse(fields[24]);
                    int textureVariationFileDataId2 = Int32.Parse(fields[25]);
                    int textureVariationFileDataId3 = Int32.Parse(fields[26]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.CreatureDisplayInfo;
                    record.HotfixId = HotfixCreatureDisplayInfoBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteUInt16(modelId);
                    record.HotfixContent.WriteUInt16(soundId);
                    record.HotfixContent.WriteInt8(sizeClass);
                    record.HotfixContent.WriteFloat(creatureModelScale);
                    record.HotfixContent.WriteUInt8(creatureModelAlpha);
                    record.HotfixContent.WriteUInt8(bloodId);
                    record.HotfixContent.WriteInt32(extendedDisplayInfoId);
                    record.HotfixContent.WriteUInt16(nPCSoundId);
                    record.HotfixContent.WriteUInt16(particleColorId);
                    record.HotfixContent.WriteInt32(portraitCreatureDisplayInfoId);
                    record.HotfixContent.WriteInt32(portraitTextureFileDataId);
                    record.HotfixContent.WriteUInt16(objectEffectPackageId);
                    record.HotfixContent.WriteUInt16(animReplacementSetId);
                    record.HotfixContent.WriteUInt8(flags);
                    record.HotfixContent.WriteInt32(stateSpellVisualKitId);
                    record.HotfixContent.WriteFloat(playerOverrideScale);
                    record.HotfixContent.WriteFloat(petInstanceScale);
                    record.HotfixContent.WriteInt8(unarmedWeaponType);
                    record.HotfixContent.WriteInt32(mountPoofSpellVisualKitId);
                    record.HotfixContent.WriteInt32(dissolveEffectId);
                    record.HotfixContent.WriteInt8(gender);
                    record.HotfixContent.WriteInt32(dissolveOutEffectId);
                    record.HotfixContent.WriteInt8(creatureModelMinLod);
                    record.HotfixContent.WriteInt32(textureVariationFileDataId1);
                    record.HotfixContent.WriteInt32(textureVariationFileDataId2);
                    record.HotfixContent.WriteInt32(textureVariationFileDataId3);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadCreatureDisplayInfoExtraHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoExtra{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    sbyte displayRaceId = SByte.Parse(fields[1]);
                    sbyte displaySexId = SByte.Parse(fields[2]);
                    sbyte displayClassId = SByte.Parse(fields[3]);
                    sbyte skinId = SByte.Parse(fields[4]);
                    sbyte faceId = SByte.Parse(fields[5]);
                    sbyte hairStyleId = SByte.Parse(fields[6]);
                    sbyte hairColorId = SByte.Parse(fields[7]);
                    sbyte facialHairId = SByte.Parse(fields[8]);
                    sbyte flags = SByte.Parse(fields[9]);
                    int bakeMaterialResourcesId = Int32.Parse(fields[10]);
                    int hDBakeMaterialResourcesId = Int32.Parse(fields[11]);
                    byte customDisplayOption1 = Byte.Parse(fields[12]);
                    byte customDisplayOption2 = Byte.Parse(fields[13]);
                    byte customDisplayOption3 = Byte.Parse(fields[14]);

                    HotfixRecord record = new HotfixRecord();
                    record.TableHash = DB2Hash.CreatureDisplayInfoExtra;
                    record.HotfixId = HotfixCreatureDisplayInfoExtraBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.Status = HotfixStatus.Valid;
                    record.HotfixContent.WriteUInt32(id);
                    record.HotfixContent.WriteInt8(displayRaceId);
                    record.HotfixContent.WriteInt8(displaySexId);
                    record.HotfixContent.WriteInt8(displayClassId);
                    record.HotfixContent.WriteInt8(skinId);
                    record.HotfixContent.WriteInt8(faceId);
                    record.HotfixContent.WriteInt8(hairStyleId);
                    record.HotfixContent.WriteInt8(hairColorId);
                    record.HotfixContent.WriteInt8(facialHairId);
                    record.HotfixContent.WriteInt8(flags);
                    record.HotfixContent.WriteInt32(bakeMaterialResourcesId);
                    record.HotfixContent.WriteInt32(hDBakeMaterialResourcesId);
                    record.HotfixContent.WriteUInt8(customDisplayOption1);
                    record.HotfixContent.WriteUInt8(customDisplayOption2);
                    record.HotfixContent.WriteUInt8(customDisplayOption3);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        public static void LoadCreatureDisplayInfoOptionHotfixes()
        {
            var path = Path.Combine("CSV", "Hotfix", $"CreatureDisplayInfoOption{ModernVersion.ExpansionVersion}.csv");
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = false;

                // Skip the row with the column names
                csvParser.ReadLine();

                uint counter = 0;
                while (!csvParser.EndOfData)
                {
                    counter++;

                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    uint id = UInt32.Parse(fields[0]);
                    int chrCustomizationOptionId = Int32.Parse(fields[1]);
                    int chrCustomizationChoiceId = Int32.Parse(fields[2]);
                    int creatureDisplayInfoExtraId = Int32.Parse(fields[3]);

                    HotfixRecord record = new HotfixRecord();
                    record.Status = HotfixStatus.Valid;
                    record.TableHash = DB2Hash.CreatureDisplayInfoOption;
                    record.HotfixId = HotfixCreatureDisplayInfoOptionBegin + counter;
                    record.UniqueId = record.HotfixId;
                    record.RecordId = id;
                    record.HotfixContent.WriteInt32(chrCustomizationOptionId);
                    record.HotfixContent.WriteInt32(chrCustomizationChoiceId);
                    record.HotfixContent.WriteInt32(creatureDisplayInfoExtraId);
                    Hotfixes.Add(record.HotfixId, record);
                }
            }
        }
        #endregion
    }

    // Data structures
    public class BroadcastText
    {
        public uint Entry;
        public string MaleText;
        public string FemaleText;
        public uint Language;
        public ushort[] Emotes = new ushort[3];
        public ushort[] EmoteDelays = new ushort[3];
    }
    public class ItemDisplayData
    {
        public uint Entry;
        public uint DisplayId;
        public byte InventoryType;
    }
    public class Battleground
    {
        public bool IsArena;
        public List<uint> MapIds = new List<uint>();
    }
    public class TaxiPath
    {
        public uint Id;
        public uint From;
        public uint To;
        public int Cost;
    }
    public class TaxiNode
    {
        public uint Id;
        public uint mapId;
        public float x, y, z;
    }
    public class TaxiPathNode
    {
        public uint Id;
        public uint pathId;
        public uint nodeIndex;
        public uint mapId;
        public float x, y, z;
        public uint flags;
        public uint delay;
    }
    public class ChatChannel
    {
        public uint Id;
        public ChannelFlags Flags;
        public string Name;
    }

    // Hotfix structures
    public class AreaTrigger
    {
        public string Message;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public uint Id;
        public ushort MapId;
        public byte PhaseUseFlags;
        public ushort PhaseId;
        public ushort PhaseGroupId;
        public float Radius;
        public float BoxLength;
        public float BoxWidth;
        public float BoxHeight;
        public float BoxYaw;
        public byte ShapeType;
        public ushort ShapeId;
        public ushort ActionSetId;
        public byte Flags;
    }
}
