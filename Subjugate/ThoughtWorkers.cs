using RimWorld;
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
    public class colonist_buffer : MapComponent
    {
        public static int NumberOfSlaveLadies;
        public static int NumberOfFreeLadies;
        public colonist_buffer(Map m) : base(m)
        {
            
        }
        public override void MapComponentTick()
        {

            if (Find.TickManager.TicksGame % GenDate.TicksPerHour ==0)
            {
                var girls = Find.Maps.SelectMany(v => v.mapPawns.AllPawns).Where(v => v.gender == Gender.Female && !v.Dead && !v.IsQuestLodger());
                NumberOfSlaveLadies = 0;
                NumberOfFreeLadies = 0;

                foreach(var girl in girls)
                {
                    if (girl.TryGet_Subjugate_Comp(out var comp) && comp.IsGoodColonyGirl)
                    {
                        NumberOfSlaveLadies++;
                    } 
                    else
                    {
                        NumberOfFreeLadies++;
                    }
                }

                Log.Message($"free: {NumberOfFreeLadies} slave: {NumberOfSlaveLadies}");

            }
            
        }


    }

    public class ThoughtWorker_AllWomenSlaves:ThoughtWorker_Precept
    {
        
        public override float MoodMultiplier(Pawn p)
        {
            
            var ret= Mathf.Min(30f, colonist_buffer.NumberOfSlaveLadies);
            if (p.TryGet_Subjugate_Comp(out var comp))
            {
                comp.ForTheLadiesMult = ret;
            }
    
            return ret;
        }

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.gender != Gender.Male)
                return false;

            var ret = colonist_buffer.NumberOfFreeLadies == 0 && colonist_buffer.NumberOfSlaveLadies > 0;

            if (p.TryGet_Subjugate_Comp(out var comp))
            {
                comp.ForTheLadies = ret;
            }
                
            return ret;       
        }
    }

    public class ThoughtWorker_UnsubjugatedWomen : ThoughtWorker_Precept
    {
        public override float MoodMultiplier(Pawn p)
        {
            return Mathf.Min(10f, colonist_buffer.NumberOfFreeLadies);

        }

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.gender != Gender.Male)
                return false;
            return colonist_buffer.NumberOfFreeLadies > 0;
        }
    }
    
}
