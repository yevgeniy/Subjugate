using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace Subjugate
{

    public class JobDriver_GetNakedToSleep : JobDriver
    {
        private Toil gotoLocation;
        private Action atLocation;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var job = this.job as GetNakedToSleepJob;
            job.SetTarget(TargetIndex.A, job.bed.Thing.Position);
            job.SetTarget(TargetIndex.C, this.pawn);

            yield return Toils_General.Wait(2);

            var end = Toils_General.Do(() =>
            {
                this.pawn.jobs.StartJob(job.sleepJob);
            });

            /*gotobed location */
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);

            /*take off all the clothing*/
            var doneStripping = Toils_General.Wait(2);

            /* strip the girl naked */
            var stripNakedToils = Utils.StripNaked(job, this.pawn, TargetIndex.C, doneStripping, out var droppedClothing);
            foreach (var i in stripNakedToils)
                yield return i;

            yield return doneStripping;

            /*all the dropped clothing should be allowed*/
            yield return Toils_General.Do(() =>
            {
                foreach (var (clothing, _) in droppedClothing)
                {
                    clothing.SetForbidden(false);
                }
            });

            yield return end;

        }

        
    }


}
