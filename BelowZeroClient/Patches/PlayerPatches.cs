using BelowZeroClient;
using HarmonyLib;
using System.Net.Sockets;
using System.Threading;

namespace ClientSubnautica.MultiplayerManager
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

                NetSend.SpawnMe();
            }
        }
    }
}
