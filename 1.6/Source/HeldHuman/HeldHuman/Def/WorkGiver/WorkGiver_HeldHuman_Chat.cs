using RimWorld;
using Verse.AI;
using Verse;
using HeldHuman.Tool;

namespace HeldHuman.Def
{
    public class WorkGiver_HeldHuman_Chat : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return false;
            Pawn target = holder.HeldPawn;
            PrisonerInteractionModeDef exclusiveInteractionMode = target.guest.ExclusiveInteractionMode;
            
            if ((exclusiveInteractionMode == PrisonerInteractionModeDefOf.AttemptRecruit || exclusiveInteractionMode == PrisonerInteractionModeDefOf.ReduceResistance) && !target.guest.ScheduledForInteraction)
            {
                JobFailReason.Is("PrisonerInteractedTooRecently".Translate());
                return false;
            }

            return base.HasJobOnThing(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return null;
            Pawn target = holder.HeldPawn;

            PrisonerInteractionModeDef exclusiveInteractionMode = target.guest.ExclusiveInteractionMode;
            if ((exclusiveInteractionMode == PrisonerInteractionModeDefOf.AttemptRecruit || exclusiveInteractionMode == PrisonerInteractionModeDefOf.ReduceResistance) && target.guest.ScheduledForInteraction && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.CanReserve(t))
            {
                if (exclusiveInteractionMode == PrisonerInteractionModeDefOf.ReduceResistance && target.guest.Resistance <= 0f)
                    return null;
                return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("HeldHumanAttemptRecruit"), target);
            }

            return null;
        }

        public override string PostProcessedGerund(Job job)
        {
            Thing target = job.targetA.Thing;
            return "DoWorkAtThing".Translate(def.gerund.Named("GERUND"), target.LabelShort.Named("TARGETLABEL"));
        }
    }
}
