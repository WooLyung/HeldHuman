using RimWorld;
using Verse.AI;
using Verse;
using HeldHuman.Tool;

namespace HeldHuman.Def
{
    public class WorkGiver_ReleaseHeldHuman : WorkGiver_ReleaseEntity
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            if (GetEntity(t) == null)
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn entity = GetEntity(t);
            if (entity == null)
            {
                return null;
            }

            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("ReleaseHeldHuman"), t, entity).WithCount(1);
        }

        private Pawn GetEntity(Thing thing)
        {
            if (thing is Building_HoldingPlatform building_HoldingPlatform)
            {
                Pawn heldPawn = building_HoldingPlatform.HeldPawn;
                if (heldPawn == null || !HumanTool.IsHoldableHuman(heldPawn))
                    return null;

                if (heldPawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Release))
                    return heldPawn;
            }

            return null;
        }
    }
}
