using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Subjugate
{
    public class JobDriver_EmptyWear : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Do(() =>
            {
                this.pawn.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame + Rand.Range(6000, 9000);
            });

            yield return Toils_General.Wait(2);
        }


    }
}
