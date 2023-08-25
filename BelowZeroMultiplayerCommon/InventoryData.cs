using System.Collections.Generic;

namespace BelowZeroMultiplayerCommon
{
    public class InventoryData
    {
        public byte[] serializedStorage;
        public string[] serializedQuickSlots;
        public byte[] serializedEquipment;
        public Dictionary<string, string> serializedEquipmentSlots;
        public byte[] serializedPendingItems;
    }
}
