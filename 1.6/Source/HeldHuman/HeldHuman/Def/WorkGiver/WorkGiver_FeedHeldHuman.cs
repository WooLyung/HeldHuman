using HeldHuman.Tool;
using RimWorld;
using Verse;
using Verse.AI;

namespace HeldHuman.Def.WorkGiver_
{
    public class WorkGiver_FeedHeldHuman : WorkGiver_Warden_Feed
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

            if (target.needs.food == null)
                return null;

            if (target.needs.food.CurLevelPercentage >= target.needs.food.PercentageThreshHungry + 0.02f)
                return null;

            if (target.foodRestriction != null)
            {
                FoodPolicy currentRespectedRestriction = target.foodRestriction.GetCurrentRespectedRestriction(pawn);
                if (currentRespectedRestriction != null && currentRespectedRestriction.filter.AllowedDefCount == 0)
                {
                    JobFailReason.Is("NoFoodMatchingRestrictions".Translate());
                    return null;
                }
            }

            Thing foodSource;
            ThingDef foodDef;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, target, target.needs.food.CurCategory == HungerCategory.Starving, out foodSource, out foodDef, false, allowCorpse: false))
            {
                JobFailReason.Is((string)"NoFood".Translate());
                return null;
            }

            if (!pawn.CanReserveAndReach(t, PathEndMode, Danger.Deadly))
                return null;

            float nutrition = FoodUtility.GetNutrition(target, foodSource, foodDef);
            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("FeedHeldHuman"), foodSource, target);
            job.count = FoodUtility.WillIngestStackCountOf(target, foodDef, nutrition);

            if (!job.TryMakePreToilReservations(pawn, false))
                return null;

            return job;
        }
    }
}
