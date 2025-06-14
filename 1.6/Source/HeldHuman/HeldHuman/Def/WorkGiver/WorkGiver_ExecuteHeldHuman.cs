using RimWorld;
using Verse.AI;
using Verse;
using HeldHuman.Tool;

namespace HeldHuman.Def
{
    public class WorkGiver_ExecuteHeldHuman : WorkGiver_ExecuteEntity
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            return GetEntity(t) != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("ExecuteHeldHuman"), t);
        }

        protected override Pawn GetEntity(Thing potentialPlatform)
        {
            if (potentialPlatform is Building_HoldingPlatform building_HoldingPlatform)
            {
                Pawn heldPawn = building_HoldingPlatform.HeldPawn;
                if (heldPawn == null || !HumanTool.IsHoldableHuman(heldPawn))
                    return null;

                if (heldPawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Execution))
                    return heldPawn;
            }

            return null;
        }
    }
}
