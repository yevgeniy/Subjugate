using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    internal class PerkGentle : Perk
    {
        public override string Name => "Gentle Companion";


        public override string NextLevelExplain(Pawn pawn)
        {
            return CompSubjugate.GetComp(pawn).Perks.Any(v => v is PerkGentle)
                ? null
                : $"{pawn} will become a gentle companion.  Ladies who " +
                $"sleep with {pawn} will gain rest 20% quicker.  Effect increases per bed occupants who also have this perk by 5%";
        }


        public override void Activate(Pawn pawn)
        {
            Explain = "Ladies who " +
                $"sleep with {pawn} will gain rest 20% quicker.  Effect increases per bed occupants who also have this perk by 5%";
        }
    }
}
