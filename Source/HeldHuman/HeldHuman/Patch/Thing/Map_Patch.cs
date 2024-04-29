using HarmonyLib;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.Thing_
{
    [HarmonyPatch(typeof(Thing), "get_Map")]
    public class Map_Patch
    {
        static bool Prefix(ref Thing __instance, ref Map __result)
        {
            if (__instance != null && __instance is Pawn && __instance.IsOnHoldingPlatform)
            {
                __result = __instance.MapHeld;
                return false;
            }

            return true;
        }
    }
}
