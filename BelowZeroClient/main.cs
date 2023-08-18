using HarmonyLib;
using QModManager.API.ModLoading;

namespace BelowZeroClient
{
    [QModCore]
    public static class BelowZeroClientInitClass
    {
        [QModPatch]
        public static void BelowZeroClientInitMethod()
        {
            Harmony harmony = new Harmony("BelowZeroMultiplayer");
            harmony.PatchAll();
        }
    }
}
