using BelowZeroMultiplayerCommon;
using System;

namespace BelowZeroServer
{
    public class ServerNode
    {
        public string m_guid;
        public UnityTransform m_transform;
        public TokenExchangePolicy m_tokenExchangePolicy;
        public NetworkedEntityType m_networkedEntityType;
        public ServerToken m_token;
        public float m_tickRate;

        private DateTime lastUpdateTime;

        public ServerNode(NodeDescriptor _desc)
        {
            m_guid = _desc.guid;
            m_transform = _desc.initialTransform;
            m_tokenExchangePolicy = _desc.tokenExchangePolicy;
            m_networkedEntityType = _desc.networkedEntity;
            m_tickRate = _desc.tickRate;
            lastUpdateTime = DateTime.Now;

            // Create the token
            m_token = new ServerToken(_desc.client, m_tokenExchangePolicy, m_guid);
        }

        public void OnClientUpdatedNodeMetaData(TokenExchangePolicy _tokenExchangePolicy, float _tickRate)
        {
            // Invalidate this call if the token policy is none or if the tick rate would result
            // in more than 5 seconds between updates
            if (_tokenExchangePolicy == TokenExchangePolicy.None)
            {
                throw new Exception($"Invalid token exchange policy for node with guid: {m_guid}");
            }
            else if (_tickRate <= 0.2f)
            {
                Logger.Log($"Call to update tick rate for node: {m_guid} is slower than 1 state update every 5 seconds at: {_tickRate} discarding node meta-data update");
                return;
            }

            m_tokenExchangePolicy = _tokenExchangePolicy;
            m_tickRate = _tickRate;

            ReplicateNodeMetaDataChange();
        }

        public void OnClientUpdatedNode(UnityTransform newTransform)
        {
            m_transform = newTransform;

            // Record the update time, we can use this to tell if the client is struggling to keep up
            // TODO: Need to check the maths on this
            DateTime currTime = DateTime.Now;
            if (currTime.Ticks > lastUpdateTime.AddSeconds((1.0f / m_tickRate) * 2.0f).Ticks)
            {
                long ticksLate = (currTime.Ticks - lastUpdateTime.Ticks - ((long)((1.0f / m_tickRate) * 1000000000.0f)));
                float calculatedTimeLateSeconds = (float)(ticksLate / 1000000000.0);
                string clientName = Server.ResolvePlayerName(m_token.clientWithToken);
                Logger.Log($"Client: {clientName} is falling behind on network replication. Was {calculatedTimeLateSeconds * 1000.0f}ms late on update for node: {m_guid}");
            }
            
            lastUpdateTime = DateTime.Now;

            ReplicateNodeUpdate();
        }

        public void ReplicateNodeUpdate()
        {
            
        }

        public void ReplicateNodeMetaDataChange()
        {

        }
    }

    public class NodeDescriptor
    {
        public string guid = string.Empty;
        public int client = 0; // Zero == Server
        public TokenExchangePolicy tokenExchangePolicy = TokenExchangePolicy.None;
        public NetworkedEntityType networkedEntity = NetworkedEntityType.None;
        public int associatedTechType = 0;
        public float tickRate = 0.0f;
        public UnityTransform initialTransform = new UnityTransform();
    }
}
