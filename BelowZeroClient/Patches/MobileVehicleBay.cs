using HarmonyLib;

namespace BelowZeroClient
{
    public class MobileVehicleBayPatchs
    {
        [HarmonyPatch(typeof(Constructor), "Deploy")]
        class DeployPatch
        {
            [HarmonyPostfix]
            static void Postfix(bool value)
            {
                ErrorMessage.AddMessage($"Deploy called with value: {value}");
            }
        }
    }
}
