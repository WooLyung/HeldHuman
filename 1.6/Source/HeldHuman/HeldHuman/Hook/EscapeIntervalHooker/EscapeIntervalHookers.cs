using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Hook
{
    public class EscapeIntervalHookers
    {
        private static List<EscapeIntervalHooker> hookers = new List<EscapeIntervalHooker>();

        static EscapeIntervalHookers()
        {
            foreach (Type type in typeof(EscapeIntervalHooker).AllSubclasses())
                hookers.Add((EscapeIntervalHooker)Activator.CreateInstance(type));
            hookers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public static IEnumerable<EscapeIntervalHooker> GetHookers()
        {
            return hookers;
        }
    }
}
