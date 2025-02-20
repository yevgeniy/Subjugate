using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Subjugate
{
    public class WorkGiver_RemovePussyShockRod : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return null;
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return false;
            //if (NeedToRemove.Contains(pawn))
            //    return false;

            //var r = NeedToRemove.Contains(t)
            //    && pawn.CanReach(t, PathEndMode.ClosestTouch, Danger.None)
            //    && pawn.CanReserve(t);
            //return r;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn == t)
                return null;

            if (!pawn.CanReach(t, PathEndMode.ClosestTouch, Danger.None))
                return null;

            if (!pawn.CanReserve(t))
                return null;


            var job = new Job
            {
                def = new JobDef
                {
                    driverClass = typeof(ExtractRodDriver),
                    label = "Extract pussy shock rod"
                },
                targetA = t,
                count = 1
            };


            return job;

        }
    }

    public class ExtractRodDriver : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(TargetA, this.job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            return PussyRodUtils.RemoveToils(this.job, this.pawn, TargetIndex.A, TargetIndex.B);
        }
    }
}
