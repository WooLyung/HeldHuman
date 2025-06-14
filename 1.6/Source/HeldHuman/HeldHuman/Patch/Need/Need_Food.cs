using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using Verse;
using ModSettings = HeldHuman.Setting.ModSettings;

namespace HeldHuman.Patch.Need_Food_
{
    [HarmonyPatch(typeof(Need_Food), "FoodFallPerTickAssumingCategory")]
    [HarmonyPatch(new Type[] { typeof(HungerCategory), typeof(bool) })]
    public class FoodFallPerTickAssumingCategory_Patch
    {
        static bool Prefix(ref Need_Food __instance, ref float __result, HungerCategory hunger, bool ignoreMalnutrition)
        {
            if (!ModSettings.Instance.enableFood)
                return true;

            Pawn pawn = (Pawn)AccessTools.Field(typeof(Need), "pawn").GetValue(__instance);
            if (!HumanTool.IsHoldableHuman(pawn) || !pawn.IsOnHoldingPlatform)
                return true;

            Building_Bed building_Bed = pawn.CurrentBed();
            float num = Need_Food.BaseHungerRate(pawn.ageTracker.CurLifeStage, pawn.def) * hunger.HungerMultiplier() * pawn.health.hediffSet.GetHungerRateFactor(ignoreMalnutrition ? HediffDefOf.Malnutrition : null) * (pawn.story?.traits?.HungerRateFactor ?? 1f) * (building_Bed?.GetStatValue(StatDefOf.BedHungerRateFactor) ?? 1f);
            Hediff firstHediffOfDef;
            HediffComp_Lactating hediffComp_Lactating;

            if (ModsConfig.BiotechActive && (firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating)) != null && (hediffComp_Lactating = firstHediffOfDef.TryGetComp<HediffComp_Lactating>()) != null)
                num += hediffComp_Lactating.AddedNutritionPerDay() / 60000f;

            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                int num2 = 0;
                foreach (Gene item in pawn.genes.GenesListForReading)
                {
                    if (!item.Overridden)
                    {
                        num2 += item.def.biostatMet;
                    }
                }
                num *= GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate(num2);
            }

            __result = num;
            return false;
        }
    }

    [HarmonyPatch(typeof(Need_Food), "get_IsFrozen")]
    public class IsFrozen_Patch
    {
        static bool Prefix(ref Need_Food __instance, ref bool __result)
        {
            if (!ModSettings.Instance.enableFood)
                return true;

            Pawn pawn = (Pawn)AccessTools.Field(typeof(Need), "pawn").GetValue(__instance);

            if (!HumanTool.IsHoldableHuman(pawn) || !pawn.IsOnHoldingPlatform)
                return true;

            __result = pawn.Suspended || (__instance.def.freezeWhileSleeping && !pawn.Awake()) || (__instance.def.freezeInMentalState && pawn.InMentalState) || 
                !(bool)AccessTools.Method(typeof(Need), "get_IsPawnInteractableOrVisible").Invoke(__instance, null) || pawn.Deathresting;
            return false;
        }
    }
}
