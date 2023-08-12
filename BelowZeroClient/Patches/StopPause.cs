using HarmonyLib;
using UWE;

namespace BelowZeroClient.Patches
{
    class DisablePause
    {
        [HarmonyPatch(typeof(IngameMenu), "OnSelect")]
        public class Patches
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                FreezeTime.End(FreezeTime.Id.IngameMenu);
            }
        }
    }
}
