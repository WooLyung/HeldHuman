using HarmonyLib;
using HeldHuman.Tool;
using Verse;

namespace HeldHuman.Patch.Thing_
{
    [HarmonyPatch(typeof(Thing), "get_Map")]
    public class Map_Patch
    {
        static bool Prefix(ref Thing __instance, ref Map __result)
        {
            if (HumanTool.IsHoldableHuman(__instance) && __instance.IsOnHoldingPlatform)
            {
                __result = __instance.MapHeld;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Thing), "get_Position")]
    public class Position_Patch
    {
        static bool Prefix(ref Thing __instance, ref IntVec3 __result)
        {
            if (HumanTool.IsHoldableHuman(__instance) && __instance.IsOnHoldingPlatform)
            {
                __result = __instance.PositionHeld;
                return false;
            }
            return true;
        }
    }
}