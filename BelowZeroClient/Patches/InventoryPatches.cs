using BelowZeroMultiplayerCommon;
using HarmonyLib;
using UnityEngine;

namespace BelowZeroClient
{
    [HarmonyPatch(typeof(Inventory), "InternalDropItem")]
    class InventoryInternalDropItemPatch
    {
        [HarmonyPostfix]
        static void Postfix(Pickupable pickupable)
        {
            // Generate a GUID for this item so we can keep track of it
            NetToken nt = pickupable.gameObject.AddComponent<NetToken>();
            nt.GenerateNewToken(TokenExchangePolicy.AutomaticHandover, pickupable.GetTechType(), NetworkedEntityType.Pickupable, 1.0f);
            TokenManager.m_instance.AddToken(nt);

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
            NetToken token = pickupable.gameObject.GetComponent<NetToken>();
            if (token != null)
            {
                string tokenGuid = token.guid;
                Component.Destroy(token);
                NetSend.PickupItem(tokenGuid);
                ErrorMessage.AddMessage($"TOKEN WAS DESTROY!");
            }
            else
            {
                ErrorMessage.AddMessage($"TOKEN WAS NULL!");
            }

            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }

    [HarmonyPatch(typeof(Inventory), "ResetInventory")]
    class InventoryResetInventoryPatch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddPending")]
    class InventoryAddPendingPatch
    {
        [HarmonyPostfix]
        static void Postfix(Pickupable pickupable)
        {
            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }

    [HarmonyPatch(typeof(Inventory), "OnEquip")]
    class InventoryOnEquipPatch
    {
        [HarmonyPostfix]
        static void Postfix(string slot, InventoryItem item)
        {
            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }

    [HarmonyPatch(typeof(Inventory), "OnUnequip")]
    class InventoryOnUnequipPatch
    {
        [HarmonyPostfix]
        static void Postfix(string slot, InventoryItem item)
        {
            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }

    [HarmonyPatch(typeof(Inventory), "OnAddItem")]
    class InventoryOnAddItemPatch
    {
        [HarmonyPostfix]
        static void Postfix(InventoryItem item)
        {
            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }

    [HarmonyPatch(typeof(Inventory), "OnRemoveItem")]
    class InventoryOnRemoveItemPatch
    {
        [HarmonyPostfix]
        static void Postfix(InventoryItem item)
        {
            ReplicateInventory.m_instance.MarkInventoryAsDirty();
        }
    }
}
