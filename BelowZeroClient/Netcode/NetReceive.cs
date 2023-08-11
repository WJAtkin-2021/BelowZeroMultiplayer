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
    }
}
