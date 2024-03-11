using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.SubjucationPerks
{
    public class NegSkillPerk : BasePerk
    {
        public override bool CanHandle(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDef);
            if (skill.PermanentlyDisabled)
            {
                return false;
            }

            if (SubjugateComp.Repo.ContainsKey(pawn))
            {
                var existing =  SubjugateComp.GetComp(pawn).Perks.FirstOrDefault(v => v.SkillDef == SkillDef);
                if (existing == null)
                    return true;

                return existing.Disabled == false;
            }
            return false;
        }
        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDefOf.Shooting);
            var currPassion = skill.passion;

            /* All passions above minors will be set to minor passion */
            byte x = (byte)currPassion;
            if (x==1 || x == 2 || x == 4 || x == 5) /*minor, major, natural, critical */
            {

                Explain = "No longer passionate about SKILL.";
                byte p = (byte)(Adjustments.HassVanillaSkillMod ? 3 : 0);
                skill.passion = (Passion)p;
            }
            else /* apathy or no passion */
            {

                Disabled = true;
                Explain = "PAWN will no longer do SKILL.";
                skill.passion = Passion.None;
                skill.Notify_SkillDisablesChanged();
            }
            
        }

    }
}
