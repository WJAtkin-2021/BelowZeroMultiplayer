using System;
using HarmonyLib;

namespace ClientSubnautica.ClientManager
{
    class OnPickDropZone
    {
        [HarmonyPatch(typeof(SupplyDropData), "PickDropZone")]
        public class Patches
        {
            [HarmonyPrefix]
            static void Prefix(SupplyDropData __instance)
            {
                SupplyDropZone[] dropZones = __instance.dropZones;
                Array.Resize<SupplyDropZone>(ref dropZones, 1);
                __instance.dropZones = dropZones;
            }
        }
    }
}
