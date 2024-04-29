using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HeldHuman.Def.WorkGiver
{
    public class WorkGiver_OperateHeldHuman : WorkGiver_DoBill
    {
        private class DefCountList
        {
            private List<ThingDef> defs = new List<ThingDef>();

            private List<float> counts = new List<float>();

            public int Count => defs.Count;

            public float this[ThingDef def]
            {
                get
                {
                    int num = defs.IndexOf(def);
                    if (num < 0)
                    {
                        return 0f;
                    }

                    return counts[num];
                }
                set
                {
                    int num = defs.IndexOf(def);
                    if (num < 0)
                    {
                        defs.Add(def);
                        counts.Add(value);
                        num = defs.Count - 1;
                    }
                    else
                    {
                        counts[num] = value;
                    }

                    CheckRemove(num);
                }
            }

            public float GetCount(int index)
            {
                return counts[index];
            }

            public void SetCount(int index, float val)
            {
                counts[index] = val;
                CheckRemove(index);
            }

            public ThingDef GetDef(int index)
            {
                return defs[index];
            }

            private void CheckRemove(int index)
            {
                if (counts[index] == 0f)
                {
                    counts.RemoveAt(index);
                    defs.RemoveAt(index);
                }
            }

            public void Clear()
            {
                defs.Clear();
                counts.Clear();
            }

            public void GenerateFrom(List<Thing> things)
            {
                Clear();
                for (int i = 0; i < things.Count; i++)
                {
                    this[things[i].def] += things[i].stackCount;
                }
            }
        }

        private static readonly IntRange ReCheckFailedBillTicksRange = new IntRange(500, 600);
        private static CompEntityHolder holderTmp = null;
        private static Pawn patient = null;

        private static DefCountList availableCounts = new DefCountList();
        private static List<Thing> relevantThings = new List<Thing>();
        private static HashSet<Thing> processedThings = new HashSet<Thing>();
        private static List<Thing> newRelevantThings = new List<Thing>();
        private static List<Thing> tmpMedicine = new List<Thing>();
        private static List<IngredientCount> missingIngredients = new List<IngredientCount>();
        private static List<Thing> tmpMissingUniqueIngredients = new List<Thing>();
        private List<ThingCount> chosenIngThings = new List<ThingCount>();

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

            holderTmp = holder;
            patient = target;
            billGiver.BillStack.RemoveIncompletableBills();
            Job job = StartOrResumeBillJob(pawn, billGiver, forced);
            holderTmp = null;
            patient = null;
            return job;
        }

        public static Job HaulStuffOffBillGiverJob(Pawn pawn, Thing thingToIgnore)
        {
            foreach (IntVec3 ingredientStackCell in holderTmp.parent.InteractionCells)
            {
                Thing thing = pawn.Map.thingGrid.ThingAt(ingredientStackCell, ThingCategory.Item);
                if (thing != null && thing != thingToIgnore)
                {
                    return HaulAIUtility.HaulAsideJobFor(pawn, thing);
                }
            }

            return null;
        }

        public static Job TryStartNewDoBillJob0(Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob, bool dontCreateJobIfHaulOffRequired = true)
        {
            haulOffJob = HaulStuffOffBillGiverJob(pawn, null);
            if (haulOffJob != null && dontCreateJobIfHaulOffRequired)
            {
                return haulOffJob;
            }

            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("OperateHeldHuman"), (Thing)giver);
            job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
            job.countQueue = new List<int>(chosenIngThings.Count);
            for (int i = 0; i < chosenIngThings.Count; i++)
            {
                job.targetQueueB.Add(chosenIngThings[i].Thing);
                job.countQueue.Add(chosenIngThings[i].Count);
            }

            if (bill.xenogerm != null)
            {
                job.targetQueueB.Add(bill.xenogerm);
                job.countQueue.Add(1);
            }

            job.haulMode = HaulMode.ToCellNonStorage;
            job.bill = bill;
            return job;
        }

        private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
        {
            var method = AccessTools.Method(typeof(WorkGiver_DoBill), "FinishUftJob", new Type[] { typeof(Pawn), typeof(UnfinishedThing), typeof(Bill_ProductionWithUft) });
            return (Job) method.Invoke(null, new object[] { pawn, uft, bill });
        }

        private static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
        {
            var method = AccessTools.Method(typeof(WorkGiver_DoBill), "ClosestUnfinishedThingForBill", new Type[] { typeof(Pawn), typeof(Bill_ProductionWithUft) });
            return (UnfinishedThing) method.Invoke(null, new object[] { pawn, bill });
        }

        private static Job WorkOnFormedBill(Thing giver, Bill_Autonomous bill)
        {
            var method = AccessTools.Method(typeof(WorkGiver_DoBill), "WorkOnFormedBill", new Type[] { typeof(Thing), typeof(Bill_Autonomous) });
            return (Job) method.Invoke(null, new object[] { giver, bill });
        }

        private bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen, List<IngredientCount> missingIngredients)
        {
            return TryFindBestIngredientsHelper((Thing t) => IsUsableIngredient(t, bill), (List<Thing> foundThings) => TryFindBestBillIngredientsInSet(foundThings, bill, chosen, GetBillGiverRootCell(holderTmp.parent, pawn), billGiver is Pawn, missingIngredients), bill.recipe.ingredients, pawn, chosen, bill.ingredientSearchRadius);
        }

        private static void AddEveryMedicineToRelevantThings(Pawn pawn, Thing billGiver, List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map)
        {
            MedicalCareCategory medicalCareCategory = GetMedicalCareCategory(patient);
            List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
            tmpMedicine.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (medicalCareCategory.AllowsMedicine(thing.def) && baseValidator(thing) && pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly))
                {
                    tmpMedicine.Add(thing);
                }
            }

            tmpMedicine.SortBy((Thing x) => 0f - x.GetStatValue(StatDefOf.MedicalPotency), (Thing x) => x.Position.DistanceToSquared(billGiver.Position));
            relevantThings.AddRange(tmpMedicine);
            tmpMedicine.Clear();
        }

        private bool TryFindBestIngredientsHelper(Predicate<Thing> thingValidator, Predicate<List<Thing>> foundAllIngredientsAndChoose, List<IngredientCount> ingredients, Pawn pawn, List<ThingCount> chosen, float searchRadius)
        {
            chosen.Clear();
            newRelevantThings.Clear();
            if (ingredients.Count == 0)
            {
                return true;
            }

            IntVec3 billGiverRootCell = GetBillGiverRootCell(holderTmp.parent, pawn);
            Region rootReg = billGiverRootCell.GetRegion(pawn.Map);
            if (rootReg == null)
            {
                return false;
            }

            relevantThings.Clear();
            processedThings.Clear();
            bool foundAll = false;
            float radiusSq = searchRadius * searchRadius;
            Predicate<Thing> baseValidator = (Thing t) => t.Spawned && thingValidator(t) && (float)(t.Position - holderTmp.parent.Position).LengthHorizontalSquared < radiusSq && !t.IsForbidden(pawn) && pawn.CanReserve(t);
            AddEveryMedicineToRelevantThings(pawn, holderTmp.parent, relevantThings, baseValidator, pawn.Map);
            if (foundAllIngredientsAndChoose(relevantThings))
            {
                relevantThings.Clear();
                return true;
            }

            TraverseParms traverseParams = TraverseParms.For(pawn);
            RegionEntryPredicate entryCondition = null;
            if (Math.Abs(999f - searchRadius) >= 1f)
            {
                entryCondition = delegate (Region from, Region r)
                {
                    if (!r.Allows(traverseParams, isDestination: false))
                    {
                        return false;
                    }

                    CellRect extentsClose = r.extentsClose;
                    int num = Math.Abs(holderTmp.parent.Position.x - Math.Max(extentsClose.minX, Math.Min(holderTmp.parent.Position.x, extentsClose.maxX)));
                    if ((float)num > searchRadius)
                    {
                        return false;
                    }

                    int num2 = Math.Abs(holderTmp.parent.Position.z - Math.Max(extentsClose.minZ, Math.Min(holderTmp.parent.Position.z, extentsClose.maxZ)));
                    return !((float)num2 > searchRadius) && (float)(num * num + num2 * num2) <= radiusSq;
                };
            }
            else
            {
                entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
            }

            int adjacentRegionsAvailable = rootReg.Neighbors.Count((Region region) => entryCondition(rootReg, region));
            int regionsProcessed = 0;
            processedThings.AddRange(relevantThings);
            foundAllIngredientsAndChoose(relevantThings);
            RegionProcessor regionProcessor = delegate (Region r)
            {
                List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing = list[i];
                    if (!processedThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.Touch, pawn) && baseValidator(thing) && !thing.def.IsMedicine)
                    {
                        newRelevantThings.Add(thing);
                        processedThings.Add(thing);
                    }
                }

                regionsProcessed++;
                if (newRelevantThings.Count > 0 && regionsProcessed > adjacentRegionsAvailable)
                {
                    relevantThings.AddRange(newRelevantThings);
                    newRelevantThings.Clear();
                    if (foundAllIngredientsAndChoose(relevantThings))
                    {
                        foundAll = true;
                        return true;
                    }
                }

                return false;
            };
            RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999);
            relevantThings.Clear();
            newRelevantThings.Clear();
            processedThings.Clear();
            return foundAll;
        }

        private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
        {
            return billGiver.Position;
        }

        private static bool IsUsableIngredient(Thing t, Bill bill)
        {
            var method = AccessTools.Method(typeof(WorkGiver_DoBill), "IsUsableIngredient", new Type[] { typeof(Thing), typeof(Bill) });
            return (bool) method.Invoke(null, new object[] { t, bill });
        }

        private bool CannotDoBillDueToMedicineRestriction(Pawn pawn, Bill bill, List<IngredientCount> missingIngredients)
        {
            bool flag = false;
            foreach (IngredientCount missingIngredient in missingIngredients)
            {
                if (missingIngredient.filter.Allows(ThingDefOf.MedicineIndustrial))
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                MedicalCareCategory medicalCareCategory = GetMedicalCareCategory(pawn);
                foreach (Thing item in pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine))
                {
                    if (IsUsableIngredient(item, bill) && medicalCareCategory.AllowsMedicine(item.def))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private static bool TryFindBestBillIngredientsInSet(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients)
        {
            if (bill.recipe.allowMixingIngredients)
                return TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen, rootCell, missingIngredients);
            return TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen, rootCell, alreadySorted, missingIngredients);
        }

        private static bool TryFindBestBillIngredientsInSet_NoMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients)
        {
            return TryFindBestIngredientsInSet_NoMixHelper(availableThings, bill.recipe.ingredients, chosen, rootCell, alreadySorted, missingIngredients, bill);
        }

        private static bool TryFindBestIngredientsInSet_NoMixHelper(List<Thing> availableThings, List<IngredientCount> ingredients, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted, List<IngredientCount> missingIngredients, Bill bill = null)
        {
            if (!alreadySorted)
            {
                Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
                {
                    float num4 = (t1.PositionHeld - rootCell).LengthHorizontalSquared;
                    float value = (t2.PositionHeld - rootCell).LengthHorizontalSquared;
                    return num4.CompareTo(value);
                };
                availableThings.Sort(comparison);
            }

            chosen.Clear();
            availableCounts.Clear();
            missingIngredients?.Clear();
            availableCounts.GenerateFrom(availableThings);
            for (int i = 0; i < ingredients.Count; i++)
            {
                IngredientCount ingredientCount = ingredients[i];
                bool flag = false;
                for (int j = 0; j < availableCounts.Count; j++)
                {
                    float num = ((bill != null) ? ((float)ingredientCount.CountRequiredOfFor(availableCounts.GetDef(j), bill.recipe, bill)) : ingredientCount.GetBaseCount());
                    if ((bill != null && !bill.recipe.ignoreIngredientCountTakeEntireStacks && num > availableCounts.GetCount(j)) || !ingredientCount.filter.Allows(availableCounts.GetDef(j)) || (bill != null && !ingredientCount.IsFixedIngredient && !bill.ingredientFilter.Allows(availableCounts.GetDef(j))))
                    {
                        continue;
                    }

                    for (int k = 0; k < availableThings.Count; k++)
                    {
                        if (availableThings[k].def != availableCounts.GetDef(j))
                        {
                            continue;
                        }

                        int num2 = availableThings[k].stackCount - ThingCountUtility.CountOf(chosen, availableThings[k]);
                        if (num2 > 0)
                        {
                            if (bill != null && bill.recipe.ignoreIngredientCountTakeEntireStacks)
                            {
                                ThingCountUtility.AddToList(chosen, availableThings[k], num2);
                                return true;
                            }

                            int num3 = Mathf.Min(Mathf.FloorToInt(num), num2);
                            ThingCountUtility.AddToList(chosen, availableThings[k], num3);
                            num -= (float)num3;
                            if (num < 0.001f)
                            {
                                flag = true;
                                float count = availableCounts.GetCount(j);
                                count -= num;
                                availableCounts.SetCount(j, count);
                                break;
                            }
                        }
                    }

                    if (flag)
                    {
                        break;
                    }
                }

                if (!flag)
                {
                    if (missingIngredients == null)
                    {
                        return false;
                    }

                    missingIngredients.Add(ingredientCount);
                }
            }

            if (missingIngredients != null)
            {
                return missingIngredients.Count == 0;
            }

            return true;
        }

        private static bool TryFindBestBillIngredientsInSet_AllowMix(List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, List<IngredientCount> missingIngredients)
        {
            chosen.Clear();
            missingIngredients?.Clear();
            availableThings.SortBy((Thing t) => bill.recipe.IngredientValueGetter.ValuePerUnitOf(t.def), (Thing t) => (t.Position - rootCell).LengthHorizontalSquared);
            for (int i = 0; i < bill.recipe.ingredients.Count; i++)
            {
                IngredientCount ingredientCount = bill.recipe.ingredients[i];
                float num = ingredientCount.GetBaseCount();
                for (int j = 0; j < availableThings.Count; j++)
                {
                    Thing thing = availableThings[j];
                    if (ingredientCount.filter.Allows(thing) && (ingredientCount.IsFixedIngredient || bill.ingredientFilter.Allows(thing)))
                    {
                        float num2 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                        int num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                        ThingCountUtility.AddToList(chosen, thing, num3);
                        num -= (float)num3 * num2;
                        if (num <= 0.0001f)
                        {
                            break;
                        }
                    }
                }

                if (num > 0.0001f)
                {
                    if (missingIngredients == null)
                    {
                        return false;
                    }

                    missingIngredients.Add(ingredientCount);
                }
            }

            if (missingIngredients != null)
            {
                return missingIngredients.Count == 0;
            }

            return true;
        }

        private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver, bool forced = false)
        {
            bool flag = FloatMenuMakerMap.makingFor == pawn;

            for (int i = 0; i < giver.BillStack.Count; i++)
            {
                Bill bill = giver.BillStack[i];
                if ((bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType != def.workType) || (Find.TickManager.TicksGame <= bill.nextTickToSearchForIngredients && FloatMenuMakerMap.makingFor != pawn) || !bill.ShouldDoNow() || !bill.PawnAllowedToStartAnew(pawn))
                {
                    continue;
                }

                SkillRequirement skillRequirement = bill.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
                if (skillRequirement != null)
                {
                    JobFailReason.Is("UnderRequiredSkill".Translate(skillRequirement.minLevel), bill.Label);
                    continue;
                }

                Bill_Medical bill_Medical;
                if ((bill_Medical = bill as Bill_Medical) != null)
                {
                    if (bill_Medical.IsSurgeryViolationOnExtraFactionMember(pawn))
                    {
                        JobFailReason.Is("SurgeryViolationFellowFactionMember".Translate());
                        continue;
                    }

                    if (!pawn.CanReserve(bill_Medical.GiverPawn, 1, -1, null, forced))
                    {
                        Pawn pawn2 = pawn.MapHeld.reservationManager.FirstRespectedReserver(bill_Medical.GiverPawn, pawn);
                        JobFailReason.Is("IsReservedBy".Translate(bill_Medical.GiverPawn.LabelShort, pawn2.LabelShort));
                        continue;
                    }
                }

                Bill_Mech bill_Mech;
                if ((bill_Mech = bill as Bill_Mech) != null && bill_Mech.Gestator.WasteProducer.Waste != null && bill_Mech.Gestator.GestatingMech == null)
                {
                    JobFailReason.Is("WasteContainerFull".Translate());
                    continue;
                }

                Bill_ProductionWithUft bill_ProductionWithUft = bill as Bill_ProductionWithUft;
                if (bill_ProductionWithUft != null)
                {
                    if (bill_ProductionWithUft.BoundUft != null)
                    {
                        if (bill_ProductionWithUft.BoundWorker == pawn && pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly) && !bill_ProductionWithUft.BoundUft.IsForbidden(pawn))
                        {
                            return FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
                        }

                        continue;
                    }

                    UnfinishedThing unfinishedThing = ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);
                    if (unfinishedThing != null)
                    {
                        return FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
                    }
                }

                Bill_Autonomous bill_Autonomous;
                if ((bill_Autonomous = bill as Bill_Autonomous) != null && bill_Autonomous.State != 0)
                {
                    return WorkOnFormedBill((Thing)giver, bill_Autonomous);
                }

                List<IngredientCount> list = null;
                if (flag)
                {
                    list = missingIngredients;
                    list.Clear();
                    tmpMissingUniqueIngredients.Clear();
                }

                Bill_Medical bill_Medical2 = bill as Bill_Medical;
                if (bill_Medical2 != null && bill_Medical2.uniqueRequiredIngredients?.NullOrEmpty() == false)
                {
                    foreach (Thing uniqueRequiredIngredient in bill_Medical2.uniqueRequiredIngredients)
                    {
                        if (uniqueRequiredIngredient.IsForbidden(pawn) || !pawn.CanReserveAndReach(uniqueRequiredIngredient, PathEndMode.Touch, Danger.Deadly))
                        {
                            tmpMissingUniqueIngredients.Add(uniqueRequiredIngredient);
                        }
                    }
                }

                if (!TryFindBestBillIngredients(bill, pawn, (Thing)giver, chosenIngThings, list) || !tmpMissingUniqueIngredients.NullOrEmpty())
                {
                    if (FloatMenuMakerMap.makingFor != pawn)
                    {
                        bill.nextTickToSearchForIngredients = Find.TickManager.TicksGame + ReCheckFailedBillTicksRange.RandomInRange;
                    }
                    else if (flag)
                    {
                        if (CannotDoBillDueToMedicineRestriction(pawn, bill, list))
                        {
                            JobFailReason.Is("NoMedicineMatchingCategory".Translate(GetMedicalCareCategory((Thing)giver).GetLabel().Named("CATEGORY")), bill.Label);
                        }
                        else
                        {
                            string text = list.Select((IngredientCount missing) => missing.Summary).Concat(tmpMissingUniqueIngredients.Select((Thing t) => t.Label)).ToCommaList();
                            JobFailReason.Is("MissingMaterials".Translate(text), bill.Label);
                        }

                        flag = false;
                    }

                    chosenIngThings.Clear();
                    continue;
                }

                flag = false;
                if (bill_Medical2 != null && bill_Medical2.uniqueRequiredIngredients?.NullOrEmpty() == false)
                {
                    foreach (Thing uniqueRequiredIngredient2 in bill_Medical2.uniqueRequiredIngredients)
                    {
                        chosenIngThings.Add(new ThingCount(uniqueRequiredIngredient2, 1));
                    }
                }

                Job haulOffJob;
                Job result = TryStartNewDoBillJob0(pawn, bill, giver, chosenIngThings, out haulOffJob);
                chosenIngThings.Clear();
                return result;
            }

            chosenIngThings.Clear();
            return null;
        }
    }
}
