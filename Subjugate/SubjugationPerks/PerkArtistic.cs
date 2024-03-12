using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    internal class PerkArtistic : Perk
    {
        public override string Name => "Artistic";
        public override string NextLevelExplain(Pawn pawn)
        {
            var comp = CompSubjugate.GetComp(pawn);
            if (!comp.Perks.Any(v => v.GetType().Name == typeof(PerkArtistic).Name) )
            {
                return "Lady slave will engage in artistic work.";
            }
            
            return null;
        }

        public override SkillDef SkillDef => SkillDefOf.Artistic;

        public override void Activate(Pawn pawn)
        {
            /* activate for slave */

            var ex = "";
            var newPassion = Perk.UtilPassionIncrease(pawn, SkillDef, ref ex);
            Explain = ex; 
            ForceActivate = true;
            var skill = pawn.skills.GetSkill(SkillDef);
            skill.passion = (Passion)newPassion;

            skill.Notify_SkillDisablesChanged();
        }

        public static bool ShouldDoArt(Pawn pawn)
        {
            return CompSubjugate.GetComp(pawn).Perks.Any(v => v.GetType().Name == typeof(PerkArtistic).Name);
            
        }
    }
}
