using HarmonyLib;
using System.Reflection;
using Verse;

namespace HeldHuman.Core
{
    [StaticConstructorOnStartup]
    public class HeldHumanMod
    {
        public static Harmony Harmony { get; private set; }

        static HeldHumanMod()
        {
            Harmony = new Harmony("ng.lyu.heldhuman");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}