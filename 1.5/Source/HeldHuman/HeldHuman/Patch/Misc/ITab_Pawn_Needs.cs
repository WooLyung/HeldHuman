using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.ITab_Pawn_Needs_
{
    [HarmonyPatch(typeof(ITab_Pawn_Needs), "get_IsVisible")]
    public class IsVisible_Patch
    {
        static void Postfix(ref ITab_Pawn_Needs __instance, ref bool __result)
        {
            if (!__result)
            {
                Pawn pawn = (Pawn)AccessTools.Method(typeof(ITab), "get_SelPawn").Invoke(__instance, null);
                if (HumanTool.IsHoldableHuman(pawn) && pawn.IsOnHoldingPlatform && pawn.needs.AllNeeds.Count > 0)
                    __result = true;
            }
        }
    }
}
