using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BelowZeroClient.Patches
{
    class AddMenuItems
    {
        [HarmonyPatch(typeof(MainMenuRightSide), "OpenGroup")]
        public class Patches
        {
            [HarmonyPostfix]
            static void PostFix(string target, MainMenuRightSide __instance)
            {
                FileLog.Log("Harmony Patched!");
            }
        }
    }
}
