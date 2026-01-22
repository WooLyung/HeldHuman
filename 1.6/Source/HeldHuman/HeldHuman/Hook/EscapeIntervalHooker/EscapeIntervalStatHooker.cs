using HeldHuman.Def;
using RimWorld;
using System;
using System.Text;
using Verse;

namespace HeldHuman.Hook
{
    public class EscapeIntervalStatHooker : EscapeIntervalHooker
    {
        public override int Priority => 0;

        private float GetFactor(Pawn pawn)
        {
            return pawn.GetStatValue(Def.StatDefOf.EscapeIntervalFactor);
        }

        public override void Modify(Pawn pawn, ref float value)
        {
            value *= GetFactor(pawn);
        }

        public override void AddStatDraw(Pawn pawn, ref StringBuilder stringBuilder)
        {
            stringBuilder.AppendLineIfNotEmpty();
            stringBuilder.Append("  - " + "heldHumanStatDraw.EscapeIntervalFactor".Translate() + ": x" + GetFactor(pawn).ToStringPercent());
        }
    }
}
