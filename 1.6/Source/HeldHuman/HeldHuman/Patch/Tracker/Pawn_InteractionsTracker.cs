using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using Verse;

namespace HeldHuman.Patch.Pawn_InteractionsTracker_
{
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "CanInteractNowWith")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(InteractionDef) })]
    public class CanInteractNowWith_Patch
    {
        static bool Prefix(ref Pawn_InteractionsTracker __instance, ref bool __result, Pawn recipient, InteractionDef interactionDef = null)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_InteractionsTracker), "pawn").GetValue(__instance);
            if (!HumanTool.IsHoldableHuman(recipient) || !recipient.IsOnHoldingPlatform)
                return true;

            if (__instance.InteractedTooRecentlyToInteract())
                __result = false;
            else if (!SocialInteractionUtility.CanInitiateInteraction(pawn, interactionDef) || !SocialInteractionUtility.CanReceiveInteraction(recipient, interactionDef))
                __result = false;
            else
                __result = true;

            return false;
        }
    }
}
