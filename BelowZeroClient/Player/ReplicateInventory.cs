using HarmonyLib;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BelowZeroMultiplayerCommon;
using System.Collections;

namespace BelowZeroClient
{
    public class ReplicateInventory : MonoBehaviour
    {
        private const float MIN_TIME_BETWEEN_INVENTORY_UPLOAD = 5.0f;

        public static ReplicateInventory m_instance;

        private bool m_inventoryIsDirty = false;
        private bool m_canSave = true;


        public void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void Update()
        {
            if (m_inventoryIsDirty && m_canSave)
            {
                try
                {
                    StartCoroutine(SaveTimer());
                    SyncInventoryToServer();
                }
                catch(Exception ex)
                {
                    FileLog.Log($"{ex}");
                }

                m_inventoryIsDirty = false;
            }
        }

        public void MarkInventoryAsDirty()
        {
            m_inventoryIsDirty = true;
        }

        public void SyncInventoryToServer()
        {
            try
            {
                FileLog.Log($"SyncInventoryToServer");
                Inventory inv = gameObject.GetComponent<Inventory>();
                if (inv != null)
                {
                    FileLog.Log($"got Inventory");
                    // Create the serializer and ask the inventory to "Save" their data
                    ProtobufSerializer serializer = ProtobufSerializerPool.GetProxy();
                    inv.OnProtoSerialize(serializer);

                    // Grab this data
                    FileLog.Log($"created serializer");
                    InventoryData data = new InventoryData();
                    data.serializedStorage = inv.serializedStorage;
                    data.serializedQuickSlots = inv.serializedQuickSlots;
                    data.serializedEquipment = inv.serializedEquipment;
                    data.serializedEquipmentSlots = inv.serializedEquipmentSlots;
                    data.serializedPendingItems = inv.serializedPendingItems;
                    FileLog.Log($"got all data calling PlayerInventoryUpdated");

                    // Send to the server
                    NetSend.PlayerInventoryUpdated(data);
                    FileLog.Log($"Called PlayerInventoryUpdated");
                }
                else
                {
                    ErrorMessage.AddError("[ReplicateInventory] Unable to save inventory as Inventory component could not be found!");
                    FileLog.Log("[ReplicateInventory] Unable to save inventory as Inventory component could not be found!");
                }
            }
            catch (Exception ex)
            {
                FileLog.Log($"{ex}");
            }
        }

        public void LoadInventoryData(InventoryData _data)
        {
            Inventory inv = gameObject.GetComponent<Inventory>();
            if (inv != null)
            {
                // Set the data on the serialized data members
                inv.serializedStorage = _data.serializedStorage;
                inv.serializedQuickSlots = _data.serializedQuickSlots;
                inv.serializedEquipment = _data.serializedEquipment;
                inv.serializedEquipmentSlots = _data.serializedEquipmentSlots;
                inv.serializedPendingItems = _data.serializedPendingItems;

                // Create the serializer and ask the inventory to "Load" the data
                ProtobufSerializer serializer = ProtobufSerializerPool.GetProxy();
                StartCoroutine(inv.OnProtoDeserializeAsync(serializer));
            }
            else
            {
                ErrorMessage.AddError("[ReplicateInventory] Unable to load inventory from server as Inventory component could not be found!");
                FileLog.Log("[ReplicateInventory] Unable to load inventory from server as Inventory component could not be found!");
            }
        }

        private IEnumerator SaveTimer()
        {
            m_canSave = false;

            yield return new WaitForSeconds(MIN_TIME_BETWEEN_INVENTORY_UPLOAD);

            m_canSave = true;

            yield return null;
        }
    }
}
