using HarmonyLib;
using System.Reflection;
using Verse;

namespace HeldHuman
{
    [StaticConstructorOnStartup]
    public class HeldHumanMod
    {
        private static Harmony harmony;

        static HeldHumanMod()
        {
            harmony = new Harmony("ng.lyu.heldhuman");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}