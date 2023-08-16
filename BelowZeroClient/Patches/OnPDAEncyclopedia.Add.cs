using HarmonyLib;

namespace BelowZeroClient
{
    [HarmonyPatch(typeof(PDAEncyclopedia), "Add")]
    class OnPDAEncyclopedia
    {
        [HarmonyPostfix]
        static void PostFix(string key, bool verbose, bool postNotification)
        {
            // TODO: ...
        }
    }
}
