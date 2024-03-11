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

        public override SkillDef SkillDef => SkillDefOf.Artistic;

        public override bool CanHandle(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDef);
            if (skill.TotallyDisabled)
                return true;

            var p = (byte)skill.passion;
            if (p == 0 || p == 1 || p == 2 || p == 3) /*none, minor, major, apathy */
                return true;
            return false;

        }

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
