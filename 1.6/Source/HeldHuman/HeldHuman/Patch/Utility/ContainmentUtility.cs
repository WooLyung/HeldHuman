using HarmonyLib;
using HeldHuman.Hook;
using HeldHuman.Tool;
using RimWorld;
using System.Text;
using Verse;

namespace HeldHuman.Patch.ContainmentUtility_
{
    [HarmonyPatch(typeof(ContainmentUtility), "InitiateEscapeMtbDays")]
    public class InitiateEscapeMtbDays_Patch
    {
        static void Postfix(ref float __result, Pawn pawn, StringBuilder sb)
        {
            foreach (var hooker in EscapeIntervalHookers.GetHookers())
            {
                hooker.Modify(pawn, ref __result);
                if (sb != null)
                    hooker.AddStatDraw(pawn, ref sb);
            }
        }
    }
}
