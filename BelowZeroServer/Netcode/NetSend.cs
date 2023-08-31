using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using BelowZeroMultiplayerCommon;

namespace BelowZeroServer
{
    public class NetSend
    {
        public static void Connected(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.Connected))
            {
                packet.Write(_client);
                packet.Write(DataStore.GetServerGuid());

                SendTCPData(_client, packet);
            }
        }

        public static void SendNewMachineToken(int _client, string _machineToken)
        {
            using (Packet packet = new Packet((int)ServerPackets.NewMachineToken))
            {
                packet.Write(DataStore.GetServerGuid());
                packet.Write(_machineToken);

                SendTCPData(_client, packet);
            }
        }

        public static void SendUserNameTakenMessage(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.UserNameInUse))
            {
                packet.Write(_client);

                SendTCPData(_client, packet);
            }
        }

        public static void PlayerDisconnected(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerDisconnected))
            {
                packet.Write(_client);

                SendTCPDataToAll(_client, packet);
            }
        }

        public static void UploadMapToClient(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.MapDownload))
            {
                byte[] mapData = Server.m_instance.GetMapData();

                // Length
                packet.Write(mapData.Length);

                // Data
                packet.Write(mapData);

                Stopwatch performanceTimer = Stopwatch.StartNew();
                SendTCPData(_client, packet);
                performanceTimer.Stop();

                Logger.Log($"Took: {performanceTimer.ElapsedMilliseconds}ms to upload map to client {_client}");
            }
        }

        public static void PlayerSpawned(int _client, string _playerName, Vector3 _spawnPos, Quaternion _spawnRot, bool _isInside)
        {
            using (Packet packet = new Packet((int)ServerPackets.SpawnPlayer))
            {
                // Write the client ID of the new client
                packet.Write(_client);
                packet.Write(_playerName);
                packet.Write(_spawnPos);
                packet.Write(_spawnRot);
                packet.Write(_isInside);

                SendTCPDataToAll(packet);
            }
        }

        public static void SycPlayerList(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.SycPlayerList))
            {
                // Generate a list of players currently in
                List<int> spawnedPlayers = new List<int>();
                for (int i = 0; i < Server.m_instance.m_clients.Count; i++)
                {
                    if (Server.m_instance.m_clients[i].m_tcp.m_tcpClient != null && i != _client)
                    {
                        spawnedPlayers.Add(i);
                    }
                }

                // Write the total number
                packet.Write(spawnedPlayers.Count);
                // Write each client ID and name
                for (int i = 0; i < spawnedPlayers.Count; i++)
                {
                    packet.Write(spawnedPlayers[i]);
                    packet.Write(Server.m_instance.m_clients[spawnedPlayers[i]].m_clientName);
                    packet.Write(Server.m_instance.m_clients[spawnedPlayers[i]].m_lastPos);
                    packet.Write(Server.m_instance.m_clients[spawnedPlayers[i]].m_lastRot);
                }

                // Send it
                SendTCPData(_client, packet);
            }
        }

        public static void PlayerTransformUpdate(int _client, Vector3 _pos, Quaternion _rot)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerTransformUpdate))
            {
                packet.Write(_client);
                packet.Write(_pos);
                packet.Write(_rot);

                SendUDPDataToAll(_client, packet);
            }
        }

        public static void PlayerDroppedItem(int _client, string _techName, Vector3 _pos, string token)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerDroppedItem))
            {
                packet.Write(_techName);
                packet.Write(_pos);
                packet.Write(token);

                SendTCPDataToAll(_client, packet);
            }
        }

        public static void PlayerPickedUpItem(int _client, string token)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerPickedUpItem))
            {
                packet.Write(token);

                SendTCPDataToAll(_client, packet);
            }
        }

        public static void PlayerUnlockedTechKnowledge(int _toClient, int techType, bool unlockEncyclopedia, bool verbose)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerUnlockedTechKnowledge))
            {
                packet.Write(techType);
                packet.Write(unlockEncyclopedia);
                packet.Write(verbose);

                SendTCPDataToAll(_toClient, packet);
            }
        }

        public static void PlayerUnlockedPDAEncyclopedia(int _toClient, string _key, int _techType)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerUnlockedPDAEncyclopedia))
            {
                packet.Write(_key);
                packet.Write(_techType);

                SendTCPDataToAll(_toClient, packet);
            }
        }

        public static void PlayerUpdatedFragmentProgress(int _client, int _techType, int _parts)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerUpdatedFragmentProgress))
            {
                packet.Write(_techType);
                packet.Write(_parts);

                SendTCPDataToAll(_client, packet);
            }
        }

        public static void MessageBroadcast(string _message)
        {
            using (Packet packet = new Packet((int)ServerPackets.MessageBroadcast))
            {
                packet.Write(_message);

                SendTCPDataToAll(packet);
            }
        }

        public static void SyncUnlocks(int _toClient)
        {
            using (Packet packet = new Packet((int)ServerPackets.SyncUnlocks))
            {
                // Serialize the data
                // Write the tech unlocks
                List<int> techs = UnlockManager.GetAllUnlockedTech();
                packet.Write(techs.Count);
                for (int i = 0; i < techs.Count; i++)
                    packet.Write(techs[i]);
                // Write the PDA Entries
                ConcurrentDictionary<string, int> pdaEntries = UnlockManager.GetAllPdaEncyclopedia();
                packet.Write(pdaEntries.Count);
                foreach (KeyValuePair<string, int> entry in pdaEntries)
                {
                    packet.Write(entry.Key);
                    packet.Write(entry.Value);
                }

                // Write the fragment counts
                List<FragmentKnowledge> fragments = UnlockManager.GetAllFragments();
                packet.Write(fragments.Count);
                for (int i = 0; i < fragments.Count; i++)
                {
                    packet.Write(fragments[i].techType);
                    packet.Write(fragments[i].parts);
                }
                
                SendTCPData(_toClient, packet);
            }
        }

        public static void SyncPlayerInventory(int _toClient, InventoryData _inventory)
        {
            if (_inventory == null)
                return;

            using (Packet packet = new Packet((int)ServerPackets.SyncPlayerInventory))
            {
                packet.Write(_inventory.serializedStorage.Length);
                packet.Write(_inventory.serializedStorage);
                packet.Write(_inventory.serializedQuickSlots.Length);
                for (int i = 0; i < _inventory.serializedQuickSlots.Length; i++)
                {
                    packet.Write(_inventory.serializedQuickSlots[i]);
                }
                packet.Write(_inventory.serializedEquipment.Length);
                packet.Write(_inventory.serializedEquipment);
                packet.Write(_inventory.serializedEquipmentSlots.Count);
                foreach (KeyValuePair<string, string> entry in _inventory.serializedEquipmentSlots)
                {
                    packet.Write(entry.Key);
                    packet.Write(entry.Value);
                }
                packet.Write(_inventory.serializedPendingItems.Length);
                packet.Write(_inventory.serializedPendingItems);

                SendTCPData(_toClient, packet);
            }
        }

        #region SendImplementations

        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.m_instance.m_clients[_toClient].m_tcp.SendPacket(_packet);
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.m_instance.m_clients[_toClient].m_udp.SendPacket(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                if (Server.m_instance.m_clients.ContainsKey(i))
                {
                    if (Server.m_instance.m_clients[i].m_tcp != null)
                    {
                        Server.m_instance.m_clients[i].m_tcp.SendPacket(_packet);
                    }
                }
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                if (i != _exceptClient)
                {
                    if (Server.m_instance.m_clients.ContainsKey(i))
                    {
                        if (Server.m_instance.m_clients[i].m_tcp != null)
                        {
                            Server.m_instance.m_clients[i].m_tcp.SendPacket(_packet);
                        }
                    }
                }
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                if (Server.m_instance.m_clients.ContainsKey(i))
                {
                    if (Server.m_instance.m_clients[i].m_udp != null)
                    {
                        Server.m_instance.m_clients[i].m_udp.SendPacket(_packet);
                    }
                }
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                if (i != _exceptClient)
                {
                    if (Server.m_instance.m_clients.ContainsKey(i))
                    {
                        if (Server.m_instance.m_clients[i].m_udp != null)
                        {
                            Server.m_instance.m_clients[i].m_udp.SendPacket(_packet);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
