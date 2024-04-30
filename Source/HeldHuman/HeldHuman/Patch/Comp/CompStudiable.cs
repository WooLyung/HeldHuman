using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System.Linq;
using System.Reflection;
using Verse;
using static HarmonyLib.Code;

namespace HeldHuman.Patch.CompStudiable_
{
    [HarmonyPatch(typeof(CompStudiable), "CurrentlyStudiable")]
    public class CurrentlyStudiable_Patch
    {
        static bool Prefix(ref CompStudiable __instance, ref bool __result)
        {
            if (!HumanTool.IsHoldableHuman(__instance.parent))
                return true;

            Pawn pawn = (Pawn)__instance.parent;
            if (!__instance.EverStudiable(out string _) || !__instance.studyEnabled || __instance.Props.frequencyTicks > 0 && __instance.TicksTilNextStudy > 0)
                return true;

            CompHoldingPlatformTarget comp = pawn.TryGetComp<CompHoldingPlatformTarget>();
            if (comp == null || !comp.CanStudy)
                return true;

            float? anomalyKnowledge = (float?)AccessTools.Field(typeof(CompProperties_Studiable), "anomalyKnowledge").GetValue(__instance.Props);
            if (anomalyKnowledge != null && anomalyKnowledge <= 0.0f)
                return true;

            __result = true;
            return false;
        }
    }

    [HarmonyPatch]
    public class EverStudiableCached_Patch
    {
        static MethodBase TargetMethod() => AccessTools.Method(typeof(CompStudiable), "EverStudiableCached", new[] { typeof(string).MakeByRefType() });

        static bool Prefix(ref CompStudiable __instance, ref bool __result, ref string reason)
        {
            reason = null;
            if (!HumanTool.IsHoldableHuman(__instance.parent))
                return true;

            Pawn pawn = (Pawn)__instance.parent;
            if (!pawn.IsOnHoldingPlatform)
                return true;

            __result = true;
            return false;
        }
    }
}