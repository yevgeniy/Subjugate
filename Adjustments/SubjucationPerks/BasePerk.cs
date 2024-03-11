using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments.SubjucationPerks
{
    public class BasePerk : IPerk, INegSkillPerk, IExposable
    {

        private string explain;
        public virtual string Explain { get { return explain; } set { explain = value; } }

        private bool forceActivate;
        public virtual bool ForceActivate { get { return forceActivate; } set { forceActivate = value; } }

        public static byte UtilPassionIncrease(Pawn pawn, SkillDef skillDef, ref string explain)
        {
            var skill = pawn.skills.GetSkill(skillDef);

            byte p = (byte)skill.passion;
            if (p == 0 || p==3) /* none or apathy*/
            {
                explain = "PAWN likes SKILL";
                return 1;
            }
                
            else if (p == 1)
            {
                explain = "PAWN loves SKILL";
                return 2;
            }
                
            else if (p == 2)
            {
                explain = "PAWN is infatuated with SKILL";
                return new byte[] { 4, 5 }.RandomElement();
            }
            
            Log.Error("ERROR IN SKILL INCREASE: " + p + " " + pawn.Name.ToStringShort);
            return p;
        }


        public virtual SkillDef SkillDef => new SkillDef { defName = "" };

        private bool disabled;
        public virtual bool Disabled { get { return disabled; } set { disabled = value; } }

        public virtual void Activate(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanHandle(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual void Deactivate(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref explain, "subjugate-perk-explain");
            Scribe_Values.Look(ref disabled, "subjugate-perk-disabled");
            Scribe_Values.Look(ref forceActivate, "subjugate-perk-force-act");
            
        }

        public virtual bool IsDisabled(SkillRecord skill)
        {
            if (skill.def.defName== SkillDef.defName)
                return Disabled;

            return false;
        }

        public virtual bool IsEnabled(SkillRecord skill)
        {
            if (skill.def.defName == SkillDef.defName)
                return ForceActivate;

            return false;
        }



        public virtual string Describe(Pawn pawn)
        {
            return Explain.Replace("SKILL", SkillDef.skillLabel).Replace("PAWN", pawn.Name.ToStringShort);
        }
    }
}
