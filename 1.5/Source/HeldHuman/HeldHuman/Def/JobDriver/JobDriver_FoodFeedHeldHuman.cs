using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using System;
using HeldHuman.ToilHelper;

namespace HeldHuman.Def.JobDriver_
{
    public class JobDriver_FoodFeedHeldHuman : JobDriver_FoodFeedPatient
    {
        private const TargetIndex FoodSourceInd = TargetIndex.A;
        private const TargetIndex DelivereeInd = TargetIndex.B;
        private const TargetIndex FoodHolderInd = TargetIndex.C;

        private Thing Pawn => job.GetTarget(DelivereeInd).Thing;
        private Building_HoldingPlatform Platform => Pawn.ParentHolder as Building_HoldingPlatform;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            AddEndCondition(delegate
            {
                return Pawn.IsOnHoldingPlatform ? JobCondition.Ongoing : JobCondition.Incompletable;
            });
            AddEndCondition(delegate
            {
                if (!Pawn.IsOnHoldingPlatform)
                    return JobCondition.Incompletable;
                return (Platform.Suspended || Platform.IsBurning()) ? JobCondition.Incompletable : JobCondition.Ongoing;
            });
            Toil carryFoodFromInventory = Toils_Misc.TakeItemFromInventoryToCarrier(pawn, FoodSourceInd);
            Toil goToNutrientDispenser = Toils_Goto.GotoThing(FoodSourceInd, PathEndMode.InteractionCell).FailOnForbidden(FoodSourceInd);
            Toil goToFoodHolder = Toils_Goto.GotoThing(FoodHolderInd, PathEndMode.Touch).FailOn(() => FoodHolder != FoodHolderInventory?.pawn || FoodHolder.IsForbidden(pawn));
            Toil carryFoodToPatient = Toils.GotoThing(Platform, PathEndMode.Touch);
            yield return Toils_Jump.JumpIf(carryFoodFromInventory, () => pawn.inventory != null && pawn.inventory.Contains(TargetThingA));
            yield return Toils_Haul.CheckItemCarriedByOtherPawn(Food, FoodHolderInd, goToFoodHolder);
            yield return Toils_Jump.JumpIf(goToNutrientDispenser, () => base.TargetThingA is Building_NutrientPasteDispenser);
            yield return Toils_Goto.GotoThing(FoodSourceInd, PathEndMode.ClosestTouch).FailOnForbidden(FoodSourceInd);
            yield return Toils_Ingest.PickupIngestible(FoodSourceInd, Deliveree);
            yield return Toils_Jump.Jump(carryFoodToPatient);
            yield return goToFoodHolder;
            yield return Toils_General.Wait(25).WithProgressBarToilDelay(FoodHolderInd);
            yield return Toils_Haul.TakeFromOtherInventory(Food, pawn.inventory.innerContainer, FoodHolderInventory?.innerContainer, job.count, FoodSourceInd);
            yield return carryFoodFromInventory;
            yield return Toils_Jump.Jump(carryFoodToPatient);
            yield return goToNutrientDispenser;
            yield return Toils_Ingest.TakeMealFromDispenser(FoodSourceInd, pawn);
            yield return carryFoodToPatient;
            yield return Toils_Ingest.ChewIngestible(Deliveree, 1.5f, FoodSourceInd);
            Toil toil = Toils_Ingest.FinalizeIngest(Deliveree, FoodSourceInd);
            toil.finishActions = new List<Action>
            {
                delegate
                {
                    if (ModsConfig.AnomalyActive && Rand.Chance(0.3f) && MetalhorrorUtility.IsInfected(pawn))
                    {
                        MetalhorrorUtility.Infect(Deliveree, pawn, "FeedingImplant");
                    }
                }
            };
            yield return toil;
        }
    }
}
