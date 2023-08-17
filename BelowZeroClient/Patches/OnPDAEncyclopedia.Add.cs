using HarmonyLib;

namespace BelowZeroClient
{
    // Kinda gross but serves as a good example on how to patch overloaded methods
    [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Add), new[] {typeof(string), typeof(bool), typeof(bool)})]
    class OnPDAEncyclopedia
    {
        [HarmonyPostfix]
        static void PostFix(string key, bool verbose, bool postNotification)
        {
            if (!string.IsNullOrEmpty(key))
            {
                NetSend.AddedPDAEncyclopedia(key, verbose, postNotification);
            }
        }
    }
}
