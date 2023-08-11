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
                Server.instance.m_clients[i].m_tcp.SendPacket(_packet);
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                if (i != _exceptClient)
                {
                    Server.instance.m_clients[i].m_tcp.SendPacket(_packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                Server.instance.m_clients[i].m_udp.SendPacket(_packet);
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MAX_PLAYERS; i++)
            {
                if (i != _exceptClient)
                {
                    Server.instance.m_clients[i].m_udp.SendPacket(_packet);
                }
            }
        }
    }
}
