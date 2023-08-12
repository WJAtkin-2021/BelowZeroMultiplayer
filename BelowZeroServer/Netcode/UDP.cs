using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace BelowZeroServer
{
    public class UDP
    {
        public IPEndPoint m_endPoint;

        private int m_clientId;

        public UDP(int _clientID)
        {
            m_clientId = _clientID;
        }

        public void Connect(IPEndPoint _endPoint)
        {
            m_endPoint = _endPoint;
        }

        public void SendPacket(Packet _packet)
        {
            Server.instance.SendUDPData(m_endPoint, _packet);
        }

        public void HandlePacket(Packet _packet)
        {
            int packetLength = _packet.ReadInt();
            byte[] packetBytes = _packet.ReadBytes(packetLength);

            using (Packet packet = new Packet(packetBytes))
            {
                int packetId = packet.ReadInt();
                Server.instance.m_packetHandlers[packetId](m_clientId, packet);
            }
        }

        public void DisconnectUdp()
        {
            m_endPoint = null;
        }
    }
}
