﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    internal class PerkCooking: Perk
    {
        public override string Name => "Cooking";
        public static Func<Pawn, SkillRecord, Action<Perk>>[] levels = new Func<Pawn, SkillRecord, Action<Perk>>[]{
            WillDoSkill,
            GainMinorPassion,
            GainMajorPassion,
            GainBurningPassion
        };

        public override string NextLevelExplain(Pawn pawn)
        {
            var skill = pawn.skills.skills.Find(v => v.def.defName == SkillDefOf.Cooking.defName);
            if (levels[0](pawn, skill) != null)
            {
                return pawn + " will engage in cooking work.";
            }
            else if (levels[1](pawn, skill) != null)
            {
                return pawn + " will gain minor passion for cooking.";
            }
            else if (levels[2](pawn, skill) != null)
            {
                return pawn + " will gain major passion for cooking.";
            }
            else if (levels[3](pawn, skill) != null)
            {
                return pawn + " will gain burning passion for cooking.";
            }
            
            return null;
        }

        

        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.skills.Find(v => v.def.defName == SkillDefOf.Cooking.defName);
            for (var x = 0; x < levels.Count(); x++)
            {
                var activator = levels[x](pawn, skill);
                if (activator!=null)
                {
                    activator(this);
                    break;
                }
            }   
        }

        public override bool IsSkillForceEnabled(SkillRecord skill)
        {
            return skill.def.defName == SkillDefOf.Cooking.defName && ForceEnable;
        }
    }
}
