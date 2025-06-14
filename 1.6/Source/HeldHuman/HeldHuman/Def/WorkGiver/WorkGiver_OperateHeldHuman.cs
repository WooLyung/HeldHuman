using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace HeldHuman.Def.WorkGiver_
{
    public class WorkGiver_OperateHeldHuman : WorkGiver_DoBill
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);
        public override bool ShouldSkip(Pawn pawn, bool forced = false) => false;

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompEntityHolder holder = t.TryGetComp<CompEntityHolder>();
            if (holder == null || holder.HeldPawn == null || !HumanTool.IsHoldableHuman(holder.HeldPawn))
                return null;

            Pawn target = holder.HeldPawn;
            IBillGiver billGiver = target;

            if (billGiver == null || !ThingIsUsableBillGiver(target) || !billGiver.BillStack.AnyShouldDoNow || !pawn.CanReserve(target, 1, -1, null, forced) || t.IsBurning() || t.IsForbidden(pawn))
                return null;

            if (!pawn.CanReserveAndReach(t, PathEndMode, Danger.Deadly))
                return null;

            billGiver.BillStack.RemoveIncompletableBills();
            var method = AccessTools.Method(typeof(WorkGiver_DoBill), "StartOrResumeBillJob", new Type[] { typeof(Pawn), typeof(IBillGiver), typeof(bool) });
            Job job = (Job)method.Invoke(this, new object[] { pawn, billGiver, true });
            if (job != null)
                job.def = DefDatabase<JobDef>.GetNamed("OperateHeldHuman");

            if (!job.TryMakePreToilReservations(pawn, false))
                return null;

            return job;
        }
    }
}
