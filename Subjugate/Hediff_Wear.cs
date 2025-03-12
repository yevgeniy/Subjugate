using RimWorld;
using Subjugate.Comp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace Subjugate
{
    public class Hediff_Wear : HediffWithComps
    {
        public static Dictionary<Hediff_Wear, float> MaxMoodOffsets = new Dictionary<Hediff_Wear, float>();

        public Pawn Girl => this.pawn;
        public CompProperties_HediffMinSubjugation Props => this.GetComp<CompMinSubjugation>().Props;
        public override void Tick()
        {
            base.Tick();

            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                var current = Girl.Subjugate_Hediff();
                var moodOffset = 0f;
                if (!Girl.IsValidWear(Props.min, Props.maxMoodOffset, out moodOffset))
                {
                    MaxMoodOffsets[this] = moodOffset;


                    Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(Defs.Subj_HumiliatingWear_Thought);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
                }
                MaxMoodOffsets[this] = moodOffset;
                
                Log.Message($"Mood offset: {moodOffset}");
            }
        }
        public override void PostRemoved()
        {
            base.PostRemoved();
            MaxMoodOffsets.Remove(this);
        }
    }

    public class CompProperties_HediffMinSubjugation: HediffCompProperties
    {
        public float min;
        public float maxMoodOffset;
        public CompProperties_HediffMinSubjugation()
        {
            this.compClass = typeof(CompMinSubjugation);
        }
    }
    public class CompMinSubjugation: HediffComp
    {
        
        public CompProperties_HediffMinSubjugation Props
        {
            get
            {
                return (CompProperties_HediffMinSubjugation)this.props;
            }
        }
    }
    


}
