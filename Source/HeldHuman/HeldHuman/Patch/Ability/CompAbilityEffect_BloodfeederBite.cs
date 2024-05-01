using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace HeldHuman.Patch.CompAbilityEffect_BloodfeederBite_
{
    [HarmonyPatch(typeof(CompAbilityEffect_BloodfeederBite), "Valid")]
    [HarmonyPatch(new Type[] { typeof(LocalTargetInfo), typeof(bool) })]
    public class Valid_Patch
    {
        static bool Prefix(ref CompAbilityEffect_BloodfeederBite __instance, ref bool __result, LocalTargetInfo target, bool throwMessages)
        {
            return true;
        }
    }
}
