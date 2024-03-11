using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    public class ApathyPerk : Perk
    {

        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDef);
            
            Disabled = true;
            Explain = "PAWN no longer likes to do SKILL.";

            if (Subjugate.HasVanillaSkillMod)
            {
                skill.passion = Passion.None;
            } else
            {
                byte p = 3;
                skill.passion = (Passion)p;
            }

            skill.Notify_SkillDisablesChanged();
            
        }

    }
}
