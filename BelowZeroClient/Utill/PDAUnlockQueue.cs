using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BelowZeroClient
{
    // Nice little utility class that stops the PDA from getting spammed by unlocks
    // As there is a bug in the game where you can only unlock 1 entry per frame which
    // would cause the second unlock to be ignored. This class is a workaround for
    // this issue
    public class PDAUnlockQueue : MonoBehaviour
    {
        private const float UNLOCK_DELAY = 3.0f;

        public static PDAUnlockQueue m_instance = null;

        private Queue<PDAKeyTechTypePair> m_unlockQueue = new Queue<PDAKeyTechTypePair>();
        private List<string> m_unlockedKeys = new List<string>();
        private bool m_isCoroutineRunning = false;

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

        public void UnlockDelayed(string _key, TechType _techType)
        {
            // We perform a quick check here before adding it to the list
            if (!m_unlockedKeys.Contains(_key))
            {
                PDAKeyTechTypePair entry = new PDAKeyTechTypePair(_key, _techType);

                m_unlockQueue.Enqueue(entry);

                if (!m_isCoroutineRunning)
                    StartCoroutine(UnlockRunner());
            }
        }

        public void ResetTimer()
        {
            if (m_isCoroutineRunning)
            {
                StopCoroutine(UnlockRunner());
                StartCoroutine(UnlockRunner());
            }
        }

        private IEnumerator UnlockRunner()
        {
            m_isCoroutineRunning = true;
            yield return new WaitForSeconds(UNLOCK_DELAY);

            PDAKeyTechTypePair nextEntry = m_unlockQueue.Dequeue();
            m_unlockedKeys.Add(nextEntry.key);
            PDAEncyclopedia.Add(nextEntry.key, true, true);

            // Ensure it is in the completed list on the PDA scanner
            EnsureEntryIsComplete(nextEntry.techType);

            m_isCoroutineRunning = false;

            // Restart the unlock runner if there is still
            // pda entries waiting to be unlocked
            if (m_unlockQueue.Count > 0)
                StartCoroutine(UnlockRunner());

            yield return null;
        }

        public void EnsureEntryIsComplete(TechType _techType)
        {
            if (_techType != TechType.None)
            {
                PDAScanner.Data data = PDAScanner.Serialize();
                data.complete.Add(_techType);
                PDAScanner.Deserialize(data);
            }
        }
    }

    public class PDAKeyTechTypePair
    {
        public string key;
        public TechType techType;

        public PDAKeyTechTypePair() { }

        public PDAKeyTechTypePair(string _key, TechType _techType)
        {
            key = _key;
            techType = _techType;
        }
    }
}
