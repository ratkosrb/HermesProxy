using System.Collections.Generic;

using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using Google.Protobuf;
using HermesProxy.Framework.Constants;
using HermesProxy.Framework.Logging;
using HermesProxy.Framework.Util;
using HermesProxy.Network.BattleNet.REST;
using HermesProxy.Network.BattleNet.Services;
using HermesProxy.Network.Realm;

namespace HermesProxy.Network.BattleNet.Session
{
    public partial class BattlenetSession
    {
        [BattlenetService(ServiceHash.GameUtilitiesService, 1)]
        public BattlenetRpcErrorCode HandleClientRequest(ClientRequest request, ClientResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            Bgs.Protocol.Attribute command = null;
            Dictionary<string, Variant> Params = new();

            for (int i = 0; i < request.Attribute.Count; ++i)
            {
                Bgs.Protocol.Attribute attr = request.Attribute[i];
                Params[attr.Name] = attr.Value;
                if (attr.Name.Contains("Command_"))
                    command = attr;
            }

            if (command == null)
            {
                Log.Print(LogType.Error, $"{GetRemoteEndpoint()} sent ClientRequest with no command.");
                return BattlenetRpcErrorCode.RpcMalformedRequest;
            }

            return command.Name switch
            {
                "Command_RealmListTicketRequest_v1_b9" => GetRealmListTicket(Params, response),
                "Command_LastCharPlayedRequest_v1_b9" => GetLastCharPlayed(Params, response),
                "Command_RealmListRequest_v1_b9" => GetRealmList(Params, response),
                "Command_RealmJoinRequest_v1_b9" => JoinRealm(Params, response),
                _ => BattlenetRpcErrorCode.RpcNotImplemented
            };
        }

        [BattlenetService(ServiceHash.GameUtilitiesService, 10)]
        public BattlenetRpcErrorCode HandleGetAllValuesForAttributes(GetAllValuesForAttributeRequest request, GetAllValuesForAttributeResponse response)
        {
            if (!_authed)
                return BattlenetRpcErrorCode.Denied;

            if (request.AttributeKey == "Command_RealmListRequest_v1_b9")
            {
                RealmManager.WriteSubRegions(response);
                return BattlenetRpcErrorCode.Ok;
            }

            return BattlenetRpcErrorCode.RpcNotImplemented;
        }

        BattlenetRpcErrorCode GetRealmListTicket(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant identity = Params.LookupByKey("Param_Identity");
            if (identity != null)
            {
                var realmListTicketIdentity = JSON.CreateObject<RealmListTicketIdentity>(identity.BlobValue.ToStringUtf8(), true);
                /*
                var gameAccount = accountInfo.GameAccounts.LookupByKey(realmListTicketIdentity.GameAccountId);
                if (gameAccount != null)
                    gameAccountInfo = gameAccount;
                */
            }

            bool clientInfoOk = false;
            Variant clientInfo = Params.LookupByKey("Param_ClientInfo");
            if (clientInfo != null)
            {
                var realmListTicketClientInformation = JSON.CreateObject<RealmListTicketClientInformation>(clientInfo.BlobValue.ToStringUtf8(), true);
                clientInfoOk = true;
                int i = 0;
                foreach (byte b in realmListTicketClientInformation.Info.Secret)
                    _clientSecret[i++] = b;
            }

            if (!clientInfoOk)
                return BattlenetRpcErrorCode.WowServicesDeniedRealmListTicket;

            /*
            PreparedStatement stmt = DB.Login.GetPreparedStatement(LoginStatements.UpdBnetLastLoginInfo);
            stmt.AddValue(0, GetRemoteIpEndPoint().ToString());
            stmt.AddValue(1, (byte)locale.ToEnum<Locale>());
            stmt.AddValue(2, os);
            stmt.AddValue(3, accountInfo.Id);

            DB.Login.Execute(stmt);
            */

            var attribute = new Bgs.Protocol.Attribute();
            attribute.Name = "Param_RealmListTicket";
            attribute.Value = new Variant();
            attribute.Value.BlobValue = ByteString.CopyFrom("AuthRealmListTicket", System.Text.Encoding.UTF8);
            response.Attribute.Add(attribute);

            return BattlenetRpcErrorCode.Ok;
        }

        BattlenetRpcErrorCode GetLastCharPlayed(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant subRegion = Params.LookupByKey("Command_LastCharPlayedRequest_v1_b9");
            if (subRegion != null)
            {
                /*
                var lastPlayerChar = gameAccountInfo.LastPlayedCharacters.LookupByKey(subRegion.StringValue);
                if (lastPlayerChar != null)
                {
                    var compressed = RealmManager.GetRealmEntryJSON(lastPlayerChar.RealmId, build);
                    if (compressed.Length == 0)
                        return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

                    var attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_RealmEntry";
                    attribute.Value = new Variant();
                    attribute.Value.BlobValue = ByteString.CopyFrom(compressed);
                    response.Attribute.Add(attribute);

                    attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_CharacterName";
                    attribute.Value = new Variant();
                    attribute.Value.StringValue = lastPlayerChar.CharacterName;
                    response.Attribute.Add(attribute);

                    attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_CharacterGUID";
                    attribute.Value = new Variant();
                    attribute.Value.BlobValue = ByteString.CopyFrom(BitConverter.GetBytes(lastPlayerChar.CharacterGUID));
                    response.Attribute.Add(attribute);

                    attribute = new Bgs.Protocol.Attribute();
                    attribute.Name = "Param_LastPlayedTime";
                    attribute.Value = new Variant();
                    attribute.Value.IntValue = (int)lastPlayerChar.LastPlayedTime;
                    response.Attribute.Add(attribute);
                }
                */
                return BattlenetRpcErrorCode.Ok;
            }

            return BattlenetRpcErrorCode.UtilServerUnknownRealm;
        }

        BattlenetRpcErrorCode GetRealmList(Dictionary<string, Variant> Params, ClientResponse response)
        {
            //if (gameAccountInfo == null)
            //    return BattlenetRpcErrorCode.UserServerBadWowAccount;

            string subRegionId = "";
            Variant subRegion = Params.LookupByKey("Command_RealmListRequest_v1_b9");
            if (subRegion != null)
                subRegionId = subRegion.StringValue;

            var compressed = RealmManager.GetRealmList(_build, subRegionId);
            if (compressed.Length == 0)
                return BattlenetRpcErrorCode.UtilServerFailedToSerializeResponse;

            var attribute = new Bgs.Protocol.Attribute();
            attribute.Name = "Param_RealmList";
            attribute.Value = new Variant();
            attribute.Value.BlobValue = ByteString.CopyFrom(compressed);
            response.Attribute.Add(attribute);

            var realmCharacterCounts = new RealmCharacterCountList();
            /*
            foreach (var characterCount in gameAccountInfo.CharacterCounts)
            {
                var countEntry = new RealmCharacterCountEntry();
                countEntry.WowRealmAddress = (int)characterCount.Key;
                countEntry.Count = characterCount.Value;
                realmCharacterCounts.Counts.Add(countEntry);
            }
            */

            compressed = JSON.Deflate("JSONRealmCharacterCountList", realmCharacterCounts);

            attribute = new Bgs.Protocol.Attribute();
            attribute.Name = "Param_CharacterCountList";
            attribute.Value = new Variant();
            attribute.Value.BlobValue = ByteString.CopyFrom(compressed);
            response.Attribute.Add(attribute);
            return BattlenetRpcErrorCode.Ok;
        }

        BattlenetRpcErrorCode JoinRealm(Dictionary<string, Variant> Params, ClientResponse response)
        {
            Variant realmAddress = Params.LookupByKey("Param_RealmAddress");
            if (realmAddress != null)
                return RealmManager.JoinRealm((uint)realmAddress.UintValue, _build, GetRemoteIpEndPoint().Address, _clientSecret, (Locale)System.Enum.Parse(typeof(Locale), _locale), _os, "Wow1", response);

            return BattlenetRpcErrorCode.WowServicesInvalidJoinTicket;
        }
    }
}
