using System;
using System.Collections;
using UnityEngine;
using UWE;
using BelowZeroMultiplayerCommon;
using HarmonyLib;

namespace BelowZeroClient
{
    public class NetToken : MonoBehaviour
    {
        public string guid { get; private set; }
        public int clientWithToken { get; private set; }
        public float tickRate { get; private set; }
        public TokenExchangePolicy tokenExchangePolicy { get; private set; }
        public TechType associatedTechType { get; private set; }
        public NetworkedEntityType networkedEntityType { get; private set; }

        // Update runner
        private bool updateCoroutineIsRunning = false;
        private Coroutine coroutine = null;

        // Unity transform lerp
        private Vector3 position1;
        private Vector3 position2;
        private Quaternion rotation1;
        private Quaternion rotation2;
        private float lastUpdateTimer;
        private Rigidbody rigidbody;

        public NetToken()
        {
            guid = string.Empty;
            clientWithToken = -1;
            tickRate = 1.0f;
            tokenExchangePolicy = TokenExchangePolicy.None;
            associatedTechType = TechType.None;
            networkedEntityType = NetworkedEntityType.None;
            position1 = position2 = Vector3.zero;
            rotation1 = rotation2 = Quaternion.identity;
            lastUpdateTimer = 0.0f;

            rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.GetComponentInChildren<Rigidbody>();
            }
        }

        public void Update()
        {
            lastUpdateTimer += Time.deltaTime;

            if (!HasToken())
            {
                // Perform linear interpolation
                float t = Mathf.Clamp(lastUpdateTimer * tickRate, 0.0f, 1.0f);
                transform.position = Vector3.Lerp(position1, position2, t);
                transform.rotation = Quaternion.Slerp(rotation1, rotation2, t);
            }
        }

        public void GenerateNewToken(TokenExchangePolicy _tokenExchangePolicy, TechType _associatedTechType, NetworkedEntityType _networkedEntityType, float _tickRate)
        {
            guid = Guid.NewGuid().ToString();
            clientWithToken = NetworkClient.m_instance.m_clientId;
            tokenExchangePolicy = _tokenExchangePolicy;
            associatedTechType = _associatedTechType;
            networkedEntityType = _networkedEntityType;
            tickRate = _tickRate;

            NetSend.PlayerCreateToken(this);

            coroutine = CoroutineHost.StartCoroutine(SendUpdate());
            updateCoroutineIsRunning = true;
        }

        public void GenerateExistingToken(TokenDescriptor _tokenDescriptor)
        {
            guid = _tokenDescriptor.guid;
            clientWithToken = _tokenDescriptor.clientWithToken;
            tickRate = _tokenDescriptor.tickRate;
            tokenExchangePolicy = _tokenDescriptor.tokenExchangePolicy;
            associatedTechType = _tokenDescriptor.associatedTechType;
            networkedEntityType = _tokenDescriptor.networkedEntityType;
            position1 = position2 = _tokenDescriptor.position;
            rotation1 = rotation2 = _tokenDescriptor.rotation;
            lastUpdateTimer = 0.0f;
            transform.position = _tokenDescriptor.position;
            transform.rotation = _tokenDescriptor.rotation;
            transform.localScale = _tokenDescriptor.scale;

            if (rigidbody != null)
                rigidbody.isKinematic = true;
        }

        public bool HasToken()
        {
            if (clientWithToken == NetworkClient.m_instance.m_clientId)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AcquireToken()
        {
            // TODO:
        }

        public void SendTokenUpdate()
        {
            // Send position of acquired token so server can perform
            // handover if necessary
            //NetSend.PlayerUpdateToken(guid, gameObject.transform.position);
        }

        public void DestroyToken()
        {
            //NetSend.PlayerDestroyToken(guid);
            if (updateCoroutineIsRunning && coroutine != null)
            {
                CoroutineHost.StopCoroutine(coroutine);
                updateCoroutineIsRunning = false;
            }
            Destroy(this);
        }

        public void UpdateTokenExchangePolicy(TokenExchangePolicy _tokenExchangePolicy)
        {
            if (HasToken())
            {
                // TODO: Send packet with new policy
            }
            else
            {
                ErrorMessage.AddMessage($"[NetToken] Tired to change exchange policy of: {associatedTechType} with guid: {guid} but we do not own it");
                FileLog.Log($"[NetToken] Tired to change exchange policy of: {associatedTechType} with guid: {guid} but we do not own it");
            }
        }

        private IEnumerator SendUpdate()
        {
            updateCoroutineIsRunning = true;

            yield return new WaitForSeconds(1.0f / tickRate);

            try
            {
                if (HasToken())
                {
                    SendTokenUpdate();

                    coroutine = CoroutineHost.StartCoroutine(SendUpdate());
                }
                else
                {
                    updateCoroutineIsRunning = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.AddMessage($"[NetToken] Error: {ex}");
                FileLog.Log($"[NetToken] Error: {ex}");
            }

            yield return null;
        }

        public void HandleAcquiredToken(TokenExchangePolicy _tokenExchangePolicy)
        {
            clientWithToken = NetworkClient.m_instance.m_clientId;
            tokenExchangePolicy = _tokenExchangePolicy;

            if (rigidbody != null)
                rigidbody.isKinematic = true;

            if (!updateCoroutineIsRunning)
            {
                coroutine = CoroutineHost.StartCoroutine(SendUpdate());
                updateCoroutineIsRunning = true;
            }
        }

        public void HandleTokenUpdate(Vector3 _pos, Quaternion _rot, Vector3 _scale)
        {
            if (!HasToken())
            {
                position1 = position2;
                position2 = _pos;
                rotation1 = rotation2;
                rotation2 = _rot;
                transform.localScale = _scale;

                // DEBUG: Print out if we have any late state updates, need to test network performance with 10000+ tokens
#if DEBUG
                if (lastUpdateTimer >= tickRate * 100.0f)
                {
                    string errorMessage = $"[NetToken] Client: {NetworkClient.m_instance.ResolvePlayerName(clientWithToken)} is very late with state update for node: {guid}";
                    ErrorMessage.AddMessage(errorMessage);
                    FileLog.Log(errorMessage);
                }
#endif
                lastUpdateTimer = 0.0f;
            }
            else
            {
                ErrorMessage.AddMessage($"[NetToken] Received a token update for {associatedTechType} with guid: {guid} but we already own it");
                FileLog.Log($"[NetToken] Received a token update for {associatedTechType} with guid: {guid} but we already own it");
            }
        }

        public void HandleTokenDataUpdate(int _clientWithToken, float _tickRate, TokenExchangePolicy _tokenExchangePolicy)
        {
            if (!HasToken())
            {
                clientWithToken = _clientWithToken;
                tickRate = _tickRate;
                tokenExchangePolicy = _tokenExchangePolicy;
            }
            else
            {
                ErrorMessage.AddMessage($"[NetToken] Received a token data update for {associatedTechType} with guid: {guid} but we already own it");
                FileLog.Log($"[NetToken] Received a token data update for {associatedTechType} with guid: {guid} but we already own it");
            }
        }
    }

    public class TokenDescriptor
    {
        public string guid;
        public int clientWithToken;
        public float tickRate;
        public TokenExchangePolicy tokenExchangePolicy;
        public TechType associatedTechType;
        public NetworkedEntityType networkedEntityType;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}
