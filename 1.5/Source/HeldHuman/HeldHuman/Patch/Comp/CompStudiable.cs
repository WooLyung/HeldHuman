using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System.Reflection;
using Verse;
using ModSettings = HeldHuman.Setting.ModSettings;

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

        static void Postfix(ref CompStudiable __instance, ref bool __result, ref string reason)
        {
            if (!__result && HumanTool.IsHuman(__instance.parent) && __instance.parent.IsOnHoldingPlatform)
                if (HumanTool.IsCreepJoiner(__instance.parent as Pawn) || ModSettings.Instance.enableStudying)
                    __result = true;
        }
    }
}