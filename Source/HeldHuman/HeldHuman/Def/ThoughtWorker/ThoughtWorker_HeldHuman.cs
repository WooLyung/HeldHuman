using HarmonyLib;
using HeldHuman.Tool;
using RimWorld;
using Verse;

namespace HeldHuman.Def.ThoughtWorker_
{
    public class ThoughtWorker_HeldHuman : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (!HumanTool.IsHoldableHuman(pawn) || !pawn.IsOnHoldingPlatform)
                return ThoughtState.Inactive;

            Building_HoldingPlatform platform = (Building_HoldingPlatform)pawn.ParentHolder;
            int tick = Find.TickManager.TicksGame - (int)AccessTools.Field(typeof(Building_HoldingPlatform), "heldPawnStartTick").GetValue(platform);

            if (tick <= 60000 * 3) // 3 days
                return ThoughtState.ActiveAtStage(0);
            else if (tick <= 60000 * 15) // 15 days
                return ThoughtState.ActiveAtStage(1);
            else // 60 days
                return ThoughtState.ActiveAtStage(2);
        }
    }
}
