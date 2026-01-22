using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Hook
{
    public class BioferriteDensityHookers
    {
        private static List<BioferriteDensityHooker> hookers = new List<BioferriteDensityHooker>();

        static BioferriteDensityHookers()
        {
            foreach (Type type in typeof(BioferriteDensityHooker).AllSubclasses())
                hookers.Add((BioferriteDensityHooker)Activator.CreateInstance(type));
            hookers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public static IEnumerable<BioferriteDensityHooker> GetHookers()
        {
            return hookers;
        }
    }
}
