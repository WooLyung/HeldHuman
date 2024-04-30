using RimWorld;
using Verse;

namespace HeldHuman.Tool
{
    public static class PlatformTool
    {
        public static Pawn GetHeldPawn(Thing holder) => (holder as ThingWithComps)?.GetComp<CompEntityHolderPlatform>()?.HeldPawn;
    }
}
