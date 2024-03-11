using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.SubjucationPerks
{
    internal class PerkNegHateArmor : BasePerk
    {

        public override bool CanHandle(Pawn pawn)
        {
            return !SubjugateComp.GetComp(pawn).Perks.Any(v => v.GetType().Name == this.GetType().Name);
        }

        public override void Activate(Pawn pawn)
        {
            Explain = "PAWN hates wearing armor";
        }

        public static bool HatesArmor(Pawn pawn)
        {
            return SubjugateComp.GetComp(pawn).Perks.Any(v => v.GetType().Name == typeof(PerkNegHateArmor).Name);
        }
    }
}
