using HarmonyLib;

namespace BelowZeroClient
{
    // Kinda gross but serves as a good example on how to patch overloaded methods
    [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Add), new[] { typeof(string), typeof(bool), typeof(bool) })]
    class OnPDAEncyclopedia
    {
        [HarmonyPrefix]
        static void PreFix(string key, bool verbose, bool postNotification)
        {
            NetSend.AddedPDAEncyclopedia(key);
            PDAUnlockQueue.m_instance.ResetTimer();
        }
    }

    [HarmonyPatch(typeof(PDAScanner), "NotifyProgress")]
    class OnNotifyProgress
    {
        [HarmonyPostfix]
        static void PostFix(PDAScanner.Entry entry)
        {
            NetSend.FragmentProgressUpdated(entry);
        }
    }
}
