using HarmonyLib;
using HeldHuman.ToilHelper;
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
        private Thing Pawn => job.GetTarget(BillGiverInd).Thing;
        new private IBillGiver BillGiver => Pawn as IBillGiver;
        private Building_HoldingPlatform Platform => Pawn.ParentHolder as Building_HoldingPlatform;

        private List<Toil> Process1()
        {
            List<Toil> rawToils = base.MakeNewToils().ToList();
            List<Toil> newToils = new List<Toil>();

            for (int i = 0; i < rawToils.Count; i++)
            {
                Toil toil = rawToils[i];

                if (toil.debugName == "GotoThing" && rawToils[i - 1].debugName != "JumpIfTargetInsideBillGiver")
                    newToils.Add(Toils.GotoThing(Platform, GotoIngredientPathEndMode));
                else if (toil.debugName == "JumpIfTargetInsideBillGiver")
                    newToils.Add(JumpIfTargetInsideBillGiver(rawToils[i - 1]));
                else if (toil.debugName == "PlaceHauledThingInCell")
                    newToils.Add(Toils.PlaceHauledThingInCell(IngredientPlaceCellInd, rawToils[i - 1], storageMode: false));
                else if (toil.debugName == "JumpToCollectNextIntoHandsForBill")
                    newToils.Add(JumpToCollectNextIntoHandsForBill(rawToils[i - 2], IngredientInd));
                else if (toil.debugName == "DoRecipeWork")
                {
                    Toil workToil = Toils_Recipe.DoRecipeWork();
                    ToilFails.FailOnDespawnedNullOrForbiddenPlacedThings(workToil, Platform);
                    ToilFails.FailOnCannotTouch(workToil, Platform, GotoIngredientPathEndMode);
                    newToils.Add(workToil);
                }
                else
                    newToils.Add(toil);
            }

            return newToils;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            List<Toil> rawToils = Process1();
            for (int i = 0; i < rawToils.Count; i++)
            {
                Toil toil = rawToils[i];
                if (toil.debugName == "JumpIf")
                    yield return Toils_Jump.JumpIf(rawToils.FindLast(t => t.debugName == "GotoThing"), () => job.GetTargetQueue(IngredientInd).NullOrEmpty());
                else
                    yield return toil;
            }

            globalFailConditions.Clear();
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
        }

        private Toil JumpIfTargetInsideBillGiver(Toil jumpToil)
        {
            Toil toil = ToilMaker.MakeToil("JumpIfTargetInsideBillGiver");
            toil.initAction = delegate
            {
                Thing thing = Platform;
                if (thing != null && thing.Spawned)
                {
                    Thing thing2 = toil.actor.jobs.curJob.GetTarget(IngredientInd).Thing;
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

        new private static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            Toil toil = ToilMaker.MakeToil("JumpToCollectNextIntoHandsForBill");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error(string.Concat("JumpToAlsoCollectTargetInQueue run on ", actor, " who is not carrying something."));
                }
                else if (!actor.carryTracker.Full)
                {
                    Job curJob = actor.jobs.curJob;
                    List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
                    if (!targetQueue.NullOrEmpty())
                    {
                        for (int i = 0; i < targetQueue.Count; i++)
                        {
                            if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing) && targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing) && !((float)(actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared > 64f))
                            {
                                int num = ((actor.carryTracker.CarriedThing != null) ? actor.carryTracker.CarriedThing.stackCount : 0);
                                int a = curJob.countQueue[i];
                                a = Mathf.Min(a, targetQueue[i].Thing.def.stackLimit - num);
                                a = Mathf.Min(a, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
                                if (a > 0)
                                {
                                    curJob.count = a;
                                    curJob.SetTarget(ind, targetQueue[i].Thing);
                                    curJob.countQueue[i] -= a;
                                    if (curJob.countQueue[i] <= 0)
                                    {
                                        curJob.countQueue.RemoveAt(i);
                                        targetQueue.RemoveAt(i);
                                    }

                                    actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                                    break;
                                }
                            }
                        }
                    }
                }
            };
            return toil;
        }
    }
}