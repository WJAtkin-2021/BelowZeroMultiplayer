using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace BelowZeroServer
{
    public class UnlockManager
    {
        private static ConcurrentBag<int> techUnlocks = new ConcurrentBag<int>();
        private static ConcurrentDictionary<string, int> pdaEncyclopedia = new ConcurrentDictionary<string, int>();
        private static ConcurrentDictionary<string, FragmentKnowledge> fragments = new ConcurrentDictionary<string, FragmentKnowledge>();

        public static List<int> GetAllUnlockedTech()
        {
            return techUnlocks.ToList();
        }

        public static ConcurrentDictionary<string, int> GetAllPdaEncyclopedia()
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

        public static bool AddTechUnlock(int _techType)
        {
            if (!techUnlocks.Contains(_techType))
            {
                techUnlocks.Add(_techType);
                return true;
            }

            return false;
        }

        public static bool AddPdaEntry(string _key, int _techType)
        {
            if (!pdaEncyclopedia.ContainsKey(_key))
            {
                pdaEncyclopedia[_key] = _techType;
                return true;
            }

            return false;
        }

        public static void UpdateFragment(int _techType, int _parts)
        {
            string key = _techType.ToString();
            if (!fragments.ContainsKey(key))
            {
                FragmentKnowledge newFrag = new FragmentKnowledge();
                newFrag.techType = _techType;
                newFrag.parts = _parts;
                fragments[key] = newFrag;
            }
            else
            {
                fragments[key].parts = _parts;
            }
        }

        public static void SaveUnlocks()
        {
            UnlockData unlockData = new UnlockData();
            unlockData.techUnlocks = techUnlocks.ToList();
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
                techUnlocks = new ConcurrentBag<int>(unlockData.techUnlocks);
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
        public ConcurrentDictionary<string, int> pdaEncyclopedia = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, FragmentKnowledge> fragments = new ConcurrentDictionary<string, FragmentKnowledge>();
    }
}
