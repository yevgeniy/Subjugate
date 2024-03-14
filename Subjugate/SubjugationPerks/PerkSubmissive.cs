using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    internal class PerkSubmissive : Perk
    {
        public override string Name => "Submissive Companion";


        public override string NextLevelExplain(Pawn pawn)
        {
            return CompSubjugate.GetComp(pawn).Perks.Any(v => v is PerkSubmissive)
                ? null
                : $"{pawn} will become a submissive companion in bed.  Men who sleep with her, " +
                "upon effective rest, will gain +10 mood and +20% work rate increase for 8 hours.  Ladies who " +
                "sleep with her will gain rest need 20% quicker.  Effect increases per bed occupants who also have this perk by 5%";
        }


        public override void Activate(Pawn pawn)
        {
            Log.Message("ACTIVATE");
        }
    }
}
