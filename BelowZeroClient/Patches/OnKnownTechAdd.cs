using HarmonyLib;

namespace BelowZeroClient
{
    [HarmonyPatch(typeof(KnownTech), "Add")]
    class OnKnownTechAddPatch
    {
        [HarmonyPrefix]
        static void PreFix(TechType techType, bool unlockEncyclopedia, bool verbose)
        {
            if (techType != TechType.None)
            {
                TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType);
                if (techUnlockState != TechUnlockState.Available)
                {
                    NetSend.TechKnowledgeAdded(techType, unlockEncyclopedia, verbose);
                }
            }
        }
    }
}
