using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HeldHuman.Def.JobDriver
{
    public class JobDriver_OperateHeldHuman : JobDriver_DoBill
    {
        private static List<IntVec3> yieldedIngPlaceCells = new List<IntVec3>();
        private Thing Pawn => job.GetTarget(TargetIndex.A).Thing;
        new private IBillGiver BillGiver => Pawn as IBillGiver;
        private Building_HoldingPlatform Platform => Pawn.ParentHolder as Building_HoldingPlatform;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            AddEndCondition(delegate
            {
                return Pawn.IsOnHoldingPlatform ? JobCondition.Ongoing : JobCondition.Incompletable;
            });
            AddEndCondition(delegate
            {
                if (!Pawn.IsOnHoldingPlatform)
                    return JobCondition.Incompletable;
                return (Platform.Suspended || Platform.IsBurning()) ? JobCondition.Incompletable : JobCondition.Ongoing;
            });
            this.FailOn(delegate
            {
                if (BillGiver != null)
                {
                    if (job.bill.DeletedOrDereferenced)
                        return true;
                    if (!BillGiver.CurrentlyUsableForBills())
                        return true;
                }
                return false;
            });
            Toil gotoBillGiver = GotoThing(PathEndMode.Touch);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                if (job.targetQueueB != null && job.targetQueueB.Count == 1)
                {
                    UnfinishedThing unfinishedThing = job.targetQueueB[0].Thing as UnfinishedThing;
                    if (unfinishedThing != null)
                        unfinishedThing.BoundBill = (Bill_ProductionWithUft)job.bill;
                }
                job.bill.Notify_DoBillStarted(pawn);
            };
            yield return toil;
            yield return Toils_Jump.JumpIf(gotoBillGiver, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
            foreach (Toil item in CollectIngredientsToils(TargetIndex.B, TargetIndex.C, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true, BillGiver is Building_WorkTableAutonomous))
                yield return item;
            yield return gotoBillGiver;
            yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();

            Toil workToil = Toils_Recipe.DoRecipeWork();
            FailOnDespawnedNullOrForbiddenPlacedThings(workToil);
            FailOnCannotTouch(workToil, PathEndMode.Touch);
            yield return workToil;

            yield return Toils_Recipe.CheckIfRecipeCanFinishNow();
            yield return FinishRecipeAndStartStoringProduct(TargetIndex.None);
        }

        private static List<Thing> CalculateIngredients(Job job, Pawn actor)
        {
            UnfinishedThing unfinishedThing = job.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
            if (unfinishedThing != null)
            {
                List<Thing> ingredients = unfinishedThing.ingredients;
                job.RecipeDef.Worker.ConsumeIngredient(unfinishedThing, job.RecipeDef, actor.Map);
                job.placedThings = null;
                return ingredients;
            }

            List<Thing> list = new List<Thing>();
            if (job.placedThings != null)
            {
                for (int i = 0; i < job.placedThings.Count; i++)
                {
                    if (job.placedThings[i].Count <= 0)
                    {
                        Log.Error(string.Concat("PlacedThing ", job.placedThings[i], " with count ", job.placedThings[i].Count, " for job ", job));
                        continue;
                    }

                    Thing thing = ((job.placedThings[i].Count >= job.placedThings[i].thing.stackCount) ? job.placedThings[i].thing : job.placedThings[i].thing.SplitOff(job.placedThings[i].Count));
                    job.placedThings[i].Count = 0;
                    if (list.Contains(thing))
                    {
                        Log.Error("Tried to add ingredient from job placed targets twice: " + thing);
                        continue;
                    }

                    list.Add(thing);
                    if (job.RecipeDef.autoStripCorpses)
                    {
                        IStrippable strippable = thing as IStrippable;
                        if (strippable != null && strippable.AnythingToStrip())
                        {
                            strippable.Strip();
                        }
                    }
                }
            }

            job.placedThings = null;
            return list;
        }

        private static Thing CalculateDominantIngredient(Job job, List<Thing> ingredients)
        {
            UnfinishedThing uft = job.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
            if (uft != null && uft.def.MadeFromStuff)
            {
                return uft.ingredients.First((Thing ing) => ing.def == uft.Stuff);
            }

            if (!ingredients.NullOrEmpty())
            {
                RecipeDef recipeDef = job.RecipeDef;
                if (recipeDef.productHasIngredientStuff)
                {
                    return ingredients[0];
                }

                if (recipeDef.products.Any((ThingDefCountClass x) => x.thingDef.MadeFromStuff) || (recipeDef.unfinishedThingDef != null && recipeDef.unfinishedThingDef.MadeFromStuff))
                {
                    return ingredients.Where((Thing x) => x.def.IsStuff).RandomElementByWeight((Thing x) => x.stackCount);
                }

                return ingredients.RandomElementByWeight((Thing x) => x.stackCount);
            }

            return null;
        }

        private static void ConsumeIngredients(List<Thing> ingredients, RecipeDef recipe, Map map)
        {
            for (int i = 0; i < ingredients.Count; i++)
                recipe.Worker.ConsumeIngredient(ingredients[i], recipe, map);
        }

        public static Toil FinishRecipeAndStartStoringProduct(TargetIndex productIndex = TargetIndex.A)
        {
            Toil toil = ToilMaker.MakeToil("FinishRecipeAndStartStoringProduct");
            toil.AddFinishAction(delegate
            {
                Bill_Production bill_Production;
                if ((bill_Production = toil.actor.jobs.curJob.bill as Bill_Production) != null && bill_Production.repeatMode == BillRepeatModeDefOf.TargetCount)
                    toil.actor.Map.resourceCounter.UpdateResourceCounts();
            });
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                JobDriver_DoBill jobDriver_DoBill = (JobDriver_DoBill)actor.jobs.curDriver;
                if (curJob.RecipeDef.workSkill != null && !curJob.RecipeDef.UsesUnfinishedThing && actor.skills != null)
                {
                    float xp = (float)jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.1f * curJob.RecipeDef.workSkillLearnFactor;
                    actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp);
                }

                List<Thing> ingredients = CalculateIngredients(curJob, actor);
                Thing dominantIngredient = CalculateDominantIngredient(curJob, ingredients);

                ThingStyleDef style = null;
                if (ModsConfig.IdeologyActive && curJob.bill.recipe.products != null && curJob.bill.recipe.products.Count == 1)
                {
                    style = ((!curJob.bill.globalStyle) ? curJob.bill.style : Faction.OfPlayer.ideos.PrimaryIdeo.style.StyleForThingDef(curJob.bill.recipe.ProducedThingDef)?.styleDef);
                }

                Bill_Mech bill;
                List<Thing> list = (((bill = curJob.bill as Bill_Mech) != null) ? GenRecipe.FinalizeGestatedPawns(bill, actor, style).ToList() : GenRecipe.MakeRecipeProducts(curJob.RecipeDef, actor, ingredients, dominantIngredient, jobDriver_DoBill.BillGiver, curJob.bill.precept, style, curJob.bill.graphicIndexOverride).ToList());
                ConsumeIngredients(ingredients, curJob.RecipeDef, actor.Map);
                curJob.bill.Notify_IterationCompleted(actor, ingredients);
                RecordsUtility.Notify_BillDone(actor, list);
                if (curJob?.bill == null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!GenPlace.TryPlaceThing(list[i], actor.Position, actor.Map, ThingPlaceMode.Near))
                        {
                            Log.Error(string.Concat(actor, " could not drop recipe product ", list[i], " near ", actor.Position));
                        }
                    }
                }
                else
                {
                    Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
                    if (curJob.bill.recipe.WorkAmountTotal(thing) >= 10000f && list.Count > 0)
                    {
                        TaleRecorder.RecordTale(TaleDefOf.CompletedLongCraftingProject, actor, list[0].GetInnerIfMinified().def);
                    }

                    if (list.Any())
                    {
                        Find.QuestManager.Notify_ThingsProduced(actor, list);
                    }

                    if (list.Count == 0)
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                    else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.DropOnFloor)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (!GenPlace.TryPlaceThing(list[j], actor.Position, actor.Map, ThingPlaceMode.Near))
                            {
                                Log.Error($"{actor} could not drop recipe product {list[j]} near {actor.Position}");
                            }
                        }

                        actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                    else
                    {
                        if (list.Count > 1)
                        {
                            for (int k = 1; k < list.Count; k++)
                            {
                                if (!GenPlace.TryPlaceThing(list[k], actor.Position, actor.Map, ThingPlaceMode.Near))
                                {
                                    Log.Error($"{actor} could not drop recipe product {list[k]} near {actor.Position}");
                                }
                            }
                        }

                        IntVec3 foundCell = IntVec3.Invalid;
                        if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.BestStockpile)
                        {
                            StoreUtility.TryFindBestBetterStoreCellFor(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, out foundCell);
                        }
                        else if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.SpecificStockpile)
                        {
                            StoreUtility.TryFindBestBetterStoreCellForIn(list[0], actor, actor.Map, StoragePriority.Unstored, actor.Faction, curJob.bill.GetSlotGroup(), out foundCell);
                        }
                        else
                        {
                            Log.ErrorOnce("Unknown store mode", 9158246);
                        }

                        if (foundCell.IsValid)
                        {
                            int num = actor.carryTracker.MaxStackSpaceEver(list[0].def);
                            if (num < list[0].stackCount)
                            {
                                int count = list[0].stackCount - num;
                                Thing thing2 = list[0].SplitOff(count);
                                if (!GenPlace.TryPlaceThing(thing2, actor.Position, actor.Map, ThingPlaceMode.Near))
                                {
                                    Log.Error($"{actor} could not drop recipe extra product that pawn couldn't carry, {thing2} near {actor.Position}");
                                }
                            }

                            if (num == 0)
                            {
                                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                            }
                            else
                            {
                                actor.carryTracker.TryStartCarry(list[0]);
                                actor.jobs.StartJob(HaulAIUtility.HaulToCellStorageJob(actor, list[0], foundCell, fitInStoreCell: false), JobCondition.Succeeded, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, null, fromQueue: false, canReturnCurJobToPool: false, true);
                            }
                        }
                        else
                        {
                            if (!GenPlace.TryPlaceThing(list[0], actor.Position, actor.Map, ThingPlaceMode.Near))
                            {
                                Log.Error($"Bill doer could not drop product {list[0]} near {actor.Position}");
                            }

                            actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                        }
                    }
                }
            };
            return toil;
        }

        public Toil FailOnCannotTouch(Toil f, PathEndMode peMode)
        {
            f.AddEndCondition(() => f.GetActor().CanReachImmediate(Platform, peMode) ? JobCondition.Ongoing : JobCondition.Incompletable);
            return f;
        }

        public Toil FailOnDespawnedNullOrForbiddenPlacedThings(Toil toil)
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
                    ThingOwner thingOwner = Platform?.TryGetInnerInteractableThingOwner();
                    if (thingCountClass.thing == null || (!thingCountClass.thing.Spawned && (thingOwner == null || !thingOwner.Contains(thingCountClass.thing))) || thingCountClass.thing.MapHeld != toil.actor.Map || (!toil.actor.CurJob.ignoreForbidden && thingCountClass.thing.IsForbidden(toil.actor)))
                    {
                        return true;
                    }
                }

                return false;
            });
            return toil;
        }

        public Toil FailOnSelfAndParentsDespawnedOrNull(Toil f)
        {
            f.AddEndCondition(() => (!ToilFailConditions.SelfAndParentsDespawnedOrNull(Platform, f.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable);
            return f;
        }

        public Toil FailOnDespawnedOrNull(Toil f)
        {
            f.AddEndCondition(() => (!ToilFailConditions.DespawnedOrNull(Platform, f.GetActor())) ? JobCondition.Ongoing : JobCondition.Incompletable);
            return f;
        }

        public Toil GotoThing(PathEndMode peMode, bool canGotoSpawnedParent = false)
        {
            Toil toil = ToilMaker.MakeToil("GotoThing");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                actor.pather.StartPath(Platform, peMode);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            if (canGotoSpawnedParent)
                FailOnSelfAndParentsDespawnedOrNull(toil);
            else
                FailOnDespawnedOrNull(toil);
            return toil;
        }

        private static IEnumerable<IntVec3> IngredientPlaceCellsInOrder(Thing destination)
        {
            yieldedIngPlaceCells.Clear();
            try
            {
                IntVec3 interactCell = destination.Position;
                foreach (IntVec3 item in (new IntVec3[] { interactCell }.OrderBy((IntVec3 c) => (c - interactCell).LengthHorizontalSquared)))
                {
                    yieldedIngPlaceCells.Add(item);
                    yield return item;
                }
                
                for (int i = 0; i < 200; i++)
                {
                    IntVec3 intVec = interactCell + GenRadial.RadialPattern[i];
                    if (!yieldedIngPlaceCells.Contains(intVec))
                    {
                        Building edifice = intVec.GetEdifice(destination.Map);
                        if (edifice == null || edifice.def.passability != Traversability.Impassable || edifice.def.surfaceType != 0)
                        {
                            yield return intVec;
                        }
                    }
                }
            }
            finally
            {
                yieldedIngPlaceCells.Clear();
            }
        }

        public Toil SetTargetToIngredientPlaceCell(TargetIndex carryItemInd, TargetIndex cellTargetInd)
        {
            Toil toil = ToilMaker.MakeToil("SetTargetToIngredientPlaceCell");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Thing thing = curJob.GetTarget(carryItemInd).Thing;
                IntVec3 intVec = IntVec3.Invalid;

                foreach (IntVec3 item in IngredientPlaceCellsInOrder(Platform))
                {
                    if (!intVec.IsValid)
                        intVec = item;

                    bool flag = false;
                    List<Thing> list = actor.Map.thingGrid.ThingsListAt(item);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].def.category == ThingCategory.Item && (!list[i].CanStackWith(thing) || list[i].stackCount == list[i].def.stackLimit))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        curJob.SetTarget(cellTargetInd, item);
                        return;
                    }
                }

                curJob.SetTarget(cellTargetInd, intVec);
            };
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

        public IEnumerable<Toil> CollectIngredientsToils(TargetIndex ingredientInd, TargetIndex ingredientPlaceCellInd, bool subtractNumTakenFromJobCount = false, bool failIfStackCountLessThanJobCount = true, bool placeInBillGiver = false)
        {
            Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientInd);
            yield return extract;
            Toil jumpIfHaveTargetInQueue = Toils_Jump.JumpIfHaveTargetInQueue(ingredientInd, extract);
            yield return JumpIfTargetInsideBillGiver(jumpIfHaveTargetInQueue, ingredientInd);
            Toil getToHaulTarget = Toils_Goto.GotoThing(ingredientInd, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(ingredientInd).FailOnSomeonePhysicallyInteracting(ingredientInd);
            yield return getToHaulTarget;
            yield return Toils_Haul.StartCarryThing(ingredientInd, putRemainderInQueue: true, subtractNumTakenFromJobCount, failIfStackCountLessThanJobCount, reserve: false);
            yield return JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
            yield return GotoThing(PathEndMode.Touch).FailOnDestroyedOrNull(ingredientInd);
            if (!placeInBillGiver)
            {
                Toil findPlaceTarget = SetTargetToIngredientPlaceCell(ingredientInd, ingredientPlaceCellInd);
                yield return findPlaceTarget;
                yield return PlaceHauledThingInCell(ingredientPlaceCellInd, findPlaceTarget, storageMode: false);
                Toil physReserveToil = ToilMaker.MakeToil("CollectIngredientsToils");
                physReserveToil.initAction = delegate
                {
                    physReserveToil.actor.Map.physicalInteractionReservationManager.Reserve(physReserveToil.actor, physReserveToil.actor.CurJob, physReserveToil.actor.CurJob.GetTarget(ingredientInd));
                };
                yield return physReserveToil;
            }
            else
            {
                yield return DepositHauledThingInContainer(ingredientInd);
            }

            yield return jumpIfHaveTargetInQueue;
        }

        public Toil DepositHauledThingInContainer(TargetIndex reserveForContainerInd, Action onDeposited = null)
        {
            Toil toil = ToilMaker.MakeToil("DepositHauledThingInContainer");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error(string.Concat(actor, " tried to place hauled thing in container but is not hauling anything."));
                }
                else
                {
                    Thing thing = Platform;
                    ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
                    if (thingOwner != null)
                    {
                        int num = actor.carryTracker.CarriedThing.stackCount;
                        IHaulEnroute haulEnroute;
                        if ((haulEnroute = thing as IHaulEnroute) != null)
                        {
                            ThingDef def = actor.carryTracker.CarriedThing.def;
                            num = Mathf.Min(haulEnroute.GetSpaceRemainingWithEnroute(def, actor), num);
                            if (reserveForContainerInd != 0)
                            {
                                Thing thing2 = curJob.GetTarget(reserveForContainerInd).Thing;
                                IHaulEnroute enroute;
                                if (!thing2.DestroyedOrNull() && thing2 != haulEnroute && (enroute = thing2 as IHaulEnroute) != null)
                                {
                                    int spaceRemainingWithEnroute = enroute.GetSpaceRemainingWithEnroute(def, actor);
                                    num = Mathf.Min(num, actor.carryTracker.CarriedThing.stackCount - spaceRemainingWithEnroute);
                                }
                            }
                        }

                        Thing carriedThing = actor.carryTracker.CarriedThing;
                        int num2 = actor.carryTracker.innerContainer.TryTransferToContainer(carriedThing, thingOwner, num);
                        if (num2 != 0)
                        {
                            IHaulEnroute container;
                            if ((container = thing as IHaulEnroute) != null)
                            {
                                thing.Map.enrouteManager.ReleaseFor(container, actor);
                            }

                            INotifyHauledTo notifyHauledTo;
                            if ((notifyHauledTo = thing as INotifyHauledTo) != null)
                            {
                                notifyHauledTo.Notify_HauledTo(actor, carriedThing, num2);
                            }

                            ThingWithComps thingWithComps;
                            if ((thingWithComps = thing as ThingWithComps) != null)
                            {
                                foreach (ThingComp allComp in thingWithComps.AllComps)
                                {
                                    INotifyHauledTo notifyHauledTo2;
                                    if ((notifyHauledTo2 = allComp as INotifyHauledTo) != null)
                                    {
                                        notifyHauledTo2.Notify_HauledTo(actor, carriedThing, num2);
                                    }
                                }
                            }

                            HaulAIUtility.UpdateJobWithPlacedThings(curJob, carriedThing, num2);
                            onDeposited?.Invoke();
                        }
                    }
                    else if (Platform.def.Minifiable)
                    {
                        actor.carryTracker.innerContainer.ClearAndDestroyContents();
                    }
                    else
                    {
                        Log.Error("Could not deposit hauled thing in container: " + Platform);
                    }
                }
            };
            return toil;
        }

        private Toil JumpIfTargetInsideBillGiver(Toil jumpToil, TargetIndex ingredient)
        {
            Toil toil = ToilMaker.MakeToil("JumpIfTargetInsideBillGiver");
            toil.initAction = delegate
            {
                Thing thing = Platform;
                if (thing != null && thing.Spawned)
                {
                    Thing thing2 = toil.actor.jobs.curJob.GetTarget(ingredient).Thing;
                    if (thing2 != null)
                    {
                        ThingOwner thingOwner = thing.TryGetInnerInteractableThingOwner();
                        if (thingOwner != null && thingOwner.Contains(thing2))
                        {
                            HaulAIUtility.UpdateJobWithPlacedThings(toil.actor.jobs.curJob, thing2, thing2.stackCount);
                            toil.actor.jobs.curDriver.JumpToToil(jumpToil);
                        }
                    }
                }
            };
            return toil;
        }
    }
}
