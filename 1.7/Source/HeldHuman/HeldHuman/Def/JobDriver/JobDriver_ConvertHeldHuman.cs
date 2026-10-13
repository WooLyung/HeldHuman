using HeldHuman.ToilHelper;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace HeldHuman.Def
{
    public class JobDriver_ConvertHeldHuman : JobDriver_ConvertPrisoner
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (ModLister.CheckIdeology("Ideoligion conversion"))
            {
                this.FailOnDestroyedOrNull(TargetIndex.A);
                this.FailOn(() => !Prisoner.IsPrisonerOfColony || !Prisoner.guest.PrisonerIsSecure);

                yield return Toils_Interpersonal.GotoPrisoner(pawn, Prisoner, PrisonerInteractionModeDefOf.Convert);
                yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Prisoner, InteractionDefOf.Chitchat);
                yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Prisoner, InteractionDefOf.Chitchat);
                yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Prisoner, InteractionDefOf.Chitchat);
                yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
                yield return Toils.TryConvert(TargetIndex.A);
            }
        }
    }
}
