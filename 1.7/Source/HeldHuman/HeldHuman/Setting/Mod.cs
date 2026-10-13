 using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace HeldHuman.Setting
{
    [StaticConstructorOnStartup]
    public class Mod : Verse.Mod
    {
        private static ModSettings settings;

        public Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSettings>();
        }

        static Mod()
        {
            LongEventHandler.ExecuteWhenFinished(UpdateSettings);
        }
         
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            Text.Font = GameFont.Medium;
            listingStandard.Label("heldHumanSetting.Label.ProductionStuding".Translate());
            Text.Font = GameFont.Small;
            listingStandard.CheckboxLabeled("heldHumanSetting.Checkbox.EnableBioferrite".Translate(), ref settings.enableProducingBioferrate);
            settings.bioferriteDensity = listingStandard.SliderLabeled("heldHumanSetting.Slider.BioferriteDensity".Translate(settings.bioferriteDensity.ToString("F1")), settings.bioferriteDensity, 0.1f, 10.0f);
            listingStandard.Gap(20);
            listingStandard.CheckboxLabeled("heldHumanSetting.Checkbox.EnableStudying".Translate(), ref settings.enableStudying);
            settings.frequencyTicks = (int)listingStandard.SliderLabeled("heldHumanSetting.Slider.FrequencyTicks".Translate(settings.frequencyTicks), settings.frequencyTicks, 6000, 600000);
            settings.anomalyKnowledge = listingStandard.SliderLabeled("heldHumanSetting.Slider.AnomalyKnowledge".Translate(settings.anomalyKnowledge.ToString("F1")), settings.anomalyKnowledge, 0.1f, 10.0f);
            listingStandard.Gap(20);
            settings.powerFactor = (int)listingStandard.SliderLabeled("heldHumanSetting.Slider.PowerGenerationFactor".Translate(settings.powerFactor), settings.powerFactor, 0, 1000);
            listingStandard.GapLine(40);

            Text.Font = GameFont.Medium;
            listingStandard.Label("heldHumanSetting.Label.NeedsDLC".Translate());
            Text.Font = GameFont.Small;
            listingStandard.CheckboxLabeled("heldHumanSetting.Checkbox.EnableFood".Translate(), ref settings.enableFood);
            listingStandard.GapLine(40);

            if (listingStandard.ButtonText("heldHumanSetting.Button.Reset".Translate()))
                settings.Reset();
            listingStandard.Label("heldHumanSetting.Label.Restart".Translate());
            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Held Human";
        }

        private static void UpdateSettings()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.race == null || !def.race.Humanlike)
                    continue;

                foreach (CompProperties property in def.comps)
                {
                    CompProperties_ProducesBioferrite p = property as CompProperties_ProducesBioferrite;
                    if (p == null)
                        continue;
                    if (settings.enableProducingBioferrate)
                        p.bioferriteDensity = settings.bioferriteDensity;
                    else
                        p.bioferriteDensity = 0;
                }

                if (def == ThingDefOf.CreepJoiner)
                {
                    foreach (CompProperties property in def.comps)
                    {
                        CompProperties_Studiable p = property as CompProperties_Studiable;
                        if (p == null)
                            continue;
                        if (settings.enableStudying)
                            AccessTools.Field(typeof(CompProperties_Studiable), "requiresHoldingPlatform").SetValue(p, true);
                    }
                }
                else
                {
                    foreach (CompProperties property in def.comps)
                    {
                        CompProperties_Studiable p = property as CompProperties_Studiable;
                        if (p == null)
                            continue;
                        if (settings.enableStudying)
                        {
                            p.frequencyTicks = settings.frequencyTicks;
                            AccessTools.Field(typeof(CompProperties_Studiable), "requiresHoldingPlatform").SetValue(p, true);
                            AccessTools.Field(typeof(CompProperties_Studiable), "anomalyKnowledge").SetValue(p, settings.anomalyKnowledge);
                        }
                    }
                }
            }
        }
    }
}