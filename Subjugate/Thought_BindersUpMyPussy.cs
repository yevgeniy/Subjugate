using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Subjugate
{
    public class Thought_BindersUpMyPussy : Thought_Memory
    {
        private float moodoffset;


        public override bool ShouldDiscard => !pawn.TryGet_Binders(out var _);
        
        public override float MoodOffset()
        {
            if (pawn.TryGet_Subjugate_Comp(out var comp))
            {
                float days = comp.TotalPussyInsert / GenDate.TicksPerDay;
                this.moodoffset = Mathf.Min(-30f + days, -5f);
                return this.moodoffset;
            }

            return 0f;

            
        }
    }
}
