using RimWorld;
using Verse.AI;
using Verse;
using HeldHuman.Tool;

namespace HeldHuman.Def
{
    public class WorkGiver_Warden_Enslave_HeldHuman : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModLister.CheckIdeology("WorkGiver_Warden_Enslave"))
                return false;

            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return false;
            Pawn target = holder.HeldPawn;

            PrisonerInteractionModeDef exclusiveInteractionMode = target.guest.ExclusiveInteractionMode;
            if ((exclusiveInteractionMode == PrisonerInteractionModeDefOf.ReduceWill || exclusiveInteractionMode == PrisonerInteractionModeDefOf.Enslave) && !target.guest.ScheduledForInteraction)
            {
                JobFailReason.Is("PrisonerInteractedTooRecently".Translate());
                return false;
            }

            return base.HasJobOnThing(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModLister.CheckIdeology("WorkGiver_Warden_Enslave"))
                return null;

            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return null;
            Pawn target = holder.HeldPawn;

            bool flag = target.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Enslave);
            bool flag2 = target.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.ReduceWill);
            if ((flag || flag2) && target.guest.ScheduledForInteraction && target.guest.IsPrisoner && (!flag2 || target.guest.will > 0f) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.CanReserve(t) && new HistoryEvent(HistoryEventDefOf.EnslavedPrisoner, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
            {
                if (flag)
                    return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("HeldHumanEnslave"), target);
                return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("HeldHumanReduceWill"), target);
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
