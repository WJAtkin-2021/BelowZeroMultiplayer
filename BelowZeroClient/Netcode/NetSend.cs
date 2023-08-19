using BelowZeroMultiplayerCommon;
using Newtonsoft.Json.Linq;
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
                packet.Write(NetworkClient.Instance.m_playerName);

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

        public static void DroppedItem(Pickupable dropable, string token)
        {
            using (Packet packet = new Packet((int)ClientPackets.DroppedItem))
            {
                packet.Write(dropable.GetTechName());
                packet.Write(dropable.gameObject.transform.position);
                packet.Write(token);

                SendTCPData(packet);
            }
        }

        public static void PickupItem(string token)
        {
            using (Packet packet = new Packet((int)ClientPackets.PickupItem))
            {
                packet.Write(token);

                SendTCPData(packet);
            }
        }

        public static void TechKnowledgeAdded(TechType _techType, bool _unlockEncyclopedia, bool _verbose)
        {
            using (Packet packet = new Packet((int)ClientPackets.TechKnowledgeAdded))
            {
                packet.Write((int)_techType);
                packet.Write(_unlockEncyclopedia);
                packet.Write(_verbose);

                SendTCPData(packet);
            }
        }

        public static void AddedPDAEncyclopedia(string _key)
        {
            using (Packet packet = new Packet((int)ClientPackets.AddedPDAEncyclopedia))
            {
                packet.Write(_key);

                SendTCPData(packet);
            }
        }

        public static void FramentProgressUpdated(PDAScanner.Entry entry)
        {
            using (Packet packet = new Packet((int)ClientPackets.FramentProgressUpdated))
            {
                packet.Write((int)entry.techType);

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
