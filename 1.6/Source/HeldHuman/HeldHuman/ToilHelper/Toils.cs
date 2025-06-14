using Verse.AI;
using Verse;
using RimWorld;
using System;

namespace HeldHuman.ToilHelper
{
    public static class Toils
    {
        public static Toil GotoThing(Thing target, PathEndMode peMode, bool canGotoSpawnedParent = false)
        {
            Toil toil = ToilMaker.MakeToil("GotoThing");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Thing thing = target;
                if (thing != null && canGotoSpawnedParent)
                    target = thing.SpawnedParentOrMe;

                actor.pather.StartPath(target, peMode);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            if (canGotoSpawnedParent)
                ToilFails.FailOnSelfAndParentsDespawnedOrNull(toil, target);
            else
                ToilFails.FailOnDespawnedOrNull(toil, target);

            return toil;
        }

        public static Toil PlaceHauledThingInCell(TargetIndex cellInd, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode, bool tryStoreInSameStorageIfSpotCantHoldWholeStack = false)
        {
            Toil toil = ToilMaker.MakeToil("PlaceHauledThingInCell");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                IntVec3 cell = curJob.GetTarget(cellInd).Cell;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error(string.Concat(actor, " tried to place hauled thing in cell but is not hauling anything."));
                }
                else
                {
                    SlotGroup slotGroup = actor.Map.haulDestinationManager.SlotGroupAt(cell);
                    if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carryTracker.CarriedThing))
                    {
                        actor.Map.designationManager.TryRemoveDesignationOn(actor.carryTracker.CarriedThing, DesignationDefOf.Haul);
                    }

                    Action<Thing, int> placedAction = null;
                    placedAction = delegate (Thing th, int added)
                    {
                        HaulAIUtility.UpdateJobWithPlacedThings(curJob, th, added);
                    };

                    if (!actor.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out var _, placedAction))
                    {
                        if (storageMode)
                        {
                            IntVec3 storeCell;
                            if (nextToilOnPlaceFailOrIncomplete != null && ((tryStoreInSameStorageIfSpotCantHoldWholeStack && StoreUtility.TryFindBestBetterStoreCellForIn(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetSlotGroup(), out var foundCell)) || StoreUtility.TryFindBestBetterStoreCellFor(actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell)))
                            {
                                if (actor.CanReserve(foundCell))
                                {
                                    actor.Reserve(foundCell, actor.CurJob);
                                }

                                actor.CurJob.SetTarget(cellInd, foundCell);
                                actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
                            }
                            else if (HaulAIUtility.CanHaulAside(actor, actor.carryTracker.CarriedThing, out storeCell))
                            {
                                curJob.SetTarget(cellInd, storeCell);
                                curJob.count = int.MaxValue;
                                curJob.haulOpportunisticDuplicates = false;
                                curJob.haulMode = HaulMode.ToCellNonStorage;
                                actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
                            }
                            else
                            {
                                Log.Warning($"Incomplete haul for {actor}: Could not find anywhere to put {actor.carryTracker.CarriedThing} near {actor.Position}. Destroying. This should be very uncommon!");
                                actor.carryTracker.CarriedThing.Destroy();
                            }
                        }
                        else if (nextToilOnPlaceFailOrIncomplete != null)
                        {
                            actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
                        }
                    }
                }
            };
            return toil;
        }

        public static Toil TryRecruit(TargetIndex recruiteeInd)
        {
            Toil toil = ToilMaker.MakeToil("TryRecruit");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(recruiteeInd).Thing;
                actor.interactions.TryInteractWith(pawn, InteractionDefOf.RecruitAttempt);

                if (pawn.IsColonist && pawn.IsOnHoldingPlatform)
                {
                    var platform = pawn.ParentHolder as Building_HoldingPlatform;
                    platform.EjectContents();
                }
            };
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 350;
            toil.activeSkill = () => SkillDefOf.Social;
            return toil;
        }
    }
}