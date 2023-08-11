using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroMultiplayerCommon
{
    /// <summary>
    /// Sent from the server to the client
    /// </summary>
    public enum ServerPackets
    {
        Connected = 1,
        PlayerDisconnected = 2,
    }

    /// <summary>
    /// Sent from the client to the server
    /// </summary>
    public enum ClientPackets
    {
        ConnectedReceived = 1,
    }
}
