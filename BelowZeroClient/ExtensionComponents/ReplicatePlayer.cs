using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BelowZeroClient
{
    public class ReplicatePlayer : MonoBehaviour
    {
        public static ReplicatePlayer m_instance;
        Player playerComp;

        public void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this;
                playerComp = GetComponent<Player>();
            }
            else
            {
                Destroy(this);
            }
        }

        public void Update()
        {
            NetSend.TranformUpdate(transform.position, MainCameraControl.main.viewModel.transform.rotation, playerComp.IsInside());
        }

        public void Teleport(Vector3 _pos, Quaternion _rot, bool _isInside)
        {
            StartCoroutine(DelayedTeleport(_pos, _rot, _isInside));
        }

        private IEnumerator DelayedTeleport(Vector3 _pos, Quaternion _rot, bool _isInside)
        {
            yield return new WaitForSeconds(0.5f);
          
            playerComp.SetPosition(_pos, _rot);

            UnderwaterMotor underWaterController = playerComp.GetComponent<UnderwaterMotor>(); 
            GroundMotor groundController = playerComp.GetComponent<GroundMotor>();
            underWaterController.SetEnabled(enabled: false);
            groundController.SetEnabled(enabled: false);
            groundController.gravity = 0.0f;

            yield return new WaitForSeconds(2.5f);
            if (playerComp.IsUnderwaterForSwimming())
            {
                underWaterController.SetEnabled(enabled: true);
            }
            else
            {
                groundController.airAcceleration = 0f;
                groundController.SetEnabled(enabled: true);
                groundController.gravity = 12.0f;
            }
            if (_isInside)
            {
                playerComp.currentInterior = GetCurrentSpace(_pos);
            }
            else
            {
                playerComp.currentInterior = null;
            }

            yield return null;
        }

        private IInteriorSpace GetCurrentSpace(Vector3 _playerPos)
        {
            // Try and find ll IInteriorSpace interfaces
            GameObject[] allGO = UnityEngine.Object.FindObjectsOfType<GameObject>();
            List<IInteriorSpace> spaces = new List<IInteriorSpace>();
            for (int i = 0; i < allGO.Length; i++)
            {
                Component[] comps = allGO[i].GetComponents<Component>();
                for (int j = 0; j < comps.Length; j++)
                {
                    //FileLog.Log($"Checking: {comps[j].GetType()}");
                    if (typeof(IInteriorSpace).IsAssignableFrom(comps[j].GetType()))
                    {
                        spaces.Add((IInteriorSpace)comps[j]);
                    }
                }
            }

            // Making the assumption here that there will always been atleast one Interior...
            IInteriorSpace closest = spaces[0];
            float distance = Vector3.Distance(closest.GetGameObject().transform.position, _playerPos);
            for (int i = 1; i < spaces.Count; i++)
            {
                float testDist = Vector3.Distance(spaces[i].GetGameObject().transform.position, _playerPos);
                if (testDist < distance)
                {
                    closest = spaces[i];
                    distance = testDist;
                }
            }

            return closest;
        }
    }
}
