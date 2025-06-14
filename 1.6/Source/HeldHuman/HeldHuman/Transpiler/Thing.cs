using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;
using System;

namespace HeldHuman.Transpiler.Thing_
{
    [HarmonyPatch(typeof(Thing), "get_Map")]
    public static class Map_Transpiler_
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            Label label = il.DefineLabel();
            codes[0].labels.Add(label);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HumanTool), "IsHoldableHuman", new Type[] { typeof(Thing) }));
            yield return new CodeInstruction(OpCodes.Brfalse_S, label);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thing), "get_IsOnHoldingPlatform"));
            yield return new CodeInstruction(OpCodes.Brfalse_S, label);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thing), "get_MapHeld"));
            yield return new CodeInstruction(OpCodes.Ret);

            for (int i = 0; i < codes.Count; i++)
                yield return codes[i];
        }
    }

    [HarmonyPatch(typeof(Thing), "get_Position")]
    public static class Position_Transpiler_
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            Label label = il.DefineLabel();
            codes[0].labels.Add(label);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HumanTool), "IsHoldableHuman", new Type[] { typeof(Thing) }));
            yield return new CodeInstruction(OpCodes.Brfalse_S, label);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thing), "get_IsOnHoldingPlatform"));
            yield return new CodeInstruction(OpCodes.Brfalse_S, label);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thing), "get_PositionHeld"));
            yield return new CodeInstruction(OpCodes.Ret);

            for (int i = 0; i < codes.Count; i++)
                yield return codes[i];
        }
    }

    [HarmonyPatch(typeof(Thing), "get_Rotation")]
    public static class Rotation_Transpiler_
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            Label label = il.DefineLabel();
            codes[0].labels.Add(label);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HumanTool), "IsHoldableHuman", new Type[] { typeof(Thing) }));
            yield return new CodeInstruction(OpCodes.Brfalse_S, label);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Thing), "get_IsOnHoldingPlatform"));
            yield return new CodeInstruction(OpCodes.Brfalse_S, label);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Rotation_Transpiler_), "GetRot"));
            yield return new CodeInstruction(OpCodes.Ret);

            for (int i = 0; i < codes.Count; i++)
                yield return codes[i];
        }

        static Rot4 GetRot(Thing thing) => ((Building_HoldingPlatform)thing.ParentHolder).Rotation;
    }
}