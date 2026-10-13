using HeldHuman.ToilHelper;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace HeldHuman.Def
{
    public class JobDriver_EnslaveOrReduceWillHeldHuman : JobDriver_EnslaveOrReduceWillPrisoner
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => !Prisoner.IsPrisonerOfColony || !Prisoner.guest.PrisonerIsSecure);

            PrisonerInteractionModeDef interaction = (Prisoner.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Enslave) ? PrisonerInteractionModeDefOf.Enslave : PrisonerInteractionModeDefOf.ReduceWill);
            yield return Toils_Interpersonal.GotoPrisoner(pawn, Prisoner, interaction);
            yield return Toils_Interpersonal.ReduceWill(pawn, Prisoner);
            yield return Toils_Interpersonal.ReduceWill(pawn, Prisoner);
            yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
            yield return Toils.TryEnslave(TargetIndex.A);
        }
    }
}
