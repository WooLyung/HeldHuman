using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.Need_
{
    [HarmonyPatch(typeof(Need), "get_ShowOnNeedList")]
    public class get_ShowOnNeedList_Patch
    {
        static void Postfix(ref Need __instance, ref bool __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Need), "pawn").GetValue(__instance);
            if (!HumanTool.IsHoldableHuman(pawn) || !pawn.IsOnHoldingPlatform)
                return;

            if (__instance is Need_Food && !Setting.ModSettings.Instance.enableFood)
                __result = false;
            if (__instance is Need_Beauty || __instance is Need_Comfort)
                __result = false;
        }
    }
}
