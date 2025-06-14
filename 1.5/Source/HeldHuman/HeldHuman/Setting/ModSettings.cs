using System.Runtime;
using Verse;

namespace HeldHuman.Setting
{
    public class ModSettings : Verse.ModSettings
    {
        public bool enableProducingBioferrate;
        public float bioferriteDensity;
        public bool enableStudying;
        public int frequencyTicks;
        public float anomalyKnowledge;
        public int powerFactor;
        public bool enableFood;

        private static ModSettings instance;
        public static ModSettings Instance => instance;
        public ModSettings() => instance = this;

        public void Reset()
        {
            enableProducingBioferrate = false;
            bioferriteDensity = 0.5f;
            enableStudying = false;
            frequencyTicks = 120000;
            anomalyKnowledge = 0.5f;
            powerFactor = 100;
            enableFood = false;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableProducingBioferrate, "heldHuman.enableProducingBioferrate", false);
            Scribe_Values.Look(ref bioferriteDensity, "heldHuman.bioferriteDensity", 0.5f);
            Scribe_Values.Look(ref enableStudying, "heldHuman.enableStudying", false);
            Scribe_Values.Look(ref frequencyTicks, "heldHuman.frequencyTicks", 120000);
            Scribe_Values.Look(ref anomalyKnowledge, "heldHuman.anomalyKnowledge", 0.5f);
            Scribe_Values.Look(ref powerFactor, "heldHuman.powerFactor", 100);
            Scribe_Values.Look(ref enableFood, "heldHuman.enableFood", false);
            base.ExposeData();
        }
    }
}