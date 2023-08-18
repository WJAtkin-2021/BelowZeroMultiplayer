using BelowZeroMultiplayerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class NetSend
    {
        public static void Connected(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.Connected))
            {
                packet.Write(_client);

                SendTCPData(_client, packet);
            }
        }

        public static void PlayerDisconnected(int _client)
        {

        }

        public static void UploadMapToClient(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.MapDownload))
            {
                byte[] mapData = Server.instance.GetMapData();

                // Length
                packet.Write(mapData.Length);

                // Data
                packet.Write(mapData);

                SendTCPData(_client, packet);
            }
        }

        public static void PlayerSpawned(int _client, string _playerName)
        {
            using (Packet packet = new Packet((int)ServerPackets.SpawnPlayer))
            {
                // Write the client ID of the new client
                packet.Write(_client);
                packet.Write(_playerName);

                // TODO: Provide a spawn location for them.

                SendTCPDataToAll(_client, packet);
            }
        }

        public static void SycPlayerList(int _client)
        {
            using (Packet packet = new Packet((int)ServerPackets.SycPlayerList))
            {
                // Generate a list of players currently in
                List<int> spawnedPlayers = new List<int>();
                for (int i = 0; i < Server.instance.m_clients.Count; i++)
                {
                    if (Server.instance.m_clients[i].m_tcp.m_tcpClient != null && i != _client)
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
                    packet.Write(Server.instance.m_clients[i].m_clientName);
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

        public static void PlayerUnlockedPDAEncyclopedia(int _toClient, string _key, bool _verbose, bool _postNotification)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerUnlockedPDAEncyclopedia))
            {
                packet.Write(_key);
                packet.Write(_verbose);
                packet.Write(_postNotification);

                SendTCPDataToAll(_toClient, packet);
            }
        }

        public static void PlayerUpdatedFragmentProgress(int _client, int _techType)
        {
            using (Packet packet = new Packet((int)ServerPackets.PlayerUpdatedFragmentProgress))
            {
                packet.Write(_techType);

                SendTCPDataToAll(_client, packet);
            }
        }

        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.instance.m_clients[_toClient].m_tcp.SendPacket(_packet);
        }

        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.instance.m_clients[_toClient].m_udp.SendPacket(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                if (Server.instance.m_clients.ContainsKey(i))
                {
                    if (Server.instance.m_clients[i].m_tcp != null)
                    {
                        Server.instance.m_clients[i].m_tcp.SendPacket(_packet);
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
                    if (Server.instance.m_clients.ContainsKey(i))
                    {
                        if (Server.instance.m_clients[i].m_tcp != null)
                        {
                            Server.instance.m_clients[i].m_tcp.SendPacket(_packet);
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
                if (Server.instance.m_clients.ContainsKey(i))
                {
                    if (Server.instance.m_clients[i].m_udp != null)
                    {
                        Server.instance.m_clients[i].m_udp.SendPacket(_packet);
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
                    if (Server.instance.m_clients.ContainsKey(i))
                    {
                        if (Server.instance.m_clients[i].m_udp != null)
                        {
                            Server.instance.m_clients[i].m_udp.SendPacket(_packet);
                        }
                    }
                }
            }
        }
    }
}
