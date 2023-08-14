using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class TCP
    {
        public TcpClient m_tcpClient;

        private ClientConnection m_clientConnection;
        private int m_clientId;
        private int m_dataBuffSize;
        private NetworkStream m_stream;
        private Packet m_receivedPacket;
        private byte[] m_receivedBuff;

        public TCP(int _clientId, int _dataBuffSize)
        {
            m_clientId = _clientId;
            m_dataBuffSize = _dataBuffSize;
        }

        public void Connect(TcpClient _tcpClient, ClientConnection _clientConnection)
        {
            m_clientConnection = _clientConnection;
            m_tcpClient = _tcpClient;

            m_tcpClient.ReceiveBufferSize = m_dataBuffSize;
            m_tcpClient.SendBufferSize = m_dataBuffSize;

            m_stream = m_tcpClient.GetStream();

            m_receivedPacket = new Packet();
            m_receivedBuff = new byte[m_dataBuffSize];

            m_stream.BeginRead(m_receivedBuff, 0, m_dataBuffSize, ReceiveCallback, null);

            // Tell the client we have connected to them
            NetSend.Connected(m_clientId);
        }

        public void SendPacket(Packet _packet)
        {
            try
            {
                if (m_tcpClient != null)
                {
                    m_stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[TCP:SendPacket] Error sending data to client {m_clientId} via TCP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            if (Server.instance.IsServerShuttingDown())
                return;

            try
            {
                int byteLength = m_stream.EndRead(_result);
                if (byteLength <= 0)
                {
                    m_clientConnection.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(m_receivedBuff, data, byteLength);

                m_receivedPacket.Reset(HandleData(data));
                m_stream.BeginRead(m_receivedBuff, 0, m_dataBuffSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Logger.Log($"[TCP:ReceiveCallback] Error receiving TCP data: {ex}");

                // Disconnect for now but we may want reconnect logic in the future
                m_clientConnection.Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int packetLength = 0;
            m_receivedPacket.SetBytes(_data);

            if (m_receivedPacket.UnreadLength() >= 4)
            {
                // Set the packet length by reading the first integer in the buffer
                packetLength = m_receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    // If the packet contains no data we can just return here
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= m_receivedPacket.UnreadLength())
            {
                // Grab the packets raw byte data and its type
                byte[] packetBytes = m_receivedPacket.ReadBytes(packetLength);

                using (Packet packet = new Packet(packetBytes)) 
                {
                    int packetId = packet.ReadInt();
                    Server.instance.m_packetHandlers[packetId](m_clientId, packet);
                }

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

        public void DisconnectTcp()
        {
            if (m_tcpClient != null)
            {
                try
                {
                    m_tcpClient.Close();
                    m_stream = null;
                    m_receivedPacket = null;
                    m_receivedBuff = null;
                    m_tcpClient = null;
                }
                catch (Exception ex)
                {
                    Logger.Log($"[TCP:DisconnectTcp] Error {ex}");
                }
            }
        }
    }
}
