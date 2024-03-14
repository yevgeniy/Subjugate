using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Subjugate.SubjucationPerks
{
    public class Perk : IExposable
    {
        public virtual string Name => "N/A";
        private float poolxp;
        public virtual float PoolXp { get { return poolxp; } set { poolxp = value; } }
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

        public static Action<Perk> WillDoSkill(Pawn pawn, SkillRecord skill)
        {
            if (skill.PermanentlyDisabled || skill.TotallyDisabled)
            {
                return perk =>
                {
                    perk.ForceEnable = true;
                    perk.Explain = $"{pawn} will now do {skill.def.defName} skill.";
                };
            }
            return null;
        }
        public static Action<Perk> GainMinorPassion(Pawn pawn, SkillRecord skill)
        {
            byte nopassion = 0;
            byte apathy = 3;
            byte curpassion = (byte)skill.passion;
            if (curpassion == nopassion || curpassion == apathy)
            {
                return perk =>
                {
                    skill.passion = Passion.Minor;
                    perk.Explain = $"{pawn} gained minor inspiration in {skill.def.defName} skill.";
                };
            }
            return null;


        }
        public static Action<Perk> GainMajorPassion(Pawn pawn, SkillRecord skill)
        {
            if (skill.passion == Passion.Minor)
            {
                return perk =>
                {
                    skill.passion = Passion.Major;
                    perk.Explain = $"{pawn} gained major inspiration in {skill.def.defName} skill.";
                };
            }
            return null;
        }
        public static Action<Perk> GainBurningPassion(Pawn pawn, SkillRecord skill)
        {
            if (Subjugate.HasVanillaSkillMod && skill.passion == Passion.Major)
            {
                return perk =>
                {
                    skill.passion = (Passion)((byte)5);
                    perk.Explain = $"{pawn} gained burning inspiration in {skill.def.defName} skill.";
                };
            }
            return null;
        }

        public virtual void Activate(Pawn pawn)
        {
            throw new NotImplementedException();
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref explain, "subjugate-perk-explain");
            Scribe_Values.Look(ref forceEnable, "subjugate-perk-force-act");
            Scribe_Values.Look(ref poolxp, "subjugate-perk-poolxp");
            

        }

        public virtual bool IsSkillForceEnabled(SkillRecord skill)
        {
            return false;
        }

        public virtual float DrainXP(Pawn pawn, float amount)
        {
            var drained = Mathf.Min(amount, PoolXp);
            PoolXp -= drained;

            Explain = $"{pawn} has reserve xp of {PoolXp.ToString("N")}";
            return drained;
        }


    }
}
