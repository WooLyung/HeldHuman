using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System.Linq;
using Verse;

namespace HeldHuman.Patch.Building_HoldingPlatform_
{
    [HarmonyPatch(typeof(Building_HoldingPlatform), "get_TargetPawnAttachPoints")]
    public class TargetPawnAttachPoints_Patch
    {
        static void Postfix(ref Building_HoldingPlatform __instance, ref AttachPointTracker __result)
        {
            Pawn pawn = __instance.HeldPawn;
            if (pawn.HasComp<CompAttachPoints>())
                return;
            if (pawn.health.hediffSet.GetHediffComps<HediffComp_AttachPoints>().ToList().Count > 0)
                return;

            AttachPointTracker points = null;
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated && pawn.story?.bodyType?.attachPointsDessicated != null)
                points = new AttachPointTracker(pawn.story.bodyType.attachPointsDessicated, pawn);
            else if (pawn.story?.bodyType?.attachPoints != null)
                points = new AttachPointTracker(pawn.story.bodyType.attachPoints, pawn);

            if (points != null)
            {
                AccessTools.Field(typeof(Building_HoldingPlatform), "targetPoints").SetValue(__instance, points);
                __result = points;
            }
        }
    }
}
