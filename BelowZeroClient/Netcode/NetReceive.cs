using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroClient
{
    public class NetReceive
    {
        public static void Connected(Packet _packet)
        {
            // Read the packet
            int newClientId = _packet.ReadInt();

            ErrorMessage.AddMessage($"Assigned client ID: {newClientId}");

            // Tell the server we got it and to start up the UDP connection
            NetSend.ConnectedReceived();
            NetworkClient.Instance.StartUDPConnection();
        }

        public static void PlayerDisconnected(Packet _packet)
        {

        }

        public static void HandleMapData(Packet _packet)
        {
            // Grab the length
            int length = _packet.ReadInt();

            // Read the map data
            byte[] mapData = _packet.ReadBytes(length);

            // debug print to log file
            ErrorMessage.AddMessage($"Got map with total bytes: {mapData.Length}");

            string mapLocation = MapDataUtils.SaveMapData(mapData);

            ErrorMessage.AddMessage($"Saved map from server to: {mapLocation}");
        }
    }
}
