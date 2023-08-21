using System;
using System.Net.Sockets;
using UnityEngine;

namespace BelowZeroClient
{
    public class TCP
    {
        public TcpClient m_tcpClient;

        private NetworkStream m_stream;
        private Packet m_receivedPacket;
        private byte[] m_receiveBuffer;
        private int m_dataBufferSize;

        public void Connect(string _ip, int _port, int _dataBuffSize)
        {
            Debug.Log($"Connecting to: {_ip}:{_port}");

            m_tcpClient = new TcpClient
            {
                ReceiveBufferSize = _dataBuffSize,
                SendBufferSize = _dataBuffSize,
            };

            m_dataBufferSize = _dataBuffSize;
            m_receiveBuffer = new byte[m_dataBufferSize];
            m_tcpClient.BeginConnect(_ip, _port, ConnectCallback, m_tcpClient);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            if (!m_tcpClient.Connected)
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    NetworkClient.m_instance.OnFailedToConnect?.Invoke();
                    ErrorMessage.AddMessage("Failed to connect to server");
                });

                return;
            }

            m_tcpClient.EndConnect(_result);
            m_stream = m_tcpClient.GetStream();
            m_receivedPacket = new Packet();
            m_stream.BeginRead(m_receiveBuffer, 0, m_dataBufferSize, ReceiveCallback, null);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                NetworkClient.m_instance.OnConnected?.Invoke();
            });
        }

        public void SendPacket(Packet _packet)
        {
            try
            {
                if (m_tcpClient != null)
                {
                    m_stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
                else
                {
                    ErrorMessage.AddMessage($"TCP client is null!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TCP:SendData] Failed to send data to server: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int byteLength = m_stream.EndRead(_result);
                if (byteLength <= 0)
                {
                    Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(m_receiveBuffer, data, byteLength);

                m_receivedPacket.Reset(HandleData(data));
                m_stream.BeginRead(m_receiveBuffer, 0, m_dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TCP:ReceiveCallback] Failed to receive data from server: {ex}");
            }
        }

        private bool HandleData(byte[] _data)
        {
            int packetLength = 0;

            m_receivedPacket.SetBytes(_data);

            if (m_receivedPacket.UnreadLength() >= 4)
            {
                // Check if the servers packet contained any data
                packetLength = m_receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= m_receivedPacket.UnreadLength())
            {
                // Grab the packets raw byte data and its type
                byte[] packetBytes = m_receivedPacket.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        NetworkClient.m_instance.GetPacketHandlers()[packetId](packet);
                    }
                });

                // Check to see if there is another packet of data waiting
                packetLength = 0;
                if (m_receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = m_receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        // Can safely stop as the packet is now empty
                        return true;
                    }
                }
            }
            
            if (packetLength <= 1)
            {
                // Can safely stop as the packet is now empty
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            NetworkClient.m_instance.Disconnect();

            m_stream = null;
            m_receivedPacket = null;
            m_receiveBuffer = null;
            m_tcpClient = null;
        }
    }
}
