using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using BelowZeroMultiplayerCommon;

namespace BelowZeroServer
{
    public class Server
    {
        public const int MAX_PLAYERS = 8;

        private int m_port;
        public bool m_isShuttingDown = false;
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public Dictionary<int, PacketHandler> m_packetHandlers;
        public Dictionary<int, ClientConnection> m_clients = new Dictionary<int, ClientConnection>();

        private TcpListener m_tcpListener;
        private UdpClient m_udpListener;

        public static Server m_instance;

        private byte[] m_mapData;

        public Server()
        {
            m_instance = this;

            m_mapData = File.ReadAllBytes("TestMap.zip");
        }

        public void StartServer(int _port)
        {
            m_port = _port;

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                m_clients.Add(i, new ClientConnection(i, ""));
            }
            InitPacketHandlers();

            m_tcpListener = new TcpListener(IPAddress.Any, m_port);
            m_tcpListener.Start();
            m_tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            m_udpListener = new UdpClient(m_port);
            m_udpListener.BeginReceive(UDPReceiveCallback, null);

            Logger.Log($"Server Started On Port: {m_port}");
        }

        public void StopServer()
        {
            m_isShuttingDown = true;

            // Stop the listeners
            m_tcpListener.Stop();
            m_udpListener.Close();

            // Disconnect all clients
            for (int i = 0; i < m_clients.Count; i++)
            {
                m_clients[i].Disconnect();
            }
        }

        private void TCPConnectCallback(IAsyncResult _result)
        {
            if (m_isShuttingDown)
                return;

            TcpClient client = m_tcpListener.EndAcceptTcpClient(_result);
            m_tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            Logger.Log($"Inbound connection from: {client.Client.RemoteEndPoint}");

            // Go through each client object and find a free slot
            for (int i = 1; i <= MAX_PLAYERS; i++)
            {
                if (m_clients[i].m_tcp.m_tcpClient == null)
                {
                    m_clients[i].m_tcp.Connect(client, m_clients[i]);
                    return;
                }
            }

            Logger.Log($"{client.Client.RemoteEndPoint} failed to connect as the server is full");
        }

        private void UDPReceiveCallback(IAsyncResult _result)
        {
            if (m_isShuttingDown)
                return;

            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = m_udpListener.EndReceive(_result, ref clientEndPoint);
                m_udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();

                    if (clientId == 0)
                    {
                        return;
                    }

                    if (m_clients[clientId].m_udp.m_endPoint == null)
                    {
                        // This would be new connection that we need to establish
                        m_clients[clientId].m_udp.Connect(clientEndPoint);
                        return;
                    }

                    // Security check, ensure that the client id matches the endpoint of the client as
                    // someone may be impersonating another client
                    if (m_clients[clientId].m_udp.m_endPoint.ToString() == clientEndPoint.ToString())
                    {
                        m_clients[clientId].m_udp.HandlePacket(packet);
                    }
                    else
                    {
                        Logger.Log($"[SECURITY ALERT] Client at endpoint: {clientEndPoint} is trying to impersonate: {m_clients[clientId].m_udp.m_endPoint}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[Server:UDPReceiveCallback] Error receiving UDP data: {ex}");

                Logger.Log($"Restarting UDP Stream");
                m_udpListener.Close();
                m_udpListener = new UdpClient(m_port);
                m_udpListener.BeginReceive(UDPReceiveCallback, null);
            }
        }

        public void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    m_udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[Server:SendUDPData] Error sending UDP data to {_clientEndPoint}: {ex}");
            }
        }

        private void InitPacketHandlers()
        {
            m_packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.ConnectedReceived, NetReceive.ConnectedReceived },
                { (int)ClientPackets.SpawnMe, NetReceive.HandleClientSpawnMe },
                { (int)ClientPackets.TransformUpdate, NetReceive.HandleTranformUpdate },
                { (int)ClientPackets.DroppedItem, NetReceive.HandleDroppedItem },
                { (int)ClientPackets.PickupItem, NetReceive.HandlePickupItem },
                { (int)ClientPackets.TechKnowledgeAdded, NetReceive.HandleTechKnowledgeAdded },
                { (int)ClientPackets.AddedPDAEncyclopedia, NetReceive.HandleUnlockedPDAEncyclopedia },
                { (int)ClientPackets.FragmentProgressUpdated, NetReceive.HandleFragmentProgressUpdated },
                { (int)ClientPackets.PlayerInventoryUpdated, NetReceive.HandlePlayerInventoryUpdated },
                { (int)ClientPackets.PlayerCreateToken, NetReceive.HandlePlayerCreateToken },
                { (int)ClientPackets.PlayerUpdateToken, NetReceive.HandlePlayerUpdateToken },
                { (int)ClientPackets.PlayerDestroyToken, NetReceive.HandlePlayerDestroyToken },
            };
        }

        public static string ResolvePlayerName(int _clientId)
        {
            return m_instance.m_clients[_clientId].m_clientName;
        }

        public static string ResolvePartialPlayerName(string _partialClientName)
        {
            // NOTE: This implementation will have a limitation where if multiple players have a similar name
            // i.e. BillyBob vs. BillNile and only Bill is supplied to the function it will only retrieve
            // the first match it comes across. What we really should do at some point is TODO: change the return
            // type to something like NameResolveResult that return the full name of every player that matches
            // this
            foreach (KeyValuePair<int, ClientConnection> entry in m_instance.m_clients)
            {
                if (entry.Value.m_clientName.StartsWith(_partialClientName, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value.m_clientName;
                }
            }

            // Fall back if no player is found to start with this name
            string lowercaseName = _partialClientName.ToLower();
            foreach (KeyValuePair<int, ClientConnection> entry in m_instance.m_clients)
            {
                if (entry.Value.m_clientName.ToLower().Contains(lowercaseName))
                {
                    return entry.Value.m_clientName;
                }
            }

            return null;
        }

        public static int ResolveClientId(string _fullPlayerName)
        {
            foreach (KeyValuePair<int, ClientConnection> entry in m_instance.m_clients)
            {
                if (entry.Value.m_clientName == _fullPlayerName)
                {
                    return entry.Value.m_clientId;
                }
            }

            return -1;
        }

        public bool IsServerShuttingDown()
        {
            return m_isShuttingDown;
        }

        public byte[] GetMapData()
        {
            return m_mapData;
        }
    }
}
