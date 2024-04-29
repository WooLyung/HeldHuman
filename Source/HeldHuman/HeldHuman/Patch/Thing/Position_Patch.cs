using HarmonyLib;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.Thing_
{
    [HarmonyPatch(typeof(Thing), "get_Position")]
    public class Position_Patch
    {
        static bool Prefix(ref Thing __instance, ref IntVec3 __result)
        {
            if (__instance != null && __instance is Pawn && __instance.IsOnHoldingPlatform)
            {
                __result = __instance.PositionHeld;
                return false;
            }

            return true;
        }
    }
}
