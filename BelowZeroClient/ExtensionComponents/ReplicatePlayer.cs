using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static VFXParticlesPool;

namespace BelowZeroClient
{
    public class ReplicatePlayer : MonoBehaviour
    {
        public void Update()
        {
            NetSend.TranformUpdate(transform.position, MainCameraControl.main.viewModel.transform.rotation);
        }
    }
}
