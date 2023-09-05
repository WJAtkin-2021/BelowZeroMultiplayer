using BelowZeroMultiplayerCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class TokenExchange
    {
        private static ConcurrentDictionary<string, TokenData> m_tokens = new ConcurrentDictionary<string, TokenData>();

        public static void CreateToken(string _guid, int _client, Vector3 _initalPos)
        {
            TokenData data = new TokenData();
            data.tokenGuid = _guid;
            data.clientWithToken = _client;
            data.position = _initalPos;
            bool added = m_tokens.TryAdd(_guid, data);

            if (!added)
            {
                Logger.Log($"Could not add token with GUID: {_guid} as it already exists!");
                return;
            }

            Logger.Log($"Client: {Server.ResolvePlayerName(_client)} added token with GUID: {_guid}");
        }

        public static void UpdateToken(string _token, int _client, Vector3 _pos)
        {
            if (DoesClientHaveToken(_token, _client))
            {
                if (m_tokens.ContainsKey(_token))
                {
                    m_tokens[_token].position = _pos;

                    Logger.Log($"Client: {Server.ResolvePlayerName(_client)} updated token: {_token} to pos: {_pos}");

                    // TODO: Propagate this to other clients
                }
                else
                {
                    Logger.Log($"Client: {Server.ResolvePlayerName(_client)} tried to update token: {_token} that no longer exists!");
                }
            }
            else
            {
                Logger.Log($"Client: {Server.ResolvePlayerName(_client)} tried to update token that they do not own!");
            }
        }

        public static void DestroyToken(string _token, int _client)
        {
            if (DoesClientHaveToken(_token, _client))
            {
                if (m_tokens.ContainsKey(_token))
                {
                    TokenData tokenData;
                    bool removed = m_tokens.TryRemove(_token, out tokenData);
                    if (!removed)
                    {
                        Logger.Log($"Failed to remove token with GUID: {_token}");
                        return;
                    }

                    Logger.Log($"Client: {Server.ResolvePlayerName(_client)} removed token with GUID: {_token}");

                    // TODO: Propagate this to other clients

                }
                else
                {
                    Logger.Log($"Client: {Server.ResolvePlayerName(_client)} tried to remove token: {_token} that no longer exists!");
                }
            }
            else
            {
                Logger.Log($"Client: {Server.ResolvePlayerName(_client)} tried to remove token: {_token} that they do not own!");
            }
        }

        public static bool DoesClientHaveToken(string _guid, int _client)
        {
            if (m_tokens.ContainsKey(_guid))
            {
                if (m_tokens[_guid].clientWithToken == _client)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class TokenData
    {
        public string tokenGuid = string.Empty;
        public int clientWithToken = 0; // Zero == Server
        public TokenExchangePolicy tokenExchangePolicy = TokenExchangePolicy.None;
        public int associatedTechType = 0;
        public NetworkedEntityType networkedEntity = NetworkedEntityType.None;
        public float tickRate = 0.0f;
        public Vector3 position = new Vector3();
        public Quaternion rotation = new Quaternion();
        public Vector3 scale = new Vector3();
    }
}
