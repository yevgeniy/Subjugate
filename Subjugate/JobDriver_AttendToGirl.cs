using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace Subjugate
{

    public class JobDriver_AttendToGirl : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            /*target A - pussy rod to be inserted
             * target B - girl
             * target C - pussy rod that needs to be taken out */
            var job = this.job as AttendToGirlJob;


            return this.pawn.Reserve(TargetB, job)
                && (job.needRemoval
                    || job.needInsert && this.pawn.Reserve(TargetA, job));

        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var job = this.job as AttendToGirlJob;
            var girlTarget = TargetIndex.B;
            var pussyRodTarget = TargetIndex.A;


            yield return Toils_General.Wait(2);
            /*make target stop*/
            yield return new Toil
            {
                initAction = () =>
                {
                    var girl = job.GetTarget(girlTarget).Pawn;
                    Log.Message($"stop target {girl}");
                    var stopJob = new Job
                    {
                        def = new JobDef
                        {
                            driverClass = typeof(JobDriver_GirlStop)
                        },
                        count = 0
                    };
                    girl.jobs.StartJob(stopJob);
                }
            };

            if (job.needInsert)
            {
                /*go get the pussy rod*/
                yield return Toils_Goto.GotoThing(pussyRodTarget, PathEndMode.Touch);

                /* start hauling pussy rod*/
                yield return Toils_Haul.StartCarryThing(pussyRodTarget);

            }

            /* goto girl that needs to be taken care of */
            yield return Toils_Goto.GotoThing(girlTarget, PathEndMode.Touch);

            var makeGirlSquirm = Utils.MakeGirlSquirm(job, pawn, girlTarget);

            var stripNakedToils = Utils.StripNaked(job, pawn, girlTarget, makeGirlSquirm, out var droppedClothing);
            foreach (var i in stripNakedToils)
                yield return i;

            yield return makeGirlSquirm;

            /* Take out old pussy rod */
            var checkedForOldRod = Toils_General.Wait(2);
            yield return Toils_Jump.JumpIf(checkedForOldRod, () =>
            {
                var girl = job.GetTarget(girlTarget).Pawn;
                if (girl.TryGet_PussyShockRod(out var _))
                {
                    return false;
                }
                return true;
            });

            yield return Toils_General.Wait(500, TargetIndex.None).WithProgressBarToilDelay(girlTarget);

            yield return new Toil
            {
                initAction = () =>
                {
                    var girl = job.GetTarget(girlTarget).Pawn;
                    if (!girl.TryGet_PussyShockRod(out var pussyShockRod))
                    {
                        Log.Error($"COULD NOT FIND PUSSY SHOCK ROD ON GIRL {this.pawn}");
                        this.pawn.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                    girl.apparel.Remove(pussyShockRod);
                    girl.NeedPussyRodRemoval(false);

                    GenPlace.TryPlaceThing(pussyShockRod, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }
            };

            yield return checkedForOldRod;

            if (job.needInsert)
            {

                yield return Toils_General.Wait(500, TargetIndex.None).WithProgressBarToilDelay(girlTarget);
                yield return new Toil
                {
                    initAction = () =>
                    {

                        pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var newThing);

                        var girl = job.GetTarget(girlTarget).Pawn;
                        girl.apparel.Wear(newThing as Apparel, true, true);
                        girl.NeedPussyRodInsert(false);

                        job.SetTarget(pussyRodTarget, newThing);
                    }
                };

            }

            var letGirlGo = Utils.LetGirlGo(job, girlTarget);

            var dressToils = Utils.GetDressed(job, pawn, girlTarget, droppedClothing, letGirlGo);
            foreach (var i in dressToils)
                yield return i;

            /* make girl reset her jobs */
            yield return letGirlGo;

            yield return Toils_General.Wait(2);

        }
    }


}
