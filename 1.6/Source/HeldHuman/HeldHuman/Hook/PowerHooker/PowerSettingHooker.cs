using RimWorld;
using Verse;

namespace HeldHuman.Hook
{
    public class PowerSettingHooker : PowerHooker
    {
        public override int Priority => 1000;

        public override void Modify(Pawn pawn, ref float value)
        {
            
        }
    }
}
