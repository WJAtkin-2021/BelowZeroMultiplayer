using HarmonyLib;

namespace BelowZeroClient
{
    [HarmonyPatch(typeof(KnownTech), "Add")]
    class OnKnownTechAddPatch
    {
        [HarmonyPostfix]
        static void PostFix(TechType techType, bool unlockEncyclopedia, bool verbose)
        {
            if (techType != TechType.None)
            {
                NetSend.TechKnowledgeAdded(techType, unlockEncyclopedia, verbose);
            }
        }
    }
}
