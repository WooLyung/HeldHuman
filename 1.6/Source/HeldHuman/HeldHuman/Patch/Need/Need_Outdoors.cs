using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.Need_Outdoors_
{
    [HarmonyPatch(typeof(Need_Outdoors), "get_ShowOnNeedList")]
    public class get_ShowOnNeedList_Patch
    {
        static void Postfix(ref Need_Outdoors __instance, ref bool __result)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Need_Outdoors), "pawn").GetValue(__instance);
            if (!HumanTool.IsHoldableHuman(pawn) || !pawn.IsOnHoldingPlatform)
                return;

            __result = false;
        }
    }
}
