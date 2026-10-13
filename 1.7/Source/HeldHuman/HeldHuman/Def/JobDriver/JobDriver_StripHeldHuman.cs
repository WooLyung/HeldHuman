using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using HeldHuman.Tool;

namespace HeldHuman.Def
{
    public class JobDriver_StripHeldHuman : JobDriver_Strip
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOn(() => !StrippableUtility.CanBeStrippedByColony(TargetThingA));
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                pawn.pather.StartPath(TargetThingA, PathEndMode.ClosestTouch);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            yield return toil;
            yield return Toils_General.Wait(60).WithProgressBarToilDelay(TargetIndex.A);
            Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
            toil2.initAction = delegate
            {
                Thing thing = job.targetA.Thing;
                Map.designationManager.DesignationOn(thing, DesignationDefOf.Strip)?.Delete();
                if (thing is IStrippable strippable)
                {
                    strippable.Strip();
                }

                pawn.records.Increment(RecordDefOf.BodiesStripped);
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
        }
    }
}
