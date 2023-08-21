using HarmonyLib;

namespace BelowZeroClient
{
    public class TurnOffMainSubscriptionPatch
    {
        // NOTE: This class serves as a good example on how to disable the existing
        // function, here we have to disable the email subscription button as we clone
        // this to use as our join button
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
