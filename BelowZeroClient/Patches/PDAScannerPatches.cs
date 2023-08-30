using HarmonyLib;

namespace BelowZeroClient
{
    // Kinda gross but serves as a good example on how to patch overloaded methods
    // NOTE: Currently deperecated and sucessed by OnUnlock but leaving the patch here
    // as an example
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
        [HarmonyPrefix]
        static void PreFix(PDAScanner.EntryData entryData, bool unlockBlueprint, bool unlockEncyclopedia, bool verbose = true)
        {
            PDAUnlockQueue.m_instance.ResetTimer();

            PDAEncyclopedia.EntryData encyclopediaEntry = null;
            if (PDAEncyclopedia.GetEntryData(entryData.encyclopedia, out encyclopediaEntry))
            {
                if (!encyclopediaEntry.unlocked)
                {
                    NetSend.AddedPDAEncyclopedia(entryData);
                }
            }
        }
    }
}
