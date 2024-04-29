using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.CompHoldingPlatformTarget_
{
    [HarmonyPatch(typeof(CompHoldingPlatformTarget), "get_CanBeCaptured")]
    public class CanBeCaptured_Patch
    {
        static bool Prefix(ref CompHoldingPlatformTarget __instance, ref bool __result)
        {
            if (!(__instance.parent is Pawn pawn))
                return true;
            if (!HumanTool.IsHoldableHuman(pawn))
                return true;

            if (pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
