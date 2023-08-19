using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class NetReceive
    {
        public static void ConnectedReceived(int _fromClient, Packet _packet)
        {
            // Start the map download to this client
            NetSend.UploadMapToClient(_fromClient);
        }

        public static void HandleClientSpawnMe(int _fromClient, Packet _packet)
        {
            // Grab the client data
            int clientId = _packet.ReadInt();
            string clientName = _packet.ReadString();

            // This is TCP so we can skip the check
            //Logger.Log($"Client: {clientId} wishes to spawn in!");

            NetSend.PlayerSpawned(clientId, clientName);
            NetSend.SycPlayerList(clientId);
        }

        public static void HandleTranformUpdate(int _fromClient, Packet _packet)
        {
            // Grab the client ID
            int clientId = _packet.ReadInt();

            // Ensure no spoofing is happening
            if (_fromClient != clientId)
            {
                // TODO: Interface with some kind of anticheat
                return;
            }

            Vector3 position = _packet.ReadVector3();
            Quaternion rotation = _packet.ReadQuaternoin();

            //Logger.Log($"Client: {clientId}: Transform: {position} : {rotation}");

            NetSend.PlayerTransformUpdate(clientId, position, rotation);
        }

        public static void HandleDroppedItem(int _fromClient, Packet _packet)
        {
            // Get the variables of the dropped item
            string techName = _packet.ReadString();
            Vector3 position = _packet.ReadVector3();
            string token = _packet.ReadString();

            // Replicate this
            NetSend.PlayerDroppedItem(_fromClient, techName, position, token);

            Logger.Log($"Client: {_fromClient} Dropped: {techName} With Token: {token}");

            // TODO: Store this data for late joiners

        }

        public static void HandlePickupItem(int _fromClient, Packet _packet)
        {
            string itemToken = _packet.ReadString();

            // Replicate this
            NetSend.PlayerPickedUpItem(_fromClient, itemToken);

            // TODO: Un-Store this data for late joiners
        }

        public static void HandleTechKnowledgeAdded(int _fromClient, Packet _packet)
        {
            int techType = _packet.ReadInt();
            bool unlockEncyclopedia = _packet.ReadBool();
            bool verbose = _packet.ReadBool();

            Logger.Log($"Client: {_fromClient} Unlocked tech: {techType}");

            // Replicate to all other clients
            NetSend.PlayerUnlockedTechKnowledge(_fromClient, techType, unlockEncyclopedia, verbose);

            // TODO: Store this in a data base

        }

        public static void HandleUnlockedPDAEncyclopedia(int _fromClient, Packet _packet)
        {
            // Read the packet
            string key = _packet.ReadString();

            if (string.IsNullOrEmpty(key))
                return;

            Logger.Log($"Client: {_fromClient} Unlocked PDA Entry: {key}");

            // Replicate to all the other clients
            NetSend.PlayerUnlockedPDAEncyclopedia(_fromClient, key);
        }

        public static void HandleFramentProgressUpdated(int _fromClient, Packet _packet)
        {
            // Read
            int techType = _packet.ReadInt();

            Logger.Log($"Client: {_fromClient} Updated fragment progress: {techType}");

            // Replicate
            NetSend.PlayerUpdatedFragmentProgress(_fromClient, techType);

            // TODO: Store in database
        }
    }
}
