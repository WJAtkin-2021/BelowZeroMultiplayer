using UnityEngine;
using UnityEngine.Rendering;

namespace BelowZeroClient
{
    public class RemotePlayer
    {
        public const string CHARACTER_MODEL = "player_view_female";

        public int m_clientId = 0;
        public bool m_spawnedIn = false;
        public string m_clientName = "";

        private GameObject m_viewModel;
        private PingInstance m_pingInstance;
        private Vector3 m_spawnPos;

        public RemotePlayer(int _clientId, string _clientName, Vector3 _pos)
        { 
            m_clientId = _clientId;
            m_clientName = _clientName;
            m_spawnPos = _pos;

            ErrorMessage.AddMessage($"{m_clientName} is joining...");
            
            AttemptSpawn();
        }

        public void UpdateTransform(Vector3 _pos, Quaternion _rot)
        {
            if (m_spawnedIn)
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
                m_viewModel = GameObject.Instantiate<GameObject>(targetVisual, m_spawnPos, Quaternion.identity);
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

                m_spawnedIn = true;
            }
        }

        public void RemovePlayer()
        {
            GameObject.Destroy(m_viewModel);

            ErrorMessage.AddMessage($"{m_clientName} has left...");
        }
    }
}
