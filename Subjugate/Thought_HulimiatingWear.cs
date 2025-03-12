using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Subjugate
{
    public class Thought_HulimiatingWear : Thought_Memory
    {
        private float moodoffset;


        public override bool ShouldDiscard => !pawn.IsHumiliated(out float rating);
        
        public override float MoodOffset()
        {
            if (pawn.IsHumiliated(out var rating))
            {
                return -rating;
            }
            return 0f;
        }
    }
}
