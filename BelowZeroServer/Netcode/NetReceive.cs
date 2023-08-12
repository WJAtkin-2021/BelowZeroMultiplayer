﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class NetReceive
    {
        public static void ConnectedReceived(int _fromClient, Packet _packet)
        {
            // Start the map download to this client
            NetSend.UploadMapToClient(_fromClient);
        }

        public static void HandleClientSpawnMe(int _fromClient, Packet _packet)
        {
            // Grab the client ID
            int clientId = _packet.ReadInt();

            // This is TCP so we can skip the check
            //Logger.Log($"Client: {clientId} wishes to spawn in!");

            NetSend.PlayerSpawned(clientId);
            NetSend.SycPlayerList(clientId);
        }

        public static void HandleTranformUpdate(int _fromClient, Packet _packet)
        {
            // Grab the client ID
            int clientId = _packet.ReadInt();

            // Ensure no spoofing is happening
            if (_fromClient != clientId)
            {
                // TODO: Interface with some kind of anticheat
                return;
            }

            Vector3 position = _packet.ReadVector3();
            Quaternion rotation = _packet.ReadQuaternoin();

            //Logger.Log($"Client: {clientId}: Transform: {position} : {rotation}");

            NetSend.PlayerTransformUpdate(clientId, position, rotation);
        }
    }
}
