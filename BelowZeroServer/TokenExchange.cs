using System.Collections.Concurrent;
using BelowZeroMultiplayerCommon;

namespace BelowZeroServer
{
    public class TokenExchange
    {
        private static ConcurrentDictionary<string, TokenData> m_tokens = new ConcurrentDictionary<string, TokenData>();

        public static void CreateToken(TokenData tokenData)
        {
            bool added = m_tokens.TryAdd(tokenData.tokenGuid, tokenData);

            if (!added)
            {
                Logger.Log($"Could not add token with GUID: {tokenData.tokenGuid} as it already exists!");
                return;
            }

            Logger.Log($"Client: {Server.ResolvePlayerName(tokenData.clientWithToken)} added token with GUID: {tokenData.tokenGuid}");
        }

        public static void UpdateToken(string _token, int _client, Vector3 _pos, Quaternion _rot, Vector3 _scale)
        {
            if (DoesClientHaveToken(_token, _client))
            {
                if (m_tokens.ContainsKey(_token))
                {
                    m_tokens[_token].position = _pos;
                    m_tokens[_token].rotation = _rot;
                    m_tokens[_token].scale = _scale;

                    Logger.Log($"Client: {Server.ResolvePlayerName(_client)} updated token: {_token} to pos: {_pos}");

                    NetSend.PlayerUpdatedToken(m_tokens[_token]);
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

        public static int FindOwnerOfToken(string _token)
        {
            if (m_tokens.ContainsKey(_token))
            {
                return m_tokens[_token].clientWithToken;
            }
            else
            {
                Logger.Log($"Tried to resolve owner of token: {_token} that no longer exists!");
                return 0;
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

                    NetSend.PlayerDestroyedToken(tokenData);

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

        public static bool DoesClientHaveToken(string _token, int _client)
        {
            if (m_tokens.ContainsKey(_token))
            {
                if (m_tokens[_token].clientWithToken == _client)
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
