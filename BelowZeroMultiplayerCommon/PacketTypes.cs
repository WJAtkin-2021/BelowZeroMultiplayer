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
        SyncPlayerInventory = 16,
        AddInventoryItem = 17,
        ForceTechUnlock = 18,
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
        PlayerInventoryUpdated = 12,
        PlayerCreateToken = 13,
        PlayerUpdateToken = 14,
        PlayerDestroyToken = 15,
    }
}
