using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using RimWorld.Planet;
using System;
using Verse;

namespace HeldHuman.Patch.CompHoldingPlatformTarget_
{
    [HarmonyPatch(typeof(CompHoldingPlatformTarget), "Notify_HeldOnPlatform")]
    [HarmonyPatch(new Type[] { typeof(ThingOwner) })]
    public class Notify_HeldOnPlatform
    {
        static void Postfix(ref CompHoldingPlatformTarget __instance, ThingOwner newOwner)
        {
            Thing thing = newOwner.GetLast();
            if (!(thing is Pawn pawn))
                return;

            if (HumanTool.IsHoldableHuman(pawn))
                pawn.guest.CapturedBy(Faction.OfPlayer);
        }
    }
}