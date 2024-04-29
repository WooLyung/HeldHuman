using HarmonyLib;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.Pawn_
{
    [HarmonyPatch(typeof(Pawn), "CurrentlyUsableForBills")]
    public class CurrentlyUsableForBills_Patch
    {
        static bool Prefix(ref Pawn __instance, ref bool __result)
        {
            if (__instance.ParentHolder != null && __instance.ParentHolder is Building_HoldingPlatform)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
