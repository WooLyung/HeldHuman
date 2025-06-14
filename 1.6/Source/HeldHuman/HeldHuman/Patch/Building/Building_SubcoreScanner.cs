using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace HeldHuman.Patch.Building_SubcoreScanner_
{
    [HarmonyPatch(typeof(Building_SubcoreScanner), "GetGizmos")]
    public class GetGizmos_Patch
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_SubcoreScanner __instance)
        {
            if (!ModsConfig.BiotechActive)
                yield break;

            foreach (var gizmo in __result)
            {
                if (gizmo is Command_Action cmdAction && cmdAction.icon == Building_SubcoreScanner.InsertPersonIcon.Texture)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = "InsertPerson".Translate() + "...";
                    command_Action.defaultDesc = "InsertPersonSubcoreScannerDesc".Translate(__instance.def.label);
                    command_Action.icon = Building_SubcoreScanner.InsertPersonIcon.Texture;
                    command_Action.action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        List<Pawn> allPawnsSpawned = new List<Pawn>();
                        foreach (var item in __instance.Map.mapPawns.AllPawnsSpawned)
                            allPawnsSpawned.Add(item);
                        foreach (var item in HumanTool.GetAllHeldHumans(__instance.Map))
                            allPawnsSpawned.Add(item);

                        for (int j = 0; j < allPawnsSpawned.Count; j++)
                        {
                            Pawn pawn = allPawnsSpawned[j];
                            AcceptanceReport acceptanceReport = __instance.CanAcceptPawn(pawn);
                            if (!acceptanceReport.Accepted)
                            {
                                if (!acceptanceReport.Reason.NullOrEmpty())
                                    list.Add(new FloatMenuOption(pawn.LabelShortCap + ": " + acceptanceReport.Reason, null, pawn, Color.white));
                            }
                            else
                            {
                                list.Add(new FloatMenuOption(pawn.LabelShortCap, delegate
                                {
                                    if (__instance.def.building.destroyBrain)
                                    {
                                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmRipscanPawn".Translate(pawn.Named("PAWN")), delegate
                                        {
                                            AccessTools.Method(typeof(Building_SubcoreScanner), "SelectPawn", new Type[] { typeof(Pawn) }).Invoke(__instance, new object[] { pawn });
                                        }, destructive: true));
                                    }
                                    else
                                        AccessTools.Method(typeof(Building_SubcoreScanner), "SelectPawn", new Type[] { typeof(Pawn) }).Invoke(__instance, new object[] { pawn });
                                }, pawn, Color.white));
                            }
                        }

                        if (!list.Any())
                            list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
                        Find.WindowStack.Add(new FloatMenu(list));
                    };
                    if (!__instance.PowerOn)
                    {
                        command_Action.Disable("NoPower".Translate().CapitalizeFirst());
                    }
                    else if (__instance.State == SubcoreScannerState.WaitingForIngredients)
                    {
                        StringBuilder stringBuilder2 = new StringBuilder("SubcoreScannerWaitingForIngredientsDesc".Translate().CapitalizeFirst() + ":\n");
                        AccessTools.Method(typeof(Building_SubcoreScanner), "AppendIngredientsList", new Type[] { typeof(StringBuilder) }).Invoke(__instance, new object[] { stringBuilder2 });
                        command_Action.Disable(stringBuilder2.ToString());
                    }

                    yield return command_Action;
                    continue;
                }
                yield return gizmo;
            }
        }
    }
}
