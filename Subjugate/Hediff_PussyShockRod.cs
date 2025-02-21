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



        public bool HasPussyRod { get { return pussyRodProxy != null; } }

        public override bool ShouldRemove => false;
        public override bool Visible => HasPussyRod;

        public override string Label => $"Pussy shock rod {(this.isShocking ? "ACTIVE!" : "")} ({Convert.ToInt32(pussyRodProxy.charge * 100f)})";

        public void InsertPussyShockRod(ThingWithComps pussyShockRod)
        {
            this.pussyRodProxy = pussyShockRod.GetComp<CompInsertPussyShockRod>().proxy;

            Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(Defs.Subj_PussyShockRodInMyPussy_Thought);
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
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


        }
        private int ticksInserted = 0;
        private bool isShocking = false;
        public bool IsShocking => this.isShocking;

        public int TicksInserted { get { return ticksInserted; } }

        /*Every 20 punishTicks doubles the MTBEvent days result with diminishing results*/
        private int punishCounter = 0;
        public float MTBEventDaysMultiplyer()
        {
            var ticks = punishCounter;
            float res = 0f;
            float curMult = 1f;
            do
            {
                curMult += .6f;
                var punishPart = Math.Min(20, ticks);
                ticks -= punishPart;
                res += 1 / (10 * curMult) * punishPart;

            } while (ticks > 0);

            return 1f + res;
        }



        
        public override void Tick()
        {
            base.Tick();

            if (HasPussyRod)
            {
                ticksInserted++;
                pussyRodProxy.Tick();

                if (Find.TickManager.TicksGame % (GenDate.TicksPerHour / 5) == 0 && isShocking)
                {
                    var girl = this.pawn;

                    if (pussyRodProxy.TryShock())
                    {
                        var torso = girl.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso);
                        girl.health.AddHediff(Defs.Subj_ShockTheGirl_Hediff, torso);

                        PussyRodUtils.ThrowMetaIconF(pawn.Position, pawn.Map, Defs.Subj_NoHeart_Fleck);

                        if (girl.GetComp<CompSubjugate>().NeedsPunishment)
                        {
                            punishCounter++;
                        }

                    }
                    else
                    {
                        ShockOff();
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

            Scribe_Deep.Look(ref pussyRodProxy, "hed-pussyrod-rod", new object[] { });
            Scribe_Values.Look(ref ticksInserted, "hed-pussyrod-ticksins");
            Scribe_Values.Look(ref isShocking, "hed-pussyrod-isshocking");
            Scribe_Values.Look(ref punishCounter, "hed-pussyrod-punish");
            
            
        }


    }
}
