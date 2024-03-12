using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    public class PerkNudistTrait:Perk
    {
        public override string Name => "Nudist";
        public override string NextLevelExplain(Pawn pawn)
        {
            if (pawn.story.traits.GetTrait(TraitDefOf.Nudist)==null)
                return "Lady slave will love being naked.";

            return null;
        }
        public override bool CanHandle(Pawn pawn)
        {
            return !pawn.story.traits.HasTrait(TraitDefOf.Nudist);
        }

        public override void Activate(Pawn pawn)
        {
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Nudist, 0, true));
            Explain = "PAWN loves being naked";
        }
    }
}
