using RimWorld;
using System.Text;
using Verse;

namespace HeldHuman.Hook
{
    public abstract class PowerHooker : Hooker
    {
        public abstract void Modify(Pawn pawn, ref float value);
    }
}
