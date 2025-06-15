using RimWorld;
using Verse.AI;
using Verse;
using HeldHuman.Tool;

namespace HeldHuman.Def
{
    public class WorkGiver_Warden_Convert_HeldHuman : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModLister.CheckIdeology("WorkGiver_Warden_Convert"))
                return false;

            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return false;

            Pawn target = holder.HeldPawn;
            if (target.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Convert) && !target.guest.ScheduledForInteraction)
            {
                JobFailReason.Is("PrisonerInteractedTooRecently".Translate());
                return false;
            }

            return base.HasJobOnThing(pawn, t, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ModLister.CheckIdeology("WorkGiver_Warden_Convert"))
                return null;

            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return null;
            Pawn target = holder.HeldPawn;

            if (target.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Convert) && target.guest.ScheduledForInteraction && target.guest.IsPrisoner && target.Ideo != pawn.Ideo && pawn.Ideo == target.guest.ideoForConversion && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.CanReserve(t))
                return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("HeldHumanConvert"), target);

            return null;
        }

        public override string PostProcessedGerund(Job job)
        {
            Thing target = job.targetA.Thing;
            return "DoWorkAtThing".Translate(def.gerund.Named("GERUND"), target.LabelShort.Named("TARGETLABEL"));
        }
    }
}
