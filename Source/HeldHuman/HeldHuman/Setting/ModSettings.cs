using Verse;

namespace HeldHuman.Setting
{
    public class ModSettings : Verse.ModSettings
    {
        public bool enableProducingBioferrate = false;
        public float bioferriteDensity = 0.5f;

        public bool enableStudying = false;
        public int frequencyTicks = 120000;
        public float anomalyKnowledge = 0.5f;

        public void Reset()
        {
            enableProducingBioferrate = false;
            bioferriteDensity = 0.5f;

            enableStudying = false;
            frequencyTicks = 120000;
            anomalyKnowledge = 0.5f;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableProducingBioferrate, "heldHuman.enableProducingBioferrate");
            Scribe_Values.Look(ref bioferriteDensity, "heldHuman.bioferriteDensity");
            Scribe_Values.Look(ref enableStudying, "heldHuman.enableStudying");
            Scribe_Values.Look(ref frequencyTicks, "heldHuman.frequencyTicks");
            Scribe_Values.Look(ref anomalyKnowledge, "heldHuman.anomalyKnowledge");
            base.ExposeData();
        }
    }
}