using Verse.AI;
using Verse;
using RimWorld;

namespace HeldHuman.ToilHelper
{
    public static class ToilFails
    {
        public static void FailOnSelfAndParentsDespawnedOrNull(Toil toil, Thing target)
        {
            toil.AddEndCondition(() => (!ToilFailConditions.SelfAndParentsDespawnedOrNull(target, toil.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable);
        }

        public static void FailOnDespawnedOrNull(Toil toil, Thing target)
        {
            toil.AddEndCondition(() => (!ToilFailConditions.DespawnedOrNull(target, toil.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable);
        }

        public static Toil FailOnCannotTouch(Toil toil, Thing target, PathEndMode peMode)
        {
            toil.AddEndCondition(() => toil.GetActor().CanReachImmediate(target, peMode) ? JobCondition.Ongoing : JobCondition.Incompletable);
            return toil;
        }

        public static Toil FailOnDespawnedNullOrForbiddenPlacedThings(Toil toil, Thing target)
        {
            toil.AddFailCondition(delegate
            {
                if (toil.actor.jobs.curJob.placedThings == null)
                {
                    return false;
                }

                for (int i = 0; i < toil.actor.jobs.curJob.placedThings.Count; i++)
                {
                    ThingCountClass thingCountClass = toil.actor.jobs.curJob.placedThings[i];
                    ThingOwner thingOwner = target?.TryGetInnerInteractableThingOwner();
                    if (thingCountClass.thing == null || (!thingCountClass.thing.Spawned && (thingOwner == null || !thingOwner.Contains(thingCountClass.thing))) || thingCountClass.thing.MapHeld != toil.actor.Map || (!toil.actor.CurJob.ignoreForbidden && thingCountClass.thing.IsForbidden(toil.actor)))
                    {
                        return true;
                    }
                }

                return false;
            });
            return toil;
        }
    }
}