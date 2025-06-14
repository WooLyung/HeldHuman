using HeldHuman.ToilHelper;
using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace HeldHuman.Def
{
    public class JobDriver_ChatWithHeldHuman : JobDriver_ChatWithPrisoner
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(() => !Talkee.IsPrisonerOfColony || !Talkee.guest.PrisonerIsSecure);

            yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.ExclusiveInteractionMode);
            yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
            yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
            yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
            yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
            yield return Toils_Interpersonal.ConvinceRecruitee(pawn, Talkee);
            yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
            yield return Toils.TryRecruit(TargetIndex.A);
        }
    }
}