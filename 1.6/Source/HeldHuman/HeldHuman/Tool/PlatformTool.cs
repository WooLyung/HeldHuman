using RimWorld;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Tool
{
    public static class PlatformTool
    {
        public static Pawn GetHeldPawn(Thing holder) => (holder as ThingWithComps)?.GetComp<CompEntityHolderPlatform>()?.HeldPawn;

        public static IEnumerable<Building_HoldingPlatform> GetAllPlatforms(Map map)
        {
            foreach (Thing holder in map.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
                if (holder is Building_HoldingPlatform holder0)
                    yield return holder0;
        }

        public static IEnumerable<Building_HoldingPlatform> GetAllInHumanPlatforms(Map map)
        {
            foreach (Thing holder in map.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
                if (holder is Building_HoldingPlatform holder0 && HumanTool.IsHoldableHuman(GetHeldPawn(holder0)))
                    yield return holder0;
        }
    }
}
