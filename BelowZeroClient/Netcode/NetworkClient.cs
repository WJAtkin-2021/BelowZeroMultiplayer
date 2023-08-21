using BelowZeroMultiplayerCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace BelowZeroClient
{
    public class NetworkClient : MonoBehaviour
    {
        public const int DATA_BUFF_SIZE = 4096;
        public static NetworkClient m_instance;

        public Action OnAttemptConnection;
        public Action OnConnected;
        public Action OnFailedToConnect;

        public TCP m_tcp;
        public UDP m_udp;

        public int m_clientId;
        public string m_playerName;
        
        private bool m_isConnected = false;
        private bool m_isMapLoaded = false;

        public delegate void PacketHandler(Packet packet);
        private static Dictionary<int, PacketHandler> m_packetHandlers;

        public Dictionary<int,RemotePlayer> m_remotePlayers = new Dictionary<int, RemotePlayer>();

        void Awake() 
        {
            if (m_instance == null)
            {
                m_instance = this;
                DontDestroyOnLoad(gameObject);
                InitPacketHandlers();
                StartCoroutine(CheckRemoteViews());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AttemptServerConnection(string _socketAddr, string _playerName)
        {
            if (string.IsNullOrEmpty(_playerName))
            {
                ErrorMessage.AddMessage("Please Enter A Player Name");
                OnFailedToConnect?.Invoke();
                return;
            }

            int collonPos;
            string ip;
            int port;
            m_playerName = _playerName;

            try
            {
                collonPos = _socketAddr.IndexOf(":", StringComparison.Ordinal);
                ip = _socketAddr.Substring(0, collonPos);
                port = Int32.Parse(_socketAddr.Substring(collonPos + 1));
            }
            catch (Exception ex)
            {
                OnFailedToConnect?.Invoke();
                throw new FormatException("Invalid Socket Format");
            }

            if (string.IsNullOrEmpty(ip))
            {
                OnFailedToConnect?.Invoke();
                throw new FormatException("Invalid Socket Format");
            }
            else if (port <= 0)
            {
                OnFailedToConnect?.Invoke();
                throw new FormatException("Invalid Socket Format");
            }

            AttemptServerConnection(ip, port);
        }

        public void AttemptServerConnection(string _ip, int _port)
        {
            ErrorMessage.AddMessage("Connecting...");

            OnAttemptConnection?.Invoke();

            m_tcp = new TCP();
            m_udp = new UDP(_ip, _port);

            m_isConnected = true;
            m_tcp.Connect(_ip, _port, DATA_BUFF_SIZE);
        }

        public void AddRemotePlayer(int _clientId, string _clientName, Vector3 _pos)
        {
            RemotePlayer player = new RemotePlayer(_clientId, _clientName, _pos);
            m_remotePlayers[_clientId] = player;
        }

        public void Disconnect()
        {
            if (m_isConnected)
            {
                try
                {
                    m_isConnected = false;
                    m_tcp.m_tcpClient.Close();
                    m_udp.m_udpClient.Close();
                }
                catch { }
                finally
                {
                    if (m_isMapLoaded)
                    {
                        StartCoroutine(HandleDisconnectDelayed());
                    }
                }
            }
        }

        public void StartUDPConnection()
        {
            m_udp.Connect(((IPEndPoint)m_tcp.m_tcpClient.Client.LocalEndPoint).Port);
        }

        public Dictionary<int, PacketHandler> GetPacketHandlers()
        {
            return m_packetHandlers;
        }

        private void InitPacketHandlers()
        {
            m_packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ServerPackets.Connected, NetReceive.Connected },
                { (int)ServerPackets.PlayerDisconnected, NetReceive.PlayerDisconnected },
                { (int)ServerPackets.MapDownload, NetReceive.HandleMapData },
                { (int)ServerPackets.SpawnPlayer, NetReceive.HandleSpawnPlayer },
                { (int)ServerPackets.SycPlayerList, NetReceive.HandleSycPlayerList },
                { (int)ServerPackets.PlayerTransformUpdate, NetReceive.HandlePlayerTransformUpdate },
                { (int)ServerPackets.PlayerDroppedItem, NetReceive.handlePlayerDroppedItem },
                { (int)ServerPackets.PlayerPickedUpItem, NetReceive.handlePlayerPickedUpItem },
                { (int)ServerPackets.PlayerUnlockedTechKnowledge, NetReceive.HandlePlayerUnlockedTechKnowledge },
                { (int)ServerPackets.PlayerUnlockedPDAEncyclopedia, NetReceive.HandlePlayerUnlockedPDAEncyclopedia },
                { (int)ServerPackets.PlayerUpdatedFragmentProgress, NetReceive.HandlePlayerUpdatedFragmentProgress },
                { (int)ServerPackets.NewMachineToken, NetReceive.HandleNewMachineToken },
                { (int)ServerPackets.UserNameInUse, NetReceive.HandleUserNameInUse },
            };
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        private IEnumerator HandleDisconnectDelayed()
        {
            ErrorMessage.AddError("Connection to server was lost!");

            yield return new WaitForSeconds(1);

            ErrorMessage.AddError("Returning to main menu...");

            yield return new WaitForSeconds(3);

            IngameMenu.main.QuitGame(false);
        }

        private IEnumerator CheckRemoteViews()
        {
            yield return new WaitForSeconds(5);

            foreach (KeyValuePair<int, RemotePlayer> entry in m_remotePlayers)
            {
                if (!entry.Value.m_spawnedIn)
                {
                    entry.Value.AttemptSpawn();
                }
            }

            StartCoroutine(CheckRemoteViews());
            yield return null;
        }
    }
}
