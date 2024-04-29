using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.CompHoldingPlatformTarget_
{
    [HarmonyPatch(typeof(CompHoldingPlatformTarget), "get_StudiedAtHoldingPlatform")]
    public class StudiedAtHoldingPlatform_Patch
    {
        static bool Prefix(ref CompHoldingPlatformTarget __instance, ref bool __result)
        {
            if (!(__instance.parent is Pawn pawn))
                return true;

            if (HumanTool.IsHoldableHuman(pawn))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}