﻿ using RimWorld;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Tool
{
    public static class HumanTool
    {
        public static bool IsPawn(Thing thing) => thing != null && thing is Pawn;
        public static bool IsHoldableHuman(Thing thing) => IsPawn(thing) && IsHoldableHuman(thing as Pawn);
        public static bool IsHoldableHuman(Pawn pawn) => pawn != null && pawn.RaceProps.Humanlike && !pawn.IsMutant && !pawn.InAggroMentalState;
        public static bool IsCreepJoiner(Pawn pawn) => pawn != null && pawn.IsCreepJoiner;
        public static bool IsMutantHuman(Pawn pawn) => pawn != null && pawn.RaceProps.Humanlike && pawn.IsMutant;
        public static bool IsHuman(Thing thing) => thing != null && thing is Pawn && ((Pawn)thing).RaceProps.Humanlike;

        public static IEnumerable<Pawn> GetAllHeldHumans(Map map)
        {
            foreach (Thing holder in map.listerThings.ThingsInGroup(ThingRequestGroup.EntityHolder))
            {
                Pawn pawn = PlatformTool.GetHeldPawn(holder);
                if (IsHoldableHuman(pawn))
                    yield return pawn;
            }
        }
    }
}
