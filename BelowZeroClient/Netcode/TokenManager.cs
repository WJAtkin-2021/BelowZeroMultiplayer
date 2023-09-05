using BelowZeroMultiplayerCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UWE;

namespace BelowZeroClient
{
    // Manages the update / exchange of tokens client side
    public class TokenManager : MonoBehaviour
    {
        public static TokenManager m_instance;

        public Dictionary<string, NetToken> Tokens = new Dictionary<string, NetToken>();
        
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

        public void HandleTokenCreation(TokenDescriptor _tokenDescriptor)
        {
            if (!Tokens.ContainsKey(_tokenDescriptor.guid))
            {
                CoroutineHost.StartCoroutine(FactoryCreatePickupable(_tokenDescriptor));
            }
            else
            {
                ErrorMessage.AddMessage($"[TokenManager] Received request to generate node for token: {_tokenDescriptor.guid} but this already exists");
            }
        }

        public void HandleTokenUpdate(string _tokenGuid, Vector3 _pos, Quaternion _rot, Vector3 _scale)
        {

        }

        public void HandleTokenDataUpdated(TokenDescriptor _tokenDescriptor)
        {

        }

        public void HandleTokenAcquired(string _tokenGuid, bool _wasSuccessful)
        {

        }

        public void HandleTokenDestroyed(string _tokenGuid)
        {

        }

        private IEnumerator FactoryCreatePickupable(TokenDescriptor _tokenDescriptor)
        {
            CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(_tokenDescriptor.associatedTechType, true);
            yield return task;
            GameObject gameObjectPrefab = task.GetResult();

            Vector3 toDirection = Vector3.up;
            GameObject newGameObject = Instantiate(gameObjectPrefab, _tokenDescriptor.position, Quaternion.FromToRotation(Vector3.up, toDirection));
            newGameObject.SetActive(true);

            Pickupable pickupable = newGameObject.GetComponent<Pickupable>();
            pickupable.Drop(_tokenDescriptor.position, Vector3.zero, false);

            AttachTokenToGameObject(newGameObject, _tokenDescriptor);
        }

        private void AttachTokenToGameObject(GameObject go, TokenDescriptor _tokenDescriptor)
        {
            NetToken newToken = go.AddComponent<NetToken>();
            newToken.GenerateExistingToken(_tokenDescriptor);
            Tokens.Add(newToken.guid, newToken);
        }
    }
}
