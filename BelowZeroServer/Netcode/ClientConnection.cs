using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class ClientConnection
    {
        public const int DATA_BUFFER_SIZE = 4096;

        public int m_clientId;
        public string m_clientName = "";
        public TCP m_tcp;
        public UDP m_udp;

        public ClientConnection(int _clientId, string _clientName)
        {
            m_clientId = _clientId;
            m_clientName = _clientName;
            m_tcp = new TCP(m_clientId, DATA_BUFFER_SIZE);
            m_udp = new UDP(m_clientId);
        }

        public void Disconnect()
        {
            if (m_tcp.m_tcpClient != null)
            {
                try
                {
                    Logger.Log($"{m_tcp.m_tcpClient.Client.RemoteEndPoint} has disconnected"); 
                    NetSend.PlayerDisconnected(m_clientId); 
                }
                catch (Exception ex)
                {
                    Logger.Log($"[ClientConnection:Disconnect] Error while disconnecting client: {ex}");
                }
                finally 
                {
                    m_tcp.DisconnectTcp(); 
                    m_udp.DisconnectUdp();
                }
            }
        }
    }
}
