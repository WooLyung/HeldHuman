using System;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Hook
{
    public class PowerHookers
    {
        private static List<PowerHooker> hookers = new List<PowerHooker>();

        static PowerHookers()
        {
            foreach (Type type in typeof(PowerHooker).AllSubclasses())
                hookers.Add((PowerHooker)Activator.CreateInstance(type));
            hookers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        public static IEnumerable<PowerHooker> GetHookers()
        {
            return hookers;
        }
    }
}
