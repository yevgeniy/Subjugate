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

        public ShockRodProxy pussyRodProxy;



        public bool HasPussyRod { get { return pussyRodProxy!=null; }}

        public override bool ShouldRemove => false;
        public override bool Visible => HasPussyRod;

        public override string Label => $"Pussy shock rod {(this.isShocking ? "ACTIVE!" : "")} ({ Convert.ToInt32( pussyRodProxy.charge*100f)})";

        public void InsertPussyShockRod(ThingWithComps pussyShockRod)
        {
            this.pussyRodProxy = pussyShockRod.GetComp<CompInsertPussyShockRod>().proxy;
        }
        public ThingWithComps ExtractPussyShockRod()
        {
            var pussyShockRod = ThingMaker.MakeThing(Defs.Subj_PussyShockRod_Item) as ThingWithComps;
            pussyShockRod.GetComp<CompInsertPussyShockRod>().proxy = this.pussyRodProxy;
            this.pussyRodProxy = null;
            return pussyShockRod;
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

            Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(Defs.Subj_PussyShockRodInMyPussy_Thought);
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
        }
        private int ticksInserted = 0;
        private bool isShocking=false;
        public bool IsShocking => this.IsShocking;

        public int TicksInserted { get { return ticksInserted; } }

        public override void Tick()
        {
            base.Tick();

            if (HasPussyRod)
            {
                ticksInserted++; 
                pussyRodProxy.Tick();

                if (Find.TickManager.TicksGame % (GenDate.TicksPerHour/5)==0 && isShocking)
                {
                    var girl = this.pawn;
                    if (!girl.Downed )
                    {
                        if (pussyRodProxy.TryShock())
                        {
                            var torso = girl.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso);
                            girl.health.AddHediff(Defs.Subj_ShockTheGirl_Hediff, torso);
                        }
                        else
                        {
                            ShockOff();
                        }   
                    }
                }

                
            }

        }

        public void ShockOff()
        {
            this.isShocking = false;
        }

        public void ShockOn()
        {
            this.isShocking = true;
        }



        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref pussyRodProxy, "hed-pussyrod-rod");
            Scribe_Values.Look(ref ticksInserted, "hed-pussyrod-ticksins");
            Scribe_Values.Look(ref isShocking, "hed-pussyrod-isshocking");
        }

        
    }
}
