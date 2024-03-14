using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    internal class PerkSocial : Perk
    {
        public override string Name => "Social";
        public static Func<Pawn, SkillRecord, Action<Perk>>[] levels = new Func<Pawn, SkillRecord, Action<Perk>>[]{
            WillDoSocial,
            GainMinorPassion,
            GainMajorPassion,
            GainBurningPassion
        };

        public static Action<Perk> WillDoSocial(Pawn pawn, SkillRecord skill)
        {
            if (CompSubjugate.GetComp(pawn).Perks.Any(v => v.GetType().Name == typeof(PerkSocial).Name))
                return null;

            return (perk) =>
            {
                perk.ForceEnable = true;
                perk.Explain = $"{pawn} will now engage in social situations";

            };

        }

        public override string NextLevelExplain(Pawn pawn)
        {
            var skill = pawn.skills.skills.Find(v => v.def.defName == SkillDefOf.Social.defName);
            if (levels[0](pawn, skill) != null)
            {
                return pawn + " will engage in social work.";
            }
            else if (levels[1](pawn, skill) != null)
            {
                return pawn + " will gain minor passion in social cituations.";
            }
            else if (levels[2](pawn, skill) != null)
            {
                return pawn + " will gain major passion in social cituations.";
            }
            else if (levels[3](pawn, skill) != null)
            {
                return pawn + " will gain burning passion in social cituations.";
            }
            
            return null;
        }

        

        public override void Activate(Pawn pawn)
        {
            var skill = pawn.skills.skills.Find(v => v.def.defName == SkillDefOf.Social.defName);
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
            return skill.def.defName == SkillDefOf.Social.defName && ForceEnable;
        }

    }
}
