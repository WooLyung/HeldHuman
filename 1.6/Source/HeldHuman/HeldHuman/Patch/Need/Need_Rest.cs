using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using Verse;

namespace HeldHuman.Patch.Need_Rest_
{
    [HarmonyPatch(typeof(Need_Rest), "NeedInterval")]
    public class NeedInterval_Patch
    {
        static bool Prefix(ref Need_Rest __instance)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Need), "pawn").GetValue(__instance);
            if (!HumanTool.IsHoldableHuman(pawn) || !pawn.IsOnHoldingPlatform)
                return true;

            float num = 0.1f;
            num *= pawn.GetStatValue(StatDefOf.RestRateMultiplier);
            if (num > 0f)
                __instance.CurLevel += 0.005714286f * num;

            return false;
        }
    }

    [HarmonyPatch(typeof(Need_Rest), "get_GUIChangeArrow")]
    public class get_GUIChangeArrow_Patch
    {
        static bool Prefix(ref Need_Rest __instance, ref int __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Need), "pawn").GetValue(__instance);
            if (!HumanTool.IsHoldableHuman(pawn) || !pawn.IsOnHoldingPlatform)
                return true;

            __result = 1;
            return false;
        }
    }
}
