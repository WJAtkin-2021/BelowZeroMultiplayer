﻿using HarmonyLib;

namespace BelowZeroClient
{
    // Kinda gross but serves as a good example on how to patch overloaded methods
    [HarmonyPatch(typeof(PDAEncyclopedia), nameof(PDAEncyclopedia.Add), new[] { typeof(string), typeof(bool), typeof(bool) })]
    class OnPDAEncyclopedia
    {
        [HarmonyPrefix]
        static void PreFix(string key, bool verbose, bool postNotification)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (PDAEncyclopedia.HasEntryData(key))
                {
                    PDAEncyclopedia.EntryData entryData;
                    PDAEncyclopedia.GetEntryData(key, out entryData);

                    if (!entryData.unlocked)
                    {
                        NetSend.AddedPDAEncyclopedia(key, verbose, postNotification);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PDAScanner), "NotifyProgress")]
    class OnNotifyProgress
    {
        [HarmonyPostfix]
        static void PostFix(PDAScanner.Entry entry)
        {
            NetSend.FramentProgressUpdated(entry);
        }
    }
}
