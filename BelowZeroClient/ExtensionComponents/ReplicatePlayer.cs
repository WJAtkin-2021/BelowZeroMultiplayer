using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BelowZeroClient
{
    public class ReplicatePlayer : MonoBehaviour
    {
        public static ReplicatePlayer m_instance;

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
            NetSend.TranformUpdate(transform.position, MainCameraControl.main.viewModel.transform.rotation);
        }

        public void Teleport(Vector3 _pos)
        {
            StartCoroutine(DelayedTeleport(_pos));
        }

        private IEnumerator DelayedTeleport(Vector3 _pos)
        {
            yield return new WaitForSeconds(0.5f);

            Player playerComp = GetComponent<Player>();
            playerComp.SetPosition(_pos);
            FileLog.Log($"Teleported player to: {_pos}");

            yield return null;
        }
    }
}
