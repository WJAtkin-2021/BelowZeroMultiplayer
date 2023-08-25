using System;
using System.Threading;

namespace BelowZeroServer
{
    public class NetReceive
    {
        public static void ConnectedReceived(int _fromClient, Packet _packet)
        {
            // Perform a credential check
            int clientId = _packet.ReadInt();
            string playerName = _packet.ReadString();
            string playerToken = _packet.ReadString();

            if (DataStore.IsUserNameInUse(playerName))
            {
                if (DataStore.CheckCredentials(playerName, playerToken))
                {
                    // Start the map download to this client
                    NetSend.UploadMapToClient(_fromClient);
                }
                else
                {
                    // User name is in use
                    NetSend.SendUserNameTakenMessage(_fromClient);
                    // Force a disconnect, NOTE: we allow time for the user name
                    // taken packet to be recived. TODO: add another packet that can
                    // be sent by the client to acknowledge this then we close
                    Thread.Sleep(500);
                    Server.m_instance.m_clients[_fromClient].DisconnectSilent();
                }
            }
            else
            {
                // Send a new machine token to the client if they did not
                // provide one
                if (playerToken == "")
                {
                    playerToken = Guid.NewGuid().ToString();
                    DataStore.SaveCredentails(playerName, playerToken);
                    NetSend.SendNewMachineToken(clientId, playerToken);
                }

                // Start the map download to this client
                NetSend.UploadMapToClient(_fromClient);
            }
        }

        public static void HandleClientSpawnMe(int _fromClient, Packet _packet)
        {
            // Grab the client data
            int clientId = _packet.ReadInt();
            string clientName = _packet.ReadString();

            // This is TCP so we can skip the check
            //Logger.Log($"Client: {clientId} wishes to spawn in!");

            Server.m_instance.m_clients[_fromClient].m_clientName = clientName;
            PlayerSaveData data = DataStore.GetPlayerData(clientName);

            NetSend.PlayerSpawned(clientId, clientName, data.Pos, data.Rot, data.IsInside);
            NetSend.SycPlayerList(clientId);
            NetSend.SyncUnlocks(clientId);
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
            bool isInside = _packet.ReadBool();

            //Logger.Log($"Client: {clientId}: Transform: {position} : {rotation}");

            Server.m_instance.m_clients[_fromClient].SetLastPos(position, rotation, isInside);

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

            Logger.Log($"{Server.ResolvePlayerName(_fromClient)} Dropped: {techName} With Token: {token} At: {position}");

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

            Logger.Log($"{Server.ResolvePlayerName(_fromClient)} Unlocked tech: {techType}");

            // Store for players joining later
            UnlockManager.AddTechUnlock(techType);

            // Replicate to all other clients
            NetSend.PlayerUnlockedTechKnowledge(_fromClient, techType, unlockEncyclopedia, verbose);
        }

        public static void HandleUnlockedPDAEncyclopedia(int _fromClient, Packet _packet)
        {
            // Read the packet
            string key = _packet.ReadString();

            if (string.IsNullOrEmpty(key))
                return;

            Logger.Log($"{Server.ResolvePlayerName(_fromClient)} Unlocked PDA Entry: {key}");

            // Store for players joining later
            UnlockManager.AddPdaEntry(key);

            // Replicate to all the other clients
            NetSend.PlayerUnlockedPDAEncyclopedia(_fromClient, key);
        }

        public static void HandleFragmentProgressUpdated(int _fromClient, Packet _packet)
        {
            // Read
            int techType = _packet.ReadInt();
            int currentFragments = _packet.ReadInt();
            int totalFragments = _packet.ReadInt();

            Logger.Log($"{Server.ResolvePlayerName(_fromClient)} Updated fragment progress: {techType} ({currentFragments}/{totalFragments})");

            // Store for players joining later
            UnlockManager.UpdateFragment(techType, currentFragments, totalFragments);

            // Replicate
            NetSend.PlayerUpdatedFragmentProgress(_fromClient, techType, currentFragments);

        }
    }
}
