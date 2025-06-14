using HarmonyLib;
using HeldHuman.ToilHelper;
using HeldHuman.Tool;
using RimWorld;
using System.Collections.Generic;
using Unity.Jobs;
using Verse;
using Verse.AI;

namespace HeldHuman.Patch.JobDriver_CarryToBuilding_
{
    [HarmonyPatch(typeof(JobDriver_CarryToBuilding), "MakeNewToils")]
    public class MakeNewToils_Patch
    {
        private const TargetIndex BuildingInd = TargetIndex.A;
        private const TargetIndex TakeeInd = TargetIndex.B;

        static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_CarryToBuilding __instance)
        {
            Job job = __instance.job;
            Pawn takee = (Pawn)job.GetTarget(TakeeInd).Thing;
            Building_Enterable building = (Building_Enterable)job.GetTarget(TargetIndex.A).Thing;

            if (!HumanTool.IsHoldableHuman(takee) || !takee.IsOnHoldingPlatform)
            {
                foreach (var toil in __result)
                    yield return toil;
                yield break;
            }

            Building_HoldingPlatform platform = (Building_HoldingPlatform)takee.ParentHolder;

            __instance.ClearToils();
            __instance.globalFailConditions.Clear();

            __instance.FailOnDestroyedOrNull(TakeeInd);
            __instance.FailOnDespawnedNullOrForbidden(BuildingInd);
            __instance.FailOn(() => takee != null && !building.CanAcceptPawn(takee));

            yield return Toils_General.Do(delegate
            {
                building.SelectedPawn = takee;
            });
            yield return Toils.GotoThing(platform, PathEndMode.Touch);

            Toil release = ToilMaker.MakeToil("ReleaseHeldHumanFromPlatform");
            release.initAction = delegate
            {
                platform.EjectContents();
            };
            release.AddFailCondition(delegate
            {
                return platform != takee.ParentHolder;
            });
            yield return release;

            yield return Toils_Haul.StartCarryThing(TakeeInd);
            yield return Toils_Goto.GotoThing(BuildingInd, PathEndMode.InteractionCell);
            yield return Toils_General.WaitWith(BuildingInd, 60, useProgressBar: true);
            yield return Toils_General.Do(delegate
            {
                building.TryAcceptPawn(takee);
            });
        }
    }
}