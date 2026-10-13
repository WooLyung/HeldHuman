using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace HeldHuman.Def
{
    public class JobDriver_ExecuteHeldHuman : JobDriver_ExecuteEntity
    {
        private Thing Platform => TargetThingA;
        private Pawn InnerPawn => (Platform as Building_HoldingPlatform)?.HeldPawn;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(delegate
            {
                Pawn innerPawn = InnerPawn;
                if (innerPawn == null || innerPawn.Destroyed)
                {
                    return true;
                }

                if (job.ignoreDesignations)
                {
                    return false;
                }

                CompHoldingPlatformTarget compHoldingPlatformTarget = innerPawn.TryGetComp<CompHoldingPlatformTarget>();
                return compHoldingPlatformTarget == null || !innerPawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Execution);
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil toil = Toils_General.Do(delegate
            {
                Messages.Message("MessageEntityExecuted".Translate(pawn.Named("EXECUTIONER"), InnerPawn.Named("VICTIM")), pawn, MessageTypeDefOf.NeutralEvent);
                ExecutionUtility.DoExecutionByCut(pawn, InnerPawn);
                pawn.MentalState?.Notify_SlaughteredTarget();
            });
            toil.activeSkill = () => SkillDefOf.Melee;
            yield return toil;
        }
    }
}