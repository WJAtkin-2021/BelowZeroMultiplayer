using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace BelowZeroClient
{
    public class UDP
    {
        public UdpClient m_udpClient;
        public IPEndPoint m_endPoint;

        public UDP(string _ip, int _port)
        {
            m_endPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
        }

        public void Connect(int _localPort)
        {
            m_udpClient = new UdpClient(_localPort);

            m_udpClient.Connect(m_endPoint);
            m_udpClient.BeginReceive(ReceiveCallback, null);

            // Send a blank packet to the server to init the UDP connection
            using (Packet packet = new Packet())
            {
                SendPacket(packet);
            }
        }

        public void SendPacket(Packet _packet)
        {
            try
            {
                _packet.InsertInt(NetworkClient.m_instance.m_clientId);
                if (m_udpClient != null)
                {
                    m_udpClient.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }

            }
            catch (Exception ex)
            {
                Debug.Log($"[UDP:SendPacket] Error sending data via UDP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] data = m_udpClient.EndReceive(_result, ref m_endPoint);
                m_udpClient.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception ex)
            {
                Debug.Log($"[UDP:ReceiveCallback] Error receiving data via UDP: {ex}");
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet packet = new Packet(_data))
            {
                int packetLength = packet.ReadInt();
                _data = packet.ReadBytes(packetLength);
            }

            // Note we have to handle all packets on Unity's main thread
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(_data))
                {
                    int packetId = packet.ReadInt();
                    NetworkClient.m_instance.GetPacketHandlers()[packetId](packet);
                }
            });
        }

        private void Disconnect()
        {
            NetworkClient.m_instance.Disconnect();

            m_endPoint = null;
            m_udpClient = null;
        }
    }
}
