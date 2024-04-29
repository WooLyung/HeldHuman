using RimWorld;
using System.Collections;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Tool
{
    public static class HumanTool
    {
        public static bool IsHoldableHuman(Pawn pawn)
        {
            if (!pawn.RaceProps.Humanlike)
                return false;
            if (pawn.Faction.HostileTo(Faction.OfPlayer) || pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony)
                return true;
            return false;
        }

        public static IEnumerable<Pawn> GetAllHeldHumans(Map map)
        {
            foreach (Thing holder in map.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
            {
                if (!(holder is ThingWithComps tHolder))
                    continue;

                CompEntityHolderPlatform comp = tHolder.GetComp<CompEntityHolderPlatform>();
                if (comp == null)
                    continue;

                Pawn pawn = comp.HeldPawn;
                if (pawn == null)
                    continue;

                if (IsHoldableHuman(pawn))
                    yield return pawn;
            }
        }
    }
}
