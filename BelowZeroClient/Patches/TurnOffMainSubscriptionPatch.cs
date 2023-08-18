using HarmonyLib;

namespace ClientSubnautica
{
    public class TurnOffMainSubscriptionPatch
    {
        [HarmonyPatch(typeof(MainMenuEmailHandler), "Subscribe")]
        public class Patches
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                return false;
            }
        }
    }
}
