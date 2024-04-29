using HarmonyLib;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.Building_HoldingPlatform_
{
    [HarmonyPatch(typeof(Building_HoldingPlatform), "Tick")]
    public class Tick_Patch
    {
        private static bool PawnConsciousEnoughForExtraction(Pawn pawn)
        {
            return pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) > 0.45f;
        }

        private static bool CanSafelyBeQueuedForHemogenExtraction(Pawn pawn)
        {
            if (ModsConfig.BiotechActive && pawn.BillStack != null && !pawn.BillStack.Bills.Any((Bill x) => x.recipe == RecipeDefOf.ExtractHemogenPack) && PawnConsciousEnoughForExtraction(pawn) && RecipeDefOf.ExtractHemogenPack.Worker.AvailableOnNow(pawn))
                return !pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss);
            return false;
        }

        static void Postfix(ref Building_HoldingPlatform __instance)
        {
            Pawn pawn = __instance.HeldPawn;
            if (!ModsConfig.BiotechActive || pawn == null || !Tool.HumanTool.IsHoldableHuman(pawn))
                return;
            if (pawn.IsHashIntervalTick(15000) && CanSafelyBeQueuedForHemogenExtraction(pawn) && pawn.guest.guestStatusInt == GuestStatus.Prisoner && pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.HemogenFarm))
                HealthCardUtility.CreateSurgeryBill(pawn, RecipeDefOf.ExtractHemogenPack, (BodyPartRecord)null, sendMessages: false);
        }
    }
}
