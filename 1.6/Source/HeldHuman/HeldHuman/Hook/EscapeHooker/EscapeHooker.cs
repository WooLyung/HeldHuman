using System.Text;
using Verse;

namespace HeldHuman.Hook
{
    public abstract class EscapeHooker : Hooker
    {
        public abstract void Modify(Pawn pawn, ref float value);
        public virtual void AddStatDraw(Pawn pawn, ref StringBuilder stringBuilder) { }
    }
}
