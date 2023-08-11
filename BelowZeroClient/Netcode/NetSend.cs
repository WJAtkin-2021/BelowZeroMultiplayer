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
            ErrorMessage.AddMessage("Calling ConnectedReceived");

            // TODO: Send a user name with this packet
            using (Packet packet = new Packet((int)ClientPackets.ConnectedReceived))
            {
                packet.Write(NetworkClient.Instance.m_clientId);

                SendTCPData(packet);
            }
        }

        private static void SendTCPData(Packet _packet)
        {
            ErrorMessage.AddMessage("Calling SendTCPData");
            _packet.WriteLength();
            ErrorMessage.AddMessage("Calling SendPacket");

            try
            {
                ErrorMessage.AddMessage(NetworkClient.Instance.name);
                NetworkClient.Instance.m_tcp.SendPacket(_packet);
            }
            catch (Exception ex)
            {
                ErrorMessage.AddMessage($"Error {ex}");
            }
            ErrorMessage.AddMessage("Finn");
        }

        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            NetworkClient.Instance.m_udp.SendPacket(_packet);
        }
    }
}
