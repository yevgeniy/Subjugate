using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    public class Perk : IExposable
    {
        public virtual string Name => "N/A";
        public virtual string NextLevelExplain(Pawn pawn)
        {
            return null;
        }
        private string explain;
        public virtual string Explain { get {
                return explain;
            } set { explain = value; } }

        private bool forceEnable;
        public virtual bool ForceEnable { get { return forceEnable; } set { forceEnable = value; } }

        private bool disabled;
        public virtual bool Disabled { get { return disabled; } set { disabled = value; } }



        public virtual void Activate(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref explain, "subjugate-perk-explain");
            Scribe_Values.Look(ref forceEnable, "subjugate-perk-force-act");

        }

        public virtual bool IsSkillForceEnabled(SkillRecord skill)
        {
            return false;
        }

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


   



    }
}
