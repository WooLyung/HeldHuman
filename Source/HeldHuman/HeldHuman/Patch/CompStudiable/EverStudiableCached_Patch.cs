using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace HeldHuman.Patch.CompStudiable_
{
    [HarmonyPatch]
    public class EverStudiableCached_Patch
    {
        static MethodBase TargetMethod() => AccessTools.Method(typeof(CompStudiable), "EverStudiableCached", new[] { typeof(string).MakeByRefType() });

        static bool Prefix(ref CompStudiable __instance, ref bool __result, ref string reason)
        {
            reason = null;

            if (__instance.parent == null || !(__instance.parent is Pawn pawn))
                return true;
            if (!(pawn.ParentHolder is Building_HoldingPlatform))
                return true;
            if (!Tool.HumanTool.IsHoldableHuman(pawn))
                return true;

            __result = true;
            return false;
        }
    }
}