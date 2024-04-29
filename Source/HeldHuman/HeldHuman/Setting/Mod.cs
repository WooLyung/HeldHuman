using HarmonyLib;
using RimWorld;
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
            listingStandard.Label("- Bioferrite");
            listingStandard.CheckboxLabeled("enable producing bioferrate", ref settings.enableProducingBioferrate);
            settings.bioferriteDensity = listingStandard.SliderLabeled($"bioferrite density ({settings.bioferriteDensity:0.0})", settings.bioferriteDensity, 0.1f, 10.0f);
            listingStandard.GapLine(40);
            listingStandard.Label("- Studying");
            listingStandard.CheckboxLabeled("enable studying", ref settings.enableStudying);
            settings.frequencyTicks = (int)listingStandard.SliderLabeled($"frequency ticks ({settings.frequencyTicks})", settings.frequencyTicks, 6000, 600000);
            settings.anomalyKnowledge = listingStandard.SliderLabeled($"anomaly knowledge ({settings.anomalyKnowledge:0.0})", settings.anomalyKnowledge, 0.1f, 10.0f);
            listingStandard.GapLine(40);
            if (listingStandard.ButtonText("Reset"))
                settings.Reset();
            listingStandard.Label("If you have modified the settings, restart the game.");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Held Human";
        }

        private static void UpdateSettings()
        {
            ThingDef def = ThingDefOf.Human;

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
  
            foreach (CompProperties property in def.comps)
            {
                CompProperties_Studiable p = property as CompProperties_Studiable;
                if (p == null)
                    continue;
                if (settings.enableStudying)
                {
                    p.frequencyTicks = settings.frequencyTicks;
                    AccessTools.Field(typeof(CompProperties_Studiable), "anomalyKnowledge").SetValue(p, settings.anomalyKnowledge);
                }
                else
                {
                    p.frequencyTicks = 0;
                    AccessTools.Field(typeof(CompProperties_Studiable), "anomalyKnowledge").SetValue(p, 0.0f);
                }
            }
        }
    }
}