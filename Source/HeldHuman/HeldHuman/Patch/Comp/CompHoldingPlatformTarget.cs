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
            if (!HumanTool.IsHoldableFaction(__instance.parent))
                return true;
            Pawn pawn = (Pawn)__instance.parent;

            if (ModsConfig.IdeologyActive && pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony)
            {
                __result = true;
                return false;
            }
            else if (pawn.Faction.HostileTo(Faction.OfPlayer))
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
        static bool Prefix(ref CompHoldingPlatformTarget __instance, ref bool __result)
        {
            if (HumanTool.IsHoldableHuman(__instance.parent))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}