using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using Unity.Jobs;
using HeldHuman.Tool;

namespace HeldHuman.Def.JobDriver_
{
    public class JobDriver_BloodfeedHeldHuman : JobDriver
    {
        public const float BloodLoss = 0.4499f;
        public const int WaitTicks = 120;

        private Building_HoldingPlatform Platform => (Building_HoldingPlatform) job.targetA.Thing;
        private Pawn Pawn => PlatformTool.GetHeldPawn(Platform);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !HumanTool.IsHoldableHuman(Pawn) || !Pawn.IsPrisonerOfColony || !Pawn.guest.PrisonerIsSecure || Pawn.InAggroMentalState || Pawn.guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.Bloodfeed));
            yield return Toils_Interpersonal.GotoPrisoner(pawn, Pawn, PrisonerInteractionModeDefOf.Bloodfeed);
            yield return Toils_General.WaitWith(TargetIndex.A, 120, useProgressBar: true).PlaySustainerOrSound(SoundDefOf.Bloodfeed_Cast);
            yield return Toils_General.Do(delegate
            {
                SanguophageUtility.DoBite(pawn, Pawn, 0.2f, 0.1f, 0.4499f, 1f, new IntRange(1, 1), ThoughtDefOf.FedOn, ThoughtDefOf.FedOn_Social);
            });
        }
    }
}
