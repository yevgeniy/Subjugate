using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Adjustments
{
    public class SubjugateThoughtWorker : ThoughtWorker_Precept
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

    public class SubjugateAllWomenSlaves:ThoughtWorker_Precept
    {
        
        public override float MoodMultiplier(Pawn p)
        {
            return Mathf.Min(30f, colonist_buffer.NumberOfSlaveLadies);
        }

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (p.gender != Gender.Male)
                return false;

            return colonist_buffer.NumberOfFreeLadies == 0 && colonist_buffer.NumberOfSlaveLadies > 0;
                
        }
    }

    public class RecentlyDisciplinedAWoman : ThoughtWorker_Precept
    {

        public override float MoodMultiplier(Pawn p)
        {
            var comp = SubjugateComp.GetComp(p);
            if (comp != null)
            {
                return comp.DisciplineDealtRating;
            }
            return 1;
        }

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            var comp = SubjugateComp.GetComp(p);
            if (comp!=null)
            {
                return comp.DisciplineDealtRating > 0;
            }
            return false;

        }

    }

    

    /* Register a thing with ticker which will keep a buffer of colonists so that workers don't have to do
     * the heavy lifting for every colonist */

    public class colonist_buffer : MapComponent
    {
        public static int NumberOfSlaveLadies;
        public static int NumberOfFreeLadies;
        public colonist_buffer(Map m) : base(m)
        {
            
        }
        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % 2000==0)
            {
                var ladies=Find.Maps.SelectMany(v=>v.mapPawns.AllPawns).Where(v => v.gender == Gender.Female && !v.Dead);

                NumberOfSlaveLadies = ladies.Where(v => v.IsSlave).Count();
                NumberOfFreeLadies = ladies.Where(v => v.IsColonist && !v.IsSlave).Count();

            }
            
        }

    }
    
}
