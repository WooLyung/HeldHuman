using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Hook
{
    public class EscapeHookers
    {
        private static List<EscapeHooker> hookers = new List<EscapeHooker>();

        static EscapeHookers()
        {
            foreach (Type type in typeof(EscapeHooker).AllSubclasses())
                hookers.Add((EscapeHooker)Activator.CreateInstance(type));
            hookers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public static IEnumerable<EscapeHooker> GetHookers()
        {
            return hookers;
        }
    }
}
