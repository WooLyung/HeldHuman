using RimWorld;
using System.Collections.Generic;
using Verse;

namespace HeldHuman.Def
{
    public class InteractionWorker_ConvertIdeoAttempt_HeldHuman : InteractionWorker_ConvertIdeoAttempt
    {
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            Ideo ideo = recipient.Ideo;
            Precept_Role role = ideo.GetRole(recipient);
            float certainty = recipient.ideo.Certainty;

            if (recipient.ideo.IdeoConversionAttempt(CertaintyReduction(initiator, recipient), initiator.Ideo))
            {
                if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
                {
                    letterLabel = "LetterLabelConvertIdeoAttempt_Success".Translate();
                    letterText = "LetterConvertIdeoAttempt_Success".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"), initiator.Ideo.Named("IDEO"), ideo.Named("OLDIDEO")).Resolve();
                    letterDef = LetterDefOf.PositiveEvent;
                    lookTargets = new LookTargets(initiator, recipient);
                    if (role != null)
                        letterText = letterText + "\n\n" + "LetterRoleLostLetterIdeoChangedPostfix".Translate(recipient.Named("PAWN"), role.Named("ROLE"), ideo.Named("OLDIDEO")).Resolve();
                }

                extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptSuccess);
                return;
            }

            float num2 = Rand.Value * (0.979999959f);
            if (num2 < 0.78f || recipient.IsPrisoner)
            {
                extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFail);
            }
            else
            {
                if (recipient.needs.mood != null)
                {
                    if (PawnUtility.ShouldSendNotificationAbout(recipient))
                        Messages.Message("MessageFailedConvertIdeoAttempt".Translate(initiator.Named("INITIATOR"), recipient.Named("RECIPIENT"), certainty.ToStringPercent().Named("CERTAINTYBEFORE"), recipient.ideo.Certainty.ToStringPercent().Named("CERTAINTYAFTER")), recipient, MessageTypeDefOf.NeutralEvent);
                    recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedConvertIdeoAttemptResentment, initiator);
                }
                extraSentencePacks.Add(RulePackDefOf.Sentence_ConvertIdeoAttemptFailResentment);
            }
        }
    }
}
