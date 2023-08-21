using HarmonyLib;
using UWE;

namespace BelowZeroClient
{
    class IngameMenuPatches
    {
        [HarmonyPatch(typeof(IngameMenu), "OnSelect")]
        public class OnSelectPatch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                FreezeTime.End(FreezeTime.Id.IngameMenu);
            }
        }

        [HarmonyPatch(typeof(IngameMenu), "QuitGame")]
        public class QuitGamePatch
        {
            [HarmonyPrefix]
            static void PreFix(bool quitToDesktop)
            {
                NetworkClient.m_instance.Disconnect();
            }
        }
    }
}
