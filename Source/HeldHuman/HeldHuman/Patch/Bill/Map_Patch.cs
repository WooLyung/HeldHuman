using HarmonyLib;
using RimWorld;
using Verse;

namespace HeldHuman.Patch.Bill_
{
    [HarmonyPatch(typeof(Bill), "get_Map")]
    public class Map_Patch
    {
        static bool Prefix(ref Bill __instance, ref Map __result)
        {
            Pawn giver = __instance?.billStack?.billGiver as Pawn;
            if (giver != null && giver.IsOnHoldingPlatform)
            {
                __result = giver.MapHeld;
                return false;
            }

            return true;
        }
    }
}
