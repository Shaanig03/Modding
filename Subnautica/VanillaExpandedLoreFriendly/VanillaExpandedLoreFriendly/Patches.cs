using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
namespace VanillaExpandedLoreFriendly
{

    // adds vanilla expanded player script
    [HarmonyPatch(typeof(Player))]
    internal class PlayerPatches
    {
        [HarmonyPatch(nameof(Player.Awake))]
        [HarmonyPostfix]
        public static void Awake_Postfix(Player __instance)
        {
            VEPlayer ve_player = __instance.gameObject.AddComponent<VEPlayer>();
        }
    }
}
