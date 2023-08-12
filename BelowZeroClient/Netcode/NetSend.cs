using BelowZeroMultiplayerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public static void SpawnMe()
        {
            using (Packet packet = new Packet((int)ClientPackets.SpawnMe))
            {
                packet.Write(NetworkClient.Instance.m_clientId);

                SendTCPData(packet);
            }
        }

        public static void TranformUpdate(Vector3 _pos, Quaternion _rot)
        {
            using (Packet packet = new Packet((int)ClientPackets.TransformUpdate))
            {
                packet.Write(NetworkClient.Instance.m_clientId);
                packet.Write(_pos);
                packet.Write(_rot);

                SendUDPData(packet);
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
