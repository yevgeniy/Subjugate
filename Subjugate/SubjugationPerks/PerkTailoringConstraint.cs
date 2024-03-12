using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    internal class PerkTailoringConstraint : Perk
    {
        public override SkillDef SkillDef => SkillDefOf.Crafting;

        public override void Activate(Pawn pawn)
        {
            Explain = "PAWN will craft only tailoring";
            
            /* if pawn has apathy in crafting remove it */
            var skill = pawn.skills.GetSkill(SkillDef);
            byte p = (byte)skill.passion;
            if (p==3)
            {
                skill.passion = Passion.None;    
            }
            skill.Notify_SkillDisablesChanged();
            pawn.Notify_DisabledWorkTypesChanged();

            //if (p == 0 || p == 3) /* none or apathy*/
            //{
            //    explain = "PAWN likes SKILL";
            //    return 1;
            //}

            //else if (p == 1)
            //{
            //    explain = "PAWN loves SKILL";
            //    return 2;
            //}

            //else if (p == 2)
            //{
            //    explain = "PAWN is infatuated with SKILL";
            //    return new byte[] { 4, 5 }.RandomElement();
            //}

            //Log.Error("ERROR IN SKILL INCREASE: " + p + " " + pawn.Name.ToStringShort);
            //return p;


            //var ex = "";
            //var newPassion = Perk.UtilPassionIncrease(pawn, SkillDef, ref ex);
            //Explain = ex.Replace("SKILL", "tailoring");
            //Explain += "\n    " + "- will not do other crafting";
            //ForceActivate = true;
            //var skill = pawn.skills.GetSkill(SkillDef);
            //skill.passion = (Passion)newPassion;
            //skill.Notify_SkillDisablesChanged();
            //pawn.Notify_DisabledWorkTypesChanged();
        }

        public static bool HasTailoringPerk(Pawn pawn)
        {
            return CompSubjugate.GetComp(pawn).Perks.Any(v => v.GetType().Name == typeof(PerkTailoringConstraint).Name);
        }
    }
}
