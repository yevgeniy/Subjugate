using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Subjugate
{
    public class Thought_PussyShockRodInMyPussy : Thought_Memory
    {
        private float moodoffset;


        public override bool ShouldDiscard
        {
            get
            {
                return !pawn.health.hediffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var _);
            }
        }

        public override float MoodOffset()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Defs.Subj_PussyShockRod_Hediff) as Hediff_PussyShockRod;
            float days = hediff.TicksInserted / GenDate.TicksPerHour;
            this.moodoffset = Mathf.Min(-30f + days, -5f);
            return this.moodoffset;
        }
    }
}
