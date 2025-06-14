using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace HeldHuman.Patch.HumanEmbryo_
{
    [HarmonyPatch]
    public class GetGizmos_Patch
    {
        static MethodBase TargetMethod() => AccessTools.Method(typeof(HumanEmbryo), "GetGizmos");

        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, HumanEmbryo __instance)
        {
            if (!ModsConfig.BiotechActive)
                yield break;

            foreach (var gizmo in __result)
            {
                if (gizmo is Command_Action cmdAction && cmdAction.icon == HumanEmbryo.ImplantIcon.Texture)
                {
                    List<FloatMenuOption> surrogateOptions = new List<FloatMenuOption>();

                    foreach (Pawn item in __instance.MapHeld.mapPawns.FreeColonistsAndPrisonersSpawned)
                    {
                        FloatMenuOption floatMenuOption = (FloatMenuOption)AccessTools.Method(typeof(HumanEmbryo), "CanImplantFloatOption", new Type[] { typeof(Pawn), typeof(bool) }).Invoke(__instance, new object[] { item, true });
                        if (floatMenuOption != null)
                            surrogateOptions.Add(floatMenuOption);
                    }
                    foreach (Pawn item in HumanTool.GetAllHeldHumans(__instance.MapHeld))
                    {
                        FloatMenuOption floatMenuOption = (FloatMenuOption)AccessTools.Method(typeof(HumanEmbryo), "CanImplantFloatOption", new Type[] { typeof(Pawn), typeof(bool) }).Invoke(__instance, new object[] { item, true });
                        if (floatMenuOption != null)
                            surrogateOptions.Add(floatMenuOption);
                    }

                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "ImplantLabel".Translate() + "...",
                        defaultDesc = "ImplantDescription".Translate(),
                        icon = HumanEmbryo.ImplantIcon.Texture,
                        action = delegate
                        {
                            Find.WindowStack.Add(new FloatMenu(surrogateOptions));
                        }
                    };

                    if (surrogateOptions.Count == 0)
                        command_Action.Disable("ImplantDisabledNoWomen".Translate());

                    yield return command_Action;
                    continue;
                }
                yield return gizmo;
            }
        }
    }
}
