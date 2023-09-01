using System;
using System.Collections;
using UnityEngine;
using UWE;

namespace BelowZeroClient
{
    public class NetToken : MonoBehaviour
    {
        public string guid = string.Empty;
        public int clientWithToken = -1;

        private bool updateCoroutineIsRunning = false;
        private Coroutine coroutine;

        //public void OnEnable()
        //{
        //    ErrorMessage.AddMessage("OnEnable");
        //    if (guid != string.Empty)
        //    {
        //        ErrorMessage.AddMessage($"Has token: {HasToken()} is update runner running: {updateCoroutineIsRunning}");
        //        if (HasToken() && !updateCoroutineIsRunning)
        //        {
        //            ErrorMessage.AddMessage("Passed criteria");
        //            updateCoroutineIsRunning = true;
        //            coroutine = CoroutineHost.StartCoroutine(SendUpdate());
        //        }
        //    }
        //}
        //
        //public void OnDisable()
        //{
        //    ErrorMessage.AddMessage("OnDisable Called");
        //
        //    if (updateCoroutineIsRunning && coroutine != null)
        //    {
        //        CoroutineHost.StopCoroutine(coroutine);
        //        updateCoroutineIsRunning = false;
        //    }
        //}

        public void GenerateNewToken()
        {
            guid = Guid.NewGuid().ToString();
            clientWithToken = NetworkClient.m_instance.m_clientId;
            coroutine = CoroutineHost.StartCoroutine(SendUpdate());
            updateCoroutineIsRunning = true;

            // TODO: Tell the server about this
            NetSend.PlayerCreateToken(guid, gameObject.transform.position);
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
            NetSend.PlayerUpdateToken(guid, gameObject.transform.position);
        }

        // Called when server removes our token and passes it to another
        // client
        public void ReceiveTokenUpdate(int _clientWithToken, Vector3 _newPos)
        {
            clientWithToken = _clientWithToken;
            gameObject.transform.position = _newPos;
        }

        public void DestroyToken()
        {
            NetSend.PlayerDestroyToken(guid);
            if (updateCoroutineIsRunning && coroutine != null)
            {
                CoroutineHost.StopCoroutine(coroutine);
                updateCoroutineIsRunning = false;
            }
            Destroy(this);
        }

        private IEnumerator SendUpdate()
        {
            updateCoroutineIsRunning = true;

            yield return new WaitForSeconds(1.0f);

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
                ErrorMessage.AddMessage($"Error: {ex}");
            }

            yield return null;
        }
    }
}
