using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using Verse;

namespace HeldHuman.Patch.CompProducesBioferrite_
{
    [HarmonyPatch(typeof(CompProducesBioferrite), "BioferritePerDay")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    public class BioferritePerDay_Patch
    {
        static void Postfix(ref float __result, Pawn pawn)
        {
            if (HumanTool.IsMutantHuman(pawn) && pawn.mutant.Def.producesBioferrite)
                __result = pawn.BodySize * 1.0f;
        }
    }
}