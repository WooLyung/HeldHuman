using RimWorld;
using Verse;

namespace HeldHuman.Def
{
    [RimWorld.DefOf]
    public static class GeneDefOf
    {
        public static GeneDef FaintMind;
        public static GeneDef VoidMind;
        public static GeneDef BioBattery;
        public static GeneDef LargeBioBattery;
        public static GeneDef BioferriteTissue;

        static GeneDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RimWorld.GeneDefOf));
        }
    }
}