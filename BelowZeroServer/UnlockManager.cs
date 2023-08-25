﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroServer
{
    public class UnlockManager
    {
        private static List<int> techUnlocks = new List<int>();
        private static List<string> pdaEncyclopedia = new List<string>();
        private static Dictionary<string, FragmentKnowledge> fragments = new Dictionary<string, FragmentKnowledge>();

        public static List<int> GetAllUnlockedTech()
        {
            return techUnlocks;
        }

        public static List<string> GetAllPdaEncyclopedia()
        {
            return pdaEncyclopedia;
        }

        public static List<FragmentKnowledge> GetAllFragments()
        {
            List<FragmentKnowledge> unknownFragments = new List<FragmentKnowledge>();

            foreach (var fragment in fragments.Values)
            {
                unknownFragments.Add(fragment);
            }

            return unknownFragments;
        }

        public static void AddTechUnlock(int _techType)
        {
            if (!techUnlocks.Contains(_techType))
            {
                techUnlocks.Add(_techType);
            }
        }

        public static void AddPdaEntry(string _key)
        {
            if (!pdaEncyclopedia.Contains(_key))
            {
                pdaEncyclopedia.Add(_key);
            }
        }

        public static void UpdateFragment(int _techType, int _parts)
        {
            string key = _techType.ToString();
            if (!fragments.ContainsKey(key))
            {
                FragmentKnowledge newFrag = new FragmentKnowledge();
                newFrag.techType = _techType;
                newFrag.parts = _parts;
                fragments.Add(key, newFrag);
            }
            else
            {
                fragments[key].parts = _parts;
            }
        }

        public static void SaveUnlocks()
        {
            UnlockData unlockData = new UnlockData();
            unlockData.techUnlocks = techUnlocks;
            unlockData.pdaEncyclopedia = pdaEncyclopedia;
            unlockData.fragments = fragments;
            DataStore.SaveUnlockData(unlockData);
        }

        public static void LoadUnlocks()
        {
            // Null check as the database might not have any data stored
            // if it is fresh
            UnlockData unlockData = DataStore.LoadUnlockData();
            if (unlockData != null)
            {
                techUnlocks = unlockData.techUnlocks;
                pdaEncyclopedia = unlockData.pdaEncyclopedia;
                fragments = unlockData.fragments;
            }
        }
    }

    public class FragmentKnowledge
    {
        public int techType;
        public int parts;
    }

    public class UnlockData
    {
        public List<int> techUnlocks = new List<int>();
        public List<string> pdaEncyclopedia = new List<string>();
        public Dictionary<string, FragmentKnowledge> fragments = new Dictionary<string, FragmentKnowledge>();
    }
}
