﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using BelowZeroMultiplayerCommon;

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
            InventoryData invData = DataStore.LoadInventoryData(clientName);

            NetSend.PlayerSpawned(clientId, clientName, data.Pos, data.Rot, data.IsInside);
            NetSend.SycPlayerList(clientId);
            NetSend.SyncUnlocks(clientId);
            NetSend.SyncPlayerInventory(clientId, invData);
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

            // Store for players joining later
            if (UnlockManager.AddTechUnlock(techType))
            {
                Logger.Log($"{Server.ResolvePlayerName(_fromClient)} Unlocked tech: {techType}");

                // Replicate to all other clients
                NetSend.PlayerUnlockedTechKnowledge(_fromClient, techType, unlockEncyclopedia, verbose);
            }
        }

        public static void HandleUnlockedPDAEncyclopedia(int _fromClient, Packet _packet)
        {
            // Read the packet
            string encyclopediaKey = _packet.ReadString();
            int techType = _packet.ReadInt();

            if (string.IsNullOrEmpty(encyclopediaKey))
                return;

            // Store for players joining later
            if (UnlockManager.AddPdaEntry(encyclopediaKey, techType))
            {
                Logger.Log($"{Server.ResolvePlayerName(_fromClient)} Unlocked PDA Entry: {encyclopediaKey}");

                // Replicate to all the other clients
                NetSend.PlayerUnlockedPDAEncyclopedia(_fromClient, encyclopediaKey, techType);
            }
        }

        public static void HandleFragmentProgressUpdated(int _fromClient, Packet _packet)
        {
            // Read
            int techType = _packet.ReadInt();
            int currentFragments = _packet.ReadInt();
            int totalFragments = _packet.ReadInt();

            Logger.Log($"{Server.ResolvePlayerName(_fromClient)} Updated fragment progress: {techType} ({currentFragments}/{totalFragments})");

            // Store for players joining later
            UnlockManager.UpdateFragment(techType, currentFragments);

            // Replicate
            NetSend.PlayerUpdatedFragmentProgress(_fromClient, techType, currentFragments);
        }

        public static void HandlePlayerInventoryUpdated(int _fromClient, Packet _packet)
        {
            Logger.Log($"Handing inventory data from: {Server.ResolvePlayerName(_fromClient)}");

            // Read the inventory data
            InventoryData data = new InventoryData();
            int storageLen = _packet.ReadInt();
            data.serializedStorage = _packet.ReadBytes(storageLen);
            int numOfQuickSlots = _packet.ReadInt();
            string[] quickSlots = new string[numOfQuickSlots];
            for (int i = 0; i < numOfQuickSlots; i++)
            {
                quickSlots[i] = _packet.ReadString();
            }
            data.serializedQuickSlots = quickSlots;
            int equipmentLen = _packet.ReadInt();
            data.serializedEquipment = _packet.ReadBytes(equipmentLen);
            int numEquipmentSlots = _packet.ReadInt();
            data.serializedEquipmentSlots = new Dictionary<string, string>();
            for (int i = 0; i < numEquipmentSlots; i++)
            {
                string key = _packet.ReadString();
                string value = _packet.ReadString();
                data.serializedEquipmentSlots[key] = value;
            }
            int pendingItemsLen = _packet.ReadInt();
            data.serializedPendingItems = _packet.ReadBytes(pendingItemsLen);

            DataStore.SaveInventoryData(data, Server.ResolvePlayerName(_fromClient));
        }
    }
}
