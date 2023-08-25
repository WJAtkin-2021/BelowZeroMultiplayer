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
        MapDownload = 3,
        SpawnPlayer = 4,
        SycPlayerList = 5,
        PlayerTransformUpdate = 6,
        PlayerDroppedItem = 7,
        PlayerPickedUpItem = 8,
        PlayerUnlockedTechKnowledge = 9,
        PlayerUnlockedPDAEncyclopedia = 10,
        PlayerUpdatedFragmentProgress = 11,
        NewMachineToken = 12,
        UserNameInUse = 13,
        MessageBroadcast = 14,
        SyncUnlocks = 15,
    }

    /// <summary>
    /// Sent from the client to the server
    /// </summary>
    public enum ClientPackets
    {
        ConnectedReceived = 1,
        SpawnMe = 2,
        TransformUpdate = 3,
        DroppedItem = 4,
        PickupItem = 5,
        TechKnowledgeAdded = 6,
        AddedPDAEncyclopedia = 10,
        FragmentProgressUpdated = 11,
    }
}
