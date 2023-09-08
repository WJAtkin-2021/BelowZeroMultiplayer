using BelowZeroMultiplayerCommon;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BelowZeroClient
{
    public class NetSend
    {
        public static void ConnectedReceived(string _serverGuid)
        {
            using (Packet packet = new Packet(ClientPackets.ConnectedReceived))
            {
                packet.Write(NetworkClient.m_instance.m_clientId);
                packet.Write(NetworkClient.m_instance.m_playerName);
                packet.Write(ApplicationSettings.GetCredentailToken(_serverGuid));

                SendTCPData(packet);
            }
        }

        public static void SpawnMe()
        {
            using (Packet packet = new Packet(ClientPackets.SpawnMe))
            {
                packet.Write(NetworkClient.m_instance.m_clientId);
                packet.Write(NetworkClient.m_instance.m_playerName);

                SendTCPData(packet);
            }
        }

        public static void TranformUpdate(Vector3 _pos, Quaternion _rot, bool _isInside)
        {
            using (Packet packet = new Packet(ClientPackets.TransformUpdate))
            {
                packet.Write(NetworkClient.m_instance.m_clientId);
                packet.Write(_pos);
                packet.Write(_rot);
                packet.Write(_isInside);

                SendUDPData(packet);
            }
        }

        [Obsolete("Obsolete, now done automatically by token")]
        public static void DroppedItem(Pickupable dropable, string token)
        {
            //using (Packet packet = new Packet((int)ClientPackets.DroppedItem))
            //{
            //    packet.Write(dropable.GetTechName());
            //    packet.Write(dropable.gameObject.transform.position);
            //    packet.Write(token);
            //
            //    SendTCPData(packet);
            //}
        }

        public static void PickupItem(string token)
        {
            using (Packet packet = new Packet(ClientPackets.PickupItem))
            {
                packet.Write(token);

                SendTCPData(packet);
            }
        }

        public static void TechKnowledgeAdded(TechType _techType, bool _unlockEncyclopedia, bool _verbose)
        {
            using (Packet packet = new Packet(ClientPackets.TechKnowledgeAdded))
            {
                packet.Write(_techType);
                packet.Write(_unlockEncyclopedia);
                packet.Write(_verbose);

                SendTCPData(packet);
            }
        }

        public static void AddedPDAEncyclopedia(PDAScanner.EntryData entryData)
        {
            using (Packet packet = new Packet(ClientPackets.AddedPDAEncyclopedia))
            {
                packet.Write(entryData.encyclopedia);
                packet.Write(entryData.key);

                SendTCPData(packet);
            }
        }

        public static void FragmentProgressUpdated(PDAScanner.Entry entry)
        {
            using (Packet packet = new Packet(ClientPackets.FragmentProgressUpdated))
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(entry.techType);

                packet.Write(entry.techType);
                packet.Write(entry.unlocked);
                packet.Write(entryData.totalFragments);

                SendTCPData(packet);
            }
        }

        public static void PlayerInventoryUpdated(InventoryData _data)
        {
            using (Packet packet = new Packet(ClientPackets.PlayerInventoryUpdated))
            {
                packet.Write(_data.serializedStorage.Length);
                packet.Write(_data.serializedStorage);
                packet.Write(_data.serializedQuickSlots.Length);
                for (int i = 0; i < _data.serializedQuickSlots.Length; i++)
                {
                    packet.Write(_data.serializedQuickSlots[i]);
                }
                packet.Write(_data.serializedEquipment.Length);
                packet.Write(_data.serializedEquipment);
                packet.Write(_data.serializedEquipmentSlots.Count);
                foreach (KeyValuePair<string, string> entry in _data.serializedEquipmentSlots)
                {
                    packet.Write(entry.Key);
                    packet.Write(entry.Value);
                }
                packet.Write(_data.serializedPendingItems.Length);
                packet.Write(_data.serializedPendingItems);

                SendTCPData(packet);
            }
        }

        public static void PlayerCreateToken(NetToken _token)
        {
            using (Packet packet = new Packet(ClientPackets.PlayerCreateToken))
            {
                packet.Write(_token.guid);
                packet.Write(_token.tokenExchangePolicy);
                packet.Write(_token.associatedTechType);
                packet.Write(_token.networkedEntityType);
                packet.Write(_token.tickRate);
                packet.Write(_token.transform.position);
                packet.Write(_token.transform.rotation);
                packet.Write(_token.transform.localScale);

                SendTCPData(packet);
            }
        }

        public static void PlayerUpdateToken(NetToken _token)
        {
            using (Packet packet = new Packet(ClientPackets.PlayerUpdateToken))
            {
                packet.Write(_token.guid);
                packet.Write(_token.transform.position);
                packet.Write(_token.transform.rotation);
                packet.Write(_token.transform.localScale);

                SendUDPData(packet);
            }
        }

        public static void PlayerUpdatedTokenData(NetToken _token)
        {
            using (Packet packet = new Packet(ClientPackets.PlayedUpdateTokenData))
            {
                packet.Write(_token.guid);
                packet.Write(_token.tokenExchangePolicy);
                packet.Write(_token.tickRate);

                SendTCPData(packet);
            }
        }

        public static void TryAcquireToken(NetToken _token)
        {
            using (Packet packet = new Packet(ClientPackets.PlayerAcquireToken))
            {
                packet.Write(_token.guid);

                SendTCPData(packet);
            }
        }

        public static void PlayerDestroyToken(NetToken _token)
        {
            ErrorMessage.AddMessage("Sending packet PlayerDestroyToken");

            using (Packet packet = new Packet(ClientPackets.PlayerDestroyToken))
            {
                packet.Write(_token.guid);

                SendTCPData(packet);
            }
        }

        #region SendImplementations

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

        #endregion
    }
}
