using System;
using BelowZeroMultiplayerCommon;

namespace BelowZeroServer
{
    public class ServerToken
    {
        public string attachedNode;
        public int clientWithToken;
        public TokenExchangePolicy exchangePolicy;

        public ServerToken(int _initalClient, TokenExchangePolicy _exchangePolicy, string _attachedNode)
        {
            clientWithToken = _initalClient;
            exchangePolicy = _exchangePolicy;
            attachedNode = _attachedNode;
        }

        public bool TryAquireToken(int _newClient, bool _wasServerRequest)
        {
            switch (exchangePolicy)
            {
                case TokenExchangePolicy.DoNotYield: 
                    return false;
                case TokenExchangePolicy.OnClientRequestOrAuto:
                    {
                        clientWithToken = _newClient;
                        return true;
                    }
                case TokenExchangePolicy.OnClientRequest:
                    {
                        if (_wasServerRequest)
                        { 
                            return false;
                        }
                        else
                        {
                            clientWithToken = _newClient;
                            return true;
                        }
                    }
                case TokenExchangePolicy.AutomaticHandover:
                    {
                        if (_wasServerRequest)
                        {
                            clientWithToken = _newClient;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    throw new Exception($"No Token Exchange Policy On Node: {attachedNode}");
            }
        }

        public bool HasToken(int _clientId)
        {
            return clientWithToken == _clientId;
        }
    }
}
