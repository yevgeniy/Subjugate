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
        public int SkillCap = 10;
        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.GetSkill(SkillDef);
            
            Explain = "PAWN no longer likes to do SKILL.";

            if (!Subjugate.HasVanillaSkillMod)
            {
                skill.passion = Passion.None;
            } else
            {
                byte p = 3;
                skill.passion = (Passion)p;
            }

            skill.Notify_SkillDisablesChanged();
            
        }

        public override bool HasSkillCap(SkillDef def, ref int skillcap)
        {
            skillcap = SkillCap;
            return def.defName == SkillDef.defName;
        }
    }
}
