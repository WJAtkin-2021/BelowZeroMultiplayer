﻿using BelowZeroMultiplayerCommon;
using System;
using UnityEngine;

namespace BelowZeroClient
{
    public class NetSend
    {
        public static void ConnectedReceived(string _serverGuid)
        {
            using (Packet packet = new Packet((int)ClientPackets.ConnectedReceived))
            {
                packet.Write(NetworkClient.m_instance.m_clientId);
                packet.Write(NetworkClient.m_instance.m_playerName);
                packet.Write(ApplicationSettings.GetCredentailToken(_serverGuid));

                SendTCPData(packet);
            }
        }

        public static void SpawnMe()
        {
            using (Packet packet = new Packet((int)ClientPackets.SpawnMe))
            {
                packet.Write(NetworkClient.m_instance.m_clientId);
                packet.Write(NetworkClient.m_instance.m_playerName);

                SendTCPData(packet);
            }
        }

        public static void TranformUpdate(Vector3 _pos, Quaternion _rot, bool _isInside)
        {
            using (Packet packet = new Packet((int)ClientPackets.TransformUpdate))
            {
                packet.Write(NetworkClient.m_instance.m_clientId);
                packet.Write(_pos);
                packet.Write(_rot);
                packet.Write(_isInside);

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

        public static void FragmentProgressUpdated(PDAScanner.Entry entry)
        {
            using (Packet packet = new Packet((int)ClientPackets.FragmentProgressUpdated))
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(entry.techType);

                int techType = (int)entry.techType;
                int currentFragments = entry.unlocked;
                int totalFragments = entryData.totalFragments;

                packet.Write(techType);
                packet.Write(currentFragments);
                packet.Write(totalFragments);

                SendTCPData(packet);
            }
        }

        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();

            try
            {
                NetworkClient.m_instance.m_tcp.SendPacket(_packet);
            }
            catch (Exception ex)
            {
                ErrorMessage.AddMessage($"Error {ex}");
            }
        }

        private static void SendUDPData(Packet _packet)
        {
            _packet.WriteLength();
            NetworkClient.m_instance.m_udp.SendPacket(_packet);
        }
    }
}
