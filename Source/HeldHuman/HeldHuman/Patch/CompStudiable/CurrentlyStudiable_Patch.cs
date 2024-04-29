using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.CompStudiable_
{
    [HarmonyPatch(typeof(CompStudiable), "CurrentlyStudiable")]
    public class CurrentlyStudiable_Patch
    {
        static bool Prefix(ref CompStudiable __instance, ref bool __result)
        {
            if (!(__instance.parent is Pawn parent))
                return true;
            if (!__instance.EverStudiable(out string _) || !__instance.studyEnabled || __instance.Props.frequencyTicks > 0 && __instance.TicksTilNextStudy > 0)
                return true;
            if (!HumanTool.IsHoldableHuman(parent))
                return true;

            CompHoldingPlatformTarget comp = parent.TryGetComp<CompHoldingPlatformTarget>();
            if (comp == null || !comp.CanStudy)
                return true;
            if ((float)AccessTools.Field(typeof(CompProperties_Studiable), "anomalyKnowledge").GetValue(__instance.Props) <= 0.0f)
                return true;

            __result = true;
            return false;
        }
    }
}
