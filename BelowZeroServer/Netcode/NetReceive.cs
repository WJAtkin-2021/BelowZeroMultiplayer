using System;
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
    }
}
