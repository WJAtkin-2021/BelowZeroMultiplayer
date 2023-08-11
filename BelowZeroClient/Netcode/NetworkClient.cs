﻿using BelowZeroMultiplayerCommon;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BelowZeroClient
{
    public class NetworkClient : MonoBehaviour
    {
        public const int DATA_BUFF_SIZE = 4096;
        public static NetworkClient Instance;

        public Action OnAttemptConnection;
        public Action OnConnected;
        public Action OnFailedToConnect;

        private TCP m_tcp;
        private UDP m_udp;

        public int m_clientId;
        private bool m_isConnected = false;

        public delegate void PacketHandler(Packet packet);
        private static Dictionary<int, PacketHandler> m_packetHandlers;

        void Awake() 
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AttemptServerConnection(string _socketAddr)
        {
            int collonPos;
            string ip;
            int port;

            try
            {
                collonPos = _socketAddr.IndexOf(":", StringComparison.Ordinal);
                ip = _socketAddr.Substring(0, collonPos);
                port = Int32.Parse(_socketAddr.Substring(collonPos + 1));
            }
            catch (Exception ex)
            {
                throw new FormatException("Invalid Socket Format");
            }

            if (string.IsNullOrEmpty(ip))
            {
                throw new FormatException("Invalid Socket Format");
            }
            else if (port <= 0)
            {
                throw new FormatException("Invalid Socket Format");
            }

            AttemptServerConnection(ip, port);
        }

        public void AttemptServerConnection(string _ip, int _port)
        {
            OnAttemptConnection?.Invoke();

            m_tcp = new TCP();
            m_udp = new UDP(_ip, _port);

            m_isConnected = true;
            m_tcp.Connect(_ip, _port, DATA_BUFF_SIZE);
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
            }
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
            };
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}
