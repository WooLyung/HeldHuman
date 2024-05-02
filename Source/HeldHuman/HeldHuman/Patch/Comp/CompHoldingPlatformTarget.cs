using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using Verse;

namespace HeldHuman.Patch.CompHoldingPlatformTarget_
{
    [HarmonyPatch(typeof(CompHoldingPlatformTarget), "get_CanBeCaptured")]
    public class CanBeCaptured_Patch
    {
        static bool Prefix(ref CompHoldingPlatformTarget __instance, ref bool __result)
        {
            if (!__instance.parent.def.race.Humanlike)
                return true;

            Pawn pawn = (Pawn)__instance.parent;
            if (pawn.IsOnHoldingPlatform)
            {
                __result = true;
                return false;
            }
            if (!pawn.Spawned)
                return true;
            if (!HumanTool.IsHoldableHuman(pawn))
                return true;

            if (ModsConfig.IdeologyActive && pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony)
            {
                __result = true;
                return false;
            }
            else if (pawn.HostileTo(Faction.OfPlayer))
            {
                if (pawn.Downed)
                    __result = pawn.GetComp<CompActivity>()?.IsDormant ?? true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CompHoldingPlatformTarget), "Notify_HeldOnPlatform")]
    [HarmonyPatch(new Type[] { typeof(ThingOwner) })]
    public class Notify_HeldOnPlatform
    {
        static void Postfix(ref CompHoldingPlatformTarget __instance, ThingOwner newOwner)
        {
            Thing thing = newOwner.GetLast();
            if (HumanTool.IsHoldableHuman(thing))
            {
                Pawn pawn = (Pawn)thing;
                pawn.guest.CapturedBy(Faction.OfPlayer);
            }
        }
    }

    [HarmonyPatch(typeof(CompHoldingPlatformTarget), "get_StudiedAtHoldingPlatform")]
    public class StudiedAtHoldingPlatform_Patch
    {
        static void Postfix(ref CompHoldingPlatformTarget __instance, ref bool __result)
        {
            if (!__result && HumanTool.IsHoldableHuman(__instance.parent))
                __result = true;
        }
    }

    [HarmonyPatch(typeof(CompHoldingPlatformTarget), "Escape")]
    [HarmonyPatch(new Type[] { typeof(bool) })]
    public class Escape_Patch
    {
        static void Postfix(ref CompHoldingPlatformTarget __instance, bool initiator)
        {
            if (!initiator)
                return;

            if (HumanTool.IsHoldableHuman(__instance.parent))
            {
                Pawn pawn = (Pawn)__instance.parent;
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, forced: true);
            }
        }
    }
}