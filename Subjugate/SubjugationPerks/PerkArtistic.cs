﻿using RimWorld;
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
        public static Func<Pawn, SkillRecord, Action<Perk>>[] levels = new Func<Pawn, SkillRecord, Action<Perk>>[]{
            WillDoArt,
            GainMinorPassion,
            GainMajorPassion,
            GainBurningPassion
        };
        public static Action<Perk> WillDoArt(Pawn pawn, SkillRecord skill)
        {
            if (CompSubjugate.GetComp(pawn).Perks.Any(v => v.GetType().Name == typeof(PerkArtistic).Name))
                return null;

            return (perk) =>
            {
                perk.Explain = $"{pawn} will now engage in artistic work";
            };
            
        }
        public override string NextLevelExplain(Pawn pawn)
        {
            var skill = pawn.skills.skills.Find(v => v.def.defName == SkillDefOf.Artistic.defName);
            if (levels[0](pawn, skill) != null)
            {
                return pawn + " will engage in artistic work.";
            }
            else if (levels[1](pawn, skill) != null)
            {
                return pawn + " will gain minor passion in art.";
            }
            else if (levels[2](pawn, skill) != null)
            {
                return pawn + " will gain major passion in art.";
            }
            else if (levels[3](pawn, skill) != null)
            {
                return pawn + " will gain burning passion in art.";
            }
            
            return null;
        }

        

        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.skills.Find(v => v.def.defName == SkillDefOf.Artistic.defName);
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
            return skill.def.defName == SkillDefOf.Artistic.defName && ForceEnable;
        }
    }
}
