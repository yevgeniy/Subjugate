using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Subjugate
{
    public class Thought_GotPunished : Thought_Memory
    {
        private float moodoffset;


        public override bool ShouldDiscard => !pawn.GotBeatings(out float rating);
        
        public override float MoodOffset()
        {
            if (pawn.GotBeatings(out var rating))
            {
                return -Mathf.Clamp(0f, rating, 30f);
            }
            return 0f;
        }
    }
}
