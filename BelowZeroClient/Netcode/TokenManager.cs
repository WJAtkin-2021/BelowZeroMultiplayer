using BelowZeroMultiplayerCommon;
using HarmonyLib;
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

        public void AddToken(NetToken _token)
        {
            if (!Tokens.ContainsKey(_token.guid))
            {
                Tokens.Add(_token.guid, _token);
            }
            else
            {
                ErrorMessage.AddMessage($"[TokenManager] Received request to generate node for token: {_token.guid} but this already exists");
                FileLog.Log($"[TokenManager] Received request to generate node for token: {_token.guid} but this already exists");
            }
        }

        public void HandleTokenCreation(TokenDescriptor _tokenDescriptor)
        {
            if (!Tokens.ContainsKey(_tokenDescriptor.guid))
            {
                ErrorMessage.AddMessage($"[TokenManager] Received request to generate node for token: {_tokenDescriptor.guid}");

                FileLog.Log($"-----TOKEN------");
                FileLog.Log($"guid: {_tokenDescriptor.guid}");
                FileLog.Log($"clientWithToken: {_tokenDescriptor.clientWithToken}");
                FileLog.Log($"tokenExchangePolicy: {_tokenDescriptor.tokenExchangePolicy}");
                FileLog.Log($"associatedTechType: {_tokenDescriptor.associatedTechType}");
                FileLog.Log($"networkedEntityType: {_tokenDescriptor.networkedEntityType}");
                FileLog.Log($"tickRate:  {_tokenDescriptor.tickRate}");
                FileLog.Log($"position:  {_tokenDescriptor.position}");
                FileLog.Log($"rotation:  {_tokenDescriptor.rotation}");
                FileLog.Log($"scale:  {_tokenDescriptor.scale}");
                FileLog.Log($"------EOF-------");

                CoroutineHost.StartCoroutine(FactoryCreatePickupable(_tokenDescriptor));
            }
            else
            {
                ErrorMessage.AddMessage($"[TokenManager] Received request to generate node for token: {_tokenDescriptor.guid} but this already exists");
                FileLog.Log($"[TokenManager] Received request to generate node for token: {_tokenDescriptor.guid} but this already exists");
            }
        }

        public void HandleTokenUpdate(string _tokenGuid, Vector3 _pos, Quaternion _rot, Vector3 _scale)
        {
            if (Tokens.ContainsKey(_tokenGuid))
            {
                NetToken token = Tokens[_tokenGuid];
                token.HandleTokenUpdate(_pos, _rot, _scale);
            }
            else
            {
                ErrorMessage.AddMessage($"[TokenManager] Received request to update node: {_tokenGuid} but this doesn't exist");
                FileLog.Log($"[TokenManager] Received request to update node: {_tokenGuid} but this doesn't exist");
            }
        }

        public void HandleTokenDataUpdated(TokenDescriptor _tokenDescriptor)
        {

        }

        public void HandleTokenAcquired(string _tokenGuid, bool _wasSuccessful)
        {

        }

        public void HandleTokenDestroyed(string _tokenGuid)
        {
            if (Tokens.ContainsKey(_tokenGuid))
            {
                NetToken token = Tokens[_tokenGuid];
                token.DestroyToken();
            }
            else
            {
                ErrorMessage.AddMessage($"[TokenManager] Received request to destroy token: {_tokenGuid} but this doesn't exist");
                FileLog.Log($"[TokenManager] Received request to destroy token: {_tokenGuid} but this doesn't exist");
            }
        }

        public void HandleServerRequestedTokenDestruction(string _tokenGuid)
        {
            if (Tokens.ContainsKey(_tokenGuid))
            {
                ErrorMessage.AddMessage($"[TokenManager] Server requested destruction of token: {_tokenGuid}");
                NetToken token = Tokens[_tokenGuid];
                GameObject tokenGo = token.gameObject;
                token.DestroyToken();
                Destroy(tokenGo);
            }
            else
            {
                ErrorMessage.AddMessage($"[TokenManager] Received request to destroy node: {_tokenGuid} but this doesn't exist");
                FileLog.Log($"[TokenManager] Received request to destroy node: {_tokenGuid} but this doesn't exist");
            }
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
