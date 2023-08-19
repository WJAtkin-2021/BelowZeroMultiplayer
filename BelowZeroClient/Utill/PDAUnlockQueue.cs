﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BelowZeroClient
{
    // Nice little utillity class that stops the PDA from getting spammed by unlocks
    // As there is a bug in the game where you can only unlock 1 entry per frame which
    // whould cuase the second unlock to be ignored. This class is a workaround for
    // this issue
    public class PDAUnlockQueue : MonoBehaviour
    {
        private const float UNLOCK_DELAY = 3.0f;

        public static PDAUnlockQueue m_instance = null;

        private Queue<string> unlockQueue = new Queue<string>();
        private List<string> unlockedKeys = new List<string>();
        private bool isCoroutineRunning = false;

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

        public void UnlockDelayed(string _key)
        {
            // We perform a quick check here before adding it to the list
            if (!unlockedKeys.Contains(_key))
            {
                unlockQueue.Enqueue(_key);

                if (!isCoroutineRunning)
                    StartCoroutine(UnlockRunner());
            }
        }

        public void ResetTimer()
        {
            if (isCoroutineRunning)
            {
                StopCoroutine(UnlockRunner());
                StartCoroutine(UnlockRunner());
            }
        }

        private IEnumerator UnlockRunner()
        {
            isCoroutineRunning = true;
            yield return new WaitForSeconds(UNLOCK_DELAY);

            string nextEntry = unlockQueue.Dequeue();
            unlockedKeys.Add(nextEntry);
            PDAEncyclopedia.Add(nextEntry, true, true);

            isCoroutineRunning = false;

            // Restart the unlock runner if there is still
            // pda entries waiting to be unlocked
            if (unlockQueue.Count > 0)
                StartCoroutine(UnlockRunner());

            yield return null;
        }
    }
}