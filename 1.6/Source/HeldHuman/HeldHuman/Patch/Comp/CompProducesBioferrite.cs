using HarmonyLib;
using HeldHuman.Hook;
using HeldHuman.Tool;
using RimWorld;
using System;
using System.Text;
using Verse;

namespace HeldHuman.Patch.CompProducesBioferrite_
{
    [HarmonyPatch(typeof(CompProducesBioferrite), "BioferritePerDay")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    public class BioferritePerDay_Patch
    {
        static bool Prefix(ref float __result, Pawn pawn)
        {
            if (HumanTools.IsMutantHuman(pawn) && pawn.mutant.Def.producesBioferrite)
            {
                float value = 1.0f;
                foreach (var hooker in BioferriteDensityHookers.GetHookers())
                    hooker.Modify(pawn, ref value);
                __result = pawn.BodySize * value;
            }
            else
            {
                CompProducesBioferrite compProducesBioferrite = pawn.TryGetComp<CompProducesBioferrite>();
                if (compProducesBioferrite == null && (!pawn.IsMutant || !pawn.mutant.Def.producesBioferrite) || pawn.health.hediffSet.HasHediff(HediffDefOf.BioferriteExtracted))
                {
                    __result = 0f;
                }
                else
                {
                    float value = pawn.BodySize * (compProducesBioferrite?.Props.bioferriteDensity ?? 1f);
                    if (HumanTools.IsHoldableHuman(pawn))
                        foreach (var hooker in BioferriteDensityHookers.GetHookers())
                            hooker.Modify(pawn, ref value);
                    __result = value;
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(CompProducesBioferrite), "BioferriteStatDrawEntry")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    public class BioferriteStatDrawEntry_Patch
    {
        static bool Prefix(ref StatDrawEntry __result, Pawn pawn)
        {
            CompProducesBioferrite compProducesBioferrite = pawn.TryGetComp<CompProducesBioferrite>();
            StringBuilder stringBuilder = new StringBuilder("StatsReport_BioferriteGeneration_Desc".Translate());
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": 1");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("StatsReport_BodySize".Translate(pawn.BodySize.ToString("F2")) + ": x" + pawn.BodySize.ToStringPercent());
            if (compProducesBioferrite != null)
                stringBuilder.AppendLine("StatsReport_BioferriteDensityMultiplier".Translate() + ": x" + compProducesBioferrite.Props.bioferriteDensity.ToStringPercent());
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.BioferriteExtracted))
                stringBuilder.AppendLine("StatsReport_BioferriteExtractedMultiplier".Translate() + ": x" + 0f.ToStringPercent());

            if (HumanTools.IsHuman(pawn))
                foreach (var hooker in BioferriteDensityHookers.GetHookers())
                    hooker.AddStatDraw(pawn, ref stringBuilder);

            stringBuilder.AppendLine();
            stringBuilder.Append("StatsReport_FinalValue".Translate() + ": " + CompProducesBioferrite.BioferritePerDay(pawn).ToString("F1"));
          
            __result = new StatDrawEntry(StatCategoryDefOf.Containment, "StatsReport_BioferriteGeneration".Translate(), CompProducesBioferrite.BioferritePerDay(pawn).ToString(), stringBuilder.ToString(), 100);
            return false;
        }
    }
}