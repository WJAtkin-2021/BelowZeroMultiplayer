using BelowZeroMultiplayerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroClient
{
    public class NetSend
    {
        public static void ConnectedReceived()
        {
            // TODO: Send a user name with this packet
            using (Packet packet = new Packet((int)ClientPackets.ConnectedReceived))
            {
                packet.Write(NetworkClient.Instance.m_clientId);

                SendTCPData(packet);
            }
        }

        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();

            try
            {
                NetworkClient.Instance.m_tcp.SendPacket(_packet);
            }
            catch (Exception ex)
            {
                ErrorMessage.AddMessage($"Error {ex}");
            }
        }

        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            NetworkClient.Instance.m_udp.SendPacket(_packet);
        }
    }
}
