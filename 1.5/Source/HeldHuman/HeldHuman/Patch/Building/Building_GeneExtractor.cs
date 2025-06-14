using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HeldHuman.Patch.Building_HoldingPlatform_
{
    [HarmonyPatch(typeof(Building_GeneExtractor), "GetGizmos")]
    public class GetGizmos_Patch
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Building_GeneExtractor __instance)
        {
            if (!ModsConfig.BiotechActive)
                yield break;

            foreach (var gizmo in __result)
            {
                if (gizmo is Command_Action cmdAction && cmdAction.icon == __instance.InsertPawnTex)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = "InsertPerson".Translate() + "...";
                    command_Action.defaultDesc = "InsertPersonGeneExtractorDesc".Translate();
                    command_Action.icon = __instance.InsertPawnTex;
                    command_Action.action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        List<Pawn> pawns = new List<Pawn>();

                        foreach (Pawn item in __instance.Map.mapPawns.AllPawnsSpawned)
                            pawns.Add(item);
                        foreach (Pawn item in HumanTool.GetAllHeldHumans(__instance.Map))
                            pawns.Add(item);

                        foreach (Pawn item in pawns)
                        {
                            Pawn pawn = item;
                            if (pawn.genes != null)
                            {
                                AcceptanceReport acceptanceReport = __instance.CanAcceptPawn(pawn);
                                string text = pawn.LabelShortCap + ", " + pawn.genes.XenotypeLabelCap;
                                if (!acceptanceReport.Accepted)
                                {
                                    if (!acceptanceReport.Reason.NullOrEmpty())
                                    {
                                        list.Add(new FloatMenuOption(text + ": " + acceptanceReport.Reason, null, pawn, Color.white));
                                    }
                                }
                                else
                                {
                                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.XenogermReplicating);
                                    if (firstHediffOfDef != null)
                                    {
                                        text = text + " (" + firstHediffOfDef.LabelBase + ", " + firstHediffOfDef.TryGetComp<HediffComp_Disappears>().ticksToDisappear.ToStringTicksToPeriod(allowSeconds: true, shortForm: true).Colorize(ColoredText.SubtleGrayColor) + ")";
                                    }

                                    list.Add(new FloatMenuOption(text, delegate
                                    {
                                        AccessTools.Method(typeof(Building_GeneExtractor), "SelectPawn", new Type[] { typeof(Pawn) }).Invoke(__instance, new object[] { pawn });
                                    }, pawn, Color.white));
                                }
                            }
                        }

                        if (!list.Any())
                            list.Add(new FloatMenuOption("NoExtractablePawns".Translate(), null));
                        Find.WindowStack.Add(new FloatMenu(list));
                    };

                    yield return command_Action;
                    continue;
                }
                yield return gizmo;
            }
        }
    }
}
