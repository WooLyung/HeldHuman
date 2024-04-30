using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace HeldHuman.Patch.HumanEmbryo_
{
    public class GetGizmos_Patch
    {
        static MethodBase TargetMethod() => AccessTools.Method(typeof(HumanEmbryo), "GetGizmos");

        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, HumanEmbryo __instance)
        {
            foreach (var gizmo in __result)
                yield return gizmo;

            if (__instance.implantTarget == null)
            {
                List<FloatMenuOption> surrogateOptions = new List<FloatMenuOption>();
                foreach (Pawn item in Tool.HumanTool.GetAllHeldHumans(__instance.MapHeld))
                {
                    FloatMenuOption floatMenuOption = (FloatMenuOption) AccessTools.Method(typeof(HumanEmbryo), "CanImplantFloatOption", new Type[] { typeof(Pawn), typeof(bool) }).Invoke(__instance, new object[] { item, true });
                    if (floatMenuOption != null)
                        surrogateOptions.Add(floatMenuOption);
                }

                Command_Action command_Action0 = new Command_Action
                {
                    defaultLabel = "ImplantLabel".Translate() + "....",
                    defaultDesc = "ImplantDescription".Translate(),
                    icon = HumanEmbryo.ImplantIcon.Texture,
                    action = delegate
                    {
                        Find.WindowStack.Add(new FloatMenu(surrogateOptions));
                    }
                };
                if (surrogateOptions.Count != 0)
                    yield return command_Action0;
            }
        }
    }
}
