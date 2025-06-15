using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using UnityEngine;
using Verse;

namespace HeldHuman.Patch.CompPowerPlantElectroharvester_
{
    [HarmonyPatch(typeof(CompPowerPlantElectroharvester), "GetPowerOutput")]
    public class GetPowerOutput_Patch
    {
        static bool Prefix(ref CompPowerPlantElectroharvester __instance, ref float __result)
        {
            float num = 0f;
            foreach (Thing platform in __instance.Platforms)
            {
                Building_HoldingPlatform building_HoldingPlatform;
                if ((building_HoldingPlatform = platform as Building_HoldingPlatform) != null && building_HoldingPlatform.Occupied)
                {
                    float value = Mathf.RoundToInt(building_HoldingPlatform.HeldPawn.BodySize * (0f - __instance.Props.PowerConsumption) * 0.1f);
                    if (HumanTool.IsHoldableHuman(building_HoldingPlatform.HeldPawn))
                        value *= Setting.ModSettings.Instance.powerFactor * 0.01f;
                    num += value;
                }
            }

            __result = Mathf.Clamp(num, 0f, 0f - __instance.Props.PowerConsumption);
            return false;
        }
    }
}
