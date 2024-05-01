using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
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

    [HarmonyPatch(typeof(Thing), "get_Rotation")]
    public class Rotation_Patch
    {
        static bool Prefix(ref Thing __instance, ref Rot4 __result)
        {
            if (HumanTool.IsHoldableHuman(__instance) && __instance.IsOnHoldingPlatform)
            {
                __result = ((Building_HoldingPlatform)__instance.ParentHolder).Rotation;
                return false;
            }
            return true;
        }
    }
}