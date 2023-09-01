using HarmonyLib;

namespace BelowZeroClient
{
    public class MobileVehicleBayPatches
    {
        [HarmonyPatch(typeof(Constructor), "Deploy")]
        class DeployPatch
        {
            [HarmonyPostfix]
            static void Postfix(Constructor __instance, bool value)
            {
                if (value)
                {
                    // If this is being deployed create a new NetToken for simulation control
                    NetToken netToken = __instance.gameObject.GetComponent<NetToken>();
                    if (netToken == null)
                    {
                        netToken = __instance.gameObject.AddComponent<NetToken>();
                        netToken.GenerateNewToken();
                    }
                    else
                    {
                        // Maybe not needed?
                        netToken.AcquireToken();
                    }
                }
                else
                {
                    // The player may just be holding it or might have picked it up
                    // we ensure the token is destroyed
                    NetToken netToken = __instance.gameObject.GetComponent<NetToken>();
                    if (netToken != null)
                    {
                        netToken.DestroyToken();
                    }
                }
            }
        }
    }
}
