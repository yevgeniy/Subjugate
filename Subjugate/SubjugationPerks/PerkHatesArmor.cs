using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    internal class PerkHatesArmor : Perk
    {

        public override void Activate(Pawn pawn)
        {
            Explain = "PAWN hates wearing armor";
        }

        public static bool HatesArmor(Pawn pawn)
        {
            return CompSubjugate.GetComp(pawn).Perks.Any(v => v.GetType().Name == typeof(PerkHatesArmor).Name);
        }
    }
}
