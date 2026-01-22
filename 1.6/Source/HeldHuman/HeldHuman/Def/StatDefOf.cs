using RimWorld;
using Verse;

namespace HeldHuman.Def
{
    [DefOf]
    public static class StatDefOf
    {
        public static StatDef EscapeIntervalFactor;
        public static StatDef BioferriteDensity;
        public static StatDef BioelectricCharge;

        static StatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf));
        }
    }
}
