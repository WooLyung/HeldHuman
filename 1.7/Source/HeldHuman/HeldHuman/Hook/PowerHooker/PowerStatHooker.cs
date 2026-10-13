using RimWorld;
using Verse;

namespace HeldHuman.Hook
{
    public class PowerStatHooker : PowerHooker
    {
        public override int Priority => 0;

        public override void Modify(Pawn pawn, ref float value)
        {
            value *= pawn.GetStatValue(Def.StatDefOf.BioelectricCharge);
        }
    }
}
