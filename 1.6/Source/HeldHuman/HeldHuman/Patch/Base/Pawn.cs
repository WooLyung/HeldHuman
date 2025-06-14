using HarmonyLib;
using HeldHuman.Tool;
using Verse;

namespace HeldHuman.Patch.Pawn_
{
    [HarmonyPatch(typeof(Pawn), "CurrentlyUsableForBills")]
    public class CurrentlyUsableForBills_Patch
    {
        static bool Prefix(ref Pawn __instance, ref bool __result)
        {
            if (HumanTool.IsHoldableHuman(__instance) && __instance.IsOnHoldingPlatform)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}