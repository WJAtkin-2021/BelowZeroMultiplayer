﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace BelowZeroClient
{
    public class RemotePlayer
    {
        public static Vector3 START_POS = new Vector3(-309.5f, 17.75f, 255.0f);
        public const string CHARACTER_MODEL = "player_view_female";
        public int m_clientId = 0;
        public bool spawnedIn = false;
        public string m_clientName = "";

        private GameObject m_viewModel;
        private PingInstance m_pingInstance;

        public RemotePlayer(int _clientId, string _clientName)
        { 
            m_clientId = _clientId;
            m_clientName = _clientName;

            ErrorMessage.AddMessage($"Player {_clientId} is joining...");
            
            AttemptSpawn();
        }

        public void UpdateTransform(Vector3 _pos, Quaternion _rot)
        {
            if (spawnedIn)
            {
                m_viewModel.transform.position = _pos;
                m_viewModel.transform.rotation = _rot;
            }
        }

        public void AttemptSpawn()
        {
            GameObject targetVisual = GameObject.Find(CHARACTER_MODEL);

            if (targetVisual != null)
            {
                targetVisual.GetComponentInParent<Player>().staticHead.shadowCastingMode = ShadowCastingMode.On;
                m_viewModel = GameObject.Instantiate<GameObject>(targetVisual, START_POS, Quaternion.identity);
                targetVisual.GetComponentInParent<Player>().staticHead.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

                m_pingInstance = m_viewModel.AddComponent<PingInstance>();
                m_pingInstance.displayPingInManager = false;
                m_pingInstance.visitable = false;
                m_pingInstance.origin = m_viewModel.transform;
                m_pingInstance.SetLabel($"{m_clientName}");
                m_pingInstance.SetType(PingType.Signal);
                m_pingInstance.SetVisible(true);
                m_pingInstance.SetColor(2);
                m_pingInstance.minDist = 2.5f;

                spawnedIn = true;
            }
        }
    }
}
