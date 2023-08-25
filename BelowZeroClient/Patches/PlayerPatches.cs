using HarmonyLib;

namespace BelowZeroClient
{
    class PlayerPatches
    {
        [HarmonyPatch(typeof(Player), "Awake")]
        internal static class PlayerPatchesAwake
        {
            [HarmonyPostfix]
            public static void Postfix(Player __instance)
            {
                __instance.gameObject.AddComponent<ReplicatePlayer>();
                __instance.gameObject.AddComponent<ReplicateInventory>();
                __instance.gameObject.AddComponent<PDAUnlockQueue>();

                NetSend.SpawnMe();
            }
        }
    }
}
