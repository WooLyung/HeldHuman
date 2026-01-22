using HeldHuman.Def;
using RimWorld;
using System.Text;
using Verse;

namespace HeldHuman.Hook
{
    public class BioferriteDensityStatHooker : BioferriteDensityHooker
    {
        public override int Priority => 0;

        private float GetOffset(Pawn pawn)
        {
            return pawn.GetStatValue(Def.StatDefOf.BioferriteDensity);
        }

        public override void Modify(Pawn pawn, ref float value)
        {
            value += GetOffset(pawn);
        }

        public override void AddStatDraw(Pawn pawn, ref StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("heldHumanStatDraw.BioferriteDensity".Translate() + ": " + GetOffset(pawn).ToString("+0;-#"));
        }
    }
}
