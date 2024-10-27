using System.Collections;
using UnityEngine;

namespace BelowZeroClient
{
    public class CreateTechType
    {
        public static IEnumerator CreateNetworkedTechTypeAsyc(TechType objectTechType, Vector3 pos, string guid, System.Action<GameObject> callback = null)
        {
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(objectTechType, true);
            yield return task;
            GameObject gameObjectPrefab = task.GetResult();

            Vector3 toDirection = Vector3.up;
            GameObject gameObject = Object.Instantiate(gameObjectPrefab, pos, Quaternion.FromToRotation(Vector3.up, toDirection));
            NetToken nt = gameObject.AddComponent<NetToken>();
            //nt.GenerateExistingToken();
            gameObject.SetActive(true);

            Pickupable pickupable = gameObject.GetComponent<Pickupable>();
            pickupable.Drop(pos, Vector3.zero, false);

            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, objectTechType);
            gameObject.SendMessage("StartConstruction", 1);
            if (callback != null) { callback.Invoke(gameObjectPrefab); }
        }

        public static IEnumerator CreateTechTypeAndGiveToInventory(TechType _TechType, int _qty)
        {
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(_TechType, true);
            yield return task;
            GameObject gameObjectPrefab = task.GetResult();

            for (int i = 0; i < _qty; i++)
            {
                GameObject gameObject = Object.Instantiate(gameObjectPrefab, Vector3.zero, Quaternion.identity);
                gameObject.SetActive(true);
                Pickupable pickupable = gameObject.GetComponent<Pickupable>();
                Inventory.main.ForcePickup(pickupable);
            }
        }
    }
}
