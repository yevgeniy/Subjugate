using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Subjugate
{
    public class Hediff_PussyShockRod : HediffWithComps
    {

        public Thing PussyShockRod => this.pawn.TryGet_PussyShockRod(out var item)
            ? item
            : null;
        public bool IsShocking => PussyShockRod != null && PussyShockRod.TryGet_PussyShockRod_Comp(out var c) 
            ? c.IsShocking 
            : false;
        public float Charge => PussyShockRod != null && PussyShockRod.TryGet_PussyShockRod_Comp(out var c) ? c.Charge : 0f;

        public override string Label => $"Pussy shock rod {(this.IsShocking ? "ACTIVE!" : "")} ({Convert.ToInt32(Charge * 100f)})";

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            
            Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(Defs.Subj_PussyShockRodInMyPussy_Thought);
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
        }


        private int shocked = 0;
        public void Shock()
        {
            this.shocked = 200;
        }

        public override void Tick()
        {
            base.Tick();

            shocked--;
            if (shocked > 0)
            {
                this.Severity = 1f;
            }
            else
            {
                this.Severity = .01f;
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref shocked, "hed-pussyrod-shocked");

        }


    }
}
