using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    public class DenySkillPerk : Perk
    {

        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDef);
            
            Disabled = true;
            Explain = "PAWN will no longer do SKILL.";
            skill.passion = Passion.None;
            skill.Notify_SkillDisablesChanged();


            //var currPassion = skill.passion;
            ///* All passions above minors will be set to minor passion */
            //byte x = (byte)currPassion;
            //if (x==1 || x == 2 || x == 4 || x == 5) /*minor, major, natural, critical */
            //{

            //    Explain = "No longer passionate about SKILL.";
            //    byte p = (byte)(Subjugate.HasVanillaSkillMod ? 3 : 0);
            //    skill.passion = (Passion)p;
            //}
            //else /* apathy or no passion */
            //{

                
            //}
            
        }

    }
}
