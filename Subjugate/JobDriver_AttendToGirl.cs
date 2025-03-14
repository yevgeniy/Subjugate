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

            if (!this.pawn.Reserve(TargetB, job))
                return false;

            if (job.needInstall)
            {
                if (!this.pawn.Reserve(TargetA, job))
                {
                    return false;
                }
            }

            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var job = this.job as AttendToGirlJob;
            var girlTarget = TargetIndex.B;
            var itemTarget = TargetIndex.A;


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

            if (job.needInstall)
            {
                /*go get the pussy rod*/
                yield return Toils_Goto.GotoThing(itemTarget, PathEndMode.Touch);

                /* start hauling pussy rod*/
                yield return Toils_Haul.StartCarryThing(itemTarget);

            }

            /* goto girl that needs to be taken care of */
            yield return Toils_Goto.GotoThing(girlTarget, PathEndMode.Touch);

            var makeGirlSquirm = Utils.MakeGirlSquirm(job, pawn, girlTarget);

            var stripNakedToils = Utils.StripNaked(job, pawn, girlTarget, makeGirlSquirm, out var droppedClothing);
            foreach (var i in stripNakedToils)
                yield return i;

            yield return makeGirlSquirm;

            /* Take out old pussy insert if need be */
            var checkForOldInsert = Toils_General.Wait(2);
            yield return Toils_Jump.JumpIf(checkForOldInsert, () =>
            {
                var girl = job.GetTarget(girlTarget).Pawn;
                if (girl.TryGet_AnySubjugationWear(out Apparel apparel))
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

                    if (!girl.TryGet_AnySubjugationWear( out var item))
                    {
                        Log.Error($"COULD NOT FIND INSERTED ITEM ON GIRL{this.pawn}");
                        this.pawn.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                    girl.apparel.Remove(item);
                    if (job.needRemoval)
                        girl.GirlHasNeeds(false);

                    GenPlace.TryPlaceThing(item, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }
            };

            yield return checkForOldInsert;

            if (job.needInstall)
            {

                yield return Toils_General.Wait(500, TargetIndex.None).WithProgressBarToilDelay(girlTarget);
                yield return new Toil
                {
                    initAction = () =>
                    {

                        pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var newThing);

                        var girl = job.GetTarget(girlTarget).Pawn;
                        girl.apparel.Wear(newThing as Apparel, true, true);
                        girl.GirlHasNeeds(false);

                        job.SetTarget(itemTarget, newThing);
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
