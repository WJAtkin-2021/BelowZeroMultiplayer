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
            //NetSend.AddedPDAEncyclopedia(key);
            //PDAUnlockQueue.m_instance.ResetTimer();
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

    [HarmonyPatch(typeof(PDAScanner), "Unlock")]
    class OnUnlock
    {
        [HarmonyPostfix]
        static void PostFix(PDAScanner.EntryData entryData, bool unlockBlueprint, bool unlockEncyclopedia, bool verbose = true)
        {
            ErrorMessage.AddMessage($"Scanned TechType: {entryData.key}");
            PDAUnlockQueue.m_instance.ResetTimer();

            if (!PDAScanner.ContainsCompleteEntry(entryData.key) && unlockEncyclopedia)
            {
                ErrorMessage.AddMessage($"Scanned TechType: {entryData.key} will be replicated");
                NetSend.AddedPDAEncyclopedia(entryData);
            }
        }
    }
}
