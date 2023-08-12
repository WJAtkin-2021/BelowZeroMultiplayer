using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BelowZeroClient.Utill
{
    public class CreateTechTypeAsyc
    {
        public static IEnumerator CreateNetworkedTechTypeAsyc(TechType objectTechType, Vector3 pos, string guid, System.Action<GameObject> callback = null)
        {
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(objectTechType, true);
            yield return task;
            GameObject gameObjectPrefab = task.GetResult();

            Vector3 toDirection = Vector3.up;
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(gameObjectPrefab, pos, Quaternion.FromToRotation(Vector3.up, toDirection));
            gameObject.AddComponent<NetToken>();
            gameObject.GetComponent<NetToken>().guid = guid;
            gameObject.SetActive(true);

            Pickupable pickupable = gameObject.GetComponent<Pickupable>();
            pickupable.Drop(pos);

            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, objectTechType);
            gameObject.SendMessage("StartConstruction", 1);
            if (callback != null) { callback.Invoke(gameObjectPrefab); }
        }
    }
}
