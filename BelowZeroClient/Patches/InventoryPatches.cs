using HarmonyLib;

namespace BelowZeroClient
{
    [HarmonyPatch(typeof(Inventory), "InternalDropItem")]
    class InventoryInternalDropItemPatch
    {
        [HarmonyPostfix]
        static void Postfix(Pickupable pickupable)
        {
            // Generate a GUID for this item so we can keep track of it
            pickupable.gameObject.AddComponent<NetToken>();
            string token = System.Guid.NewGuid().ToString();
            pickupable.gameObject.GetComponent<NetToken>().guid = token;

            // Tell the server we dropped it
            NetSend.DroppedItem(pickupable, token);

            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }

    [HarmonyPatch(typeof(Inventory), "Pickup")]
    class InventoryPickupPatch
    {
        [HarmonyPostfix]
        static void Postfix(Pickupable pickupable)
        {
            // If this was a networked item we need to tell the server that we got it and
            // that it need to remove it
            if (pickupable.gameObject.GetComponent<NetToken>() != null)
            {
                NetSend.PickupItem(pickupable.gameObject.GetComponent<NetToken>().guid);
            }

            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }
}
