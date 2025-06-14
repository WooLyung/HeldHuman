using RimWorld;
using Verse.AI;
using Verse;
using HeldHuman.Tool;
using System.Collections.Generic;
using static RimWorld.ColonistBar;

namespace HeldHuman.Def
{
    public class WorkGiver_StripHeldHuman : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Strip);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return false;

            Pawn target = holder.HeldPawn;
            if (t.Map.designationManager.DesignationOn(target, DesignationDefOf.Strip) == null)
                return false;

            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return false;

            if (!StrippableUtility.CanBeStrippedByColony(target))
                return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return null;

            Pawn target = holder.HeldPawn;
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("StripHeldHuman"), target);
        }

        public override string PostProcessedGerund(Job job)
        {
            Thing target = job.targetA.Thing;
            return "DoWorkAtThing".Translate(def.gerund.Named("GERUND"), target.LabelShort.Named("TARGETLABEL"));
        }
    }
}
