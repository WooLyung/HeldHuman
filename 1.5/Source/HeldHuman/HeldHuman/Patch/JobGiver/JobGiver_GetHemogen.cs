using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using System.Linq;
using Verse;
using Verse.AI;

namespace HeldHuman.Patch.JobGiver_GetHemogen_
{
    [HarmonyPatch(typeof(JobGiver_GetHemogen), "TryGiveJob")]
    [HarmonyPatch(new Type[] { typeof(Pawn) })]
    public class TryGiveJob_Patch
    {
        private static Building_HoldingPlatform GetTarget(Pawn pawn)
        {
            Building_HoldingPlatform platform = (Building_HoldingPlatform)GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, PlatformTool.GetAllInHumanPlatforms(pawn.Map), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, 
                (Thing t) => (platform = t as Building_HoldingPlatform) != null && HumanTool.IsHoldableHuman(PlatformTool.GetHeldPawn(platform)) && JobGiver_GetHemogen.CanFeedOnPrisoner(pawn, PlatformTool.GetHeldPawn(platform)).Accepted);
            return platform;
        }

        static bool Prefix(ref JobGiver_GetHemogen __instance, ref Job __result, Pawn pawn)
        {
            if (!ModsConfig.BiotechActive)
                return true;
            Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
            if (gene_Hemogen == null)
                return true;
            if (!gene_Hemogen.ShouldConsumeHemogenNow())
                return true;
            if (pawn.IsBloodfeeder())
            {
                Building_HoldingPlatform target = GetTarget(pawn);
                if (target != null)
                {
                    __result = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("HeldHumanBloodfeed"), target);
                    return false;
                }
            }

            return true;
        }
    }
}
