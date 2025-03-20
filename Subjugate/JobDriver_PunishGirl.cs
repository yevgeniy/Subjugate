using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using Verse.AI.Group;

namespace Subjugate
{

    public class JobDriver_PunishGirl : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            var job = this.job as PunishGirlJob;


            return this.pawn.Reserve(TargetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            var job = this.job as PunishGirlJob;
            var girlTarget = TargetIndex.A;

            AddFailCondition(() =>
            {
                var girl = this.job.GetTarget(girlTarget).Pawn;
                AcceptanceReport allowsDrafting = girl.GetLord()?.AllowsDrafting(girl) ?? ((AcceptanceReport)true);
                
                girl.TryGet_Subjugate_Comp(out var comp);

                return girl.Downed || girl.Drafted || !allowsDrafting || comp.BeatingRating>=100f;
            });
            AddFinishAction(c =>
            {
                var girl = this.job.GetTarget(girlTarget).Pawn;
                girl.jobs.StopAll();
            });

            yield return Toils_General.Wait(2);

            /*make the girl stop*/
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

            /* goto girl that needs to be punished */
            yield return Toils_Goto.GotoThing(girlTarget, PathEndMode.Touch);

            var doneStripping = Toils_General.Wait(2);

            /* strip the girl naked */
            var stripNakedToils = Utils.StripNaked(job, pawn, girlTarget, doneStripping, out var droppedClothing);
            foreach (var i in stripNakedToils)
                yield return i;

            yield return doneStripping;

            foreach (var i in HitAFewTimes(girlTarget))
            {
                yield return i;
            }
            yield return Toils_General.Wait(100);
            yield return GirlMoveAway(girlTarget);

            yield return Toils_Goto.GotoThing(girlTarget, PathEndMode.OnCell);

            yield return Toils_Jump.Jump(doneStripping);


        }

        public Toil GirlMoveAway(TargetIndex girlTargetIndex)
        {
            var done = false;

            return new Toil()
            {
                initAction = () =>
                {
                    var girl = this.job.GetTarget(girlTargetIndex).Pawn;
                    var location = FindRandomSpotNearPawn(girl);
                    var girlDriver = girl.jobs.curDriver as JobDriver_GirlStop;

                    done = false;

                    girlDriver.GoToLocation(location, () =>
                    {
                        done = true;
                    });
                },
                tickAction = () =>
                {
                    if (done)
                    {
                        this.ReadyForNextToil();
                    }
                }
            };
        }

        public IntVec3 FindRandomSpotNearPawn(Pawn pawn, float maxDistance = 3f)
        {

            IntVec3 cel = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, (int)maxDistance);
            return cel.IsValid ? cel : IntVec3.Invalid;
        }

        public IEnumerable<Toil> HitAFewTimes(TargetIndex girlTarget)
        {
            var swat = Toils_General.Do(() =>
            {
                var girl = this.job.GetTarget(girlTarget).Pawn;
                this.pawn.PunishTheGirl(girl);
            });
            var wait = Toils_General.Wait(100);

            yield return swat;
            yield return wait;
            yield return swat;
            yield return wait;
            yield return swat;
            yield return wait;

        }
    }

}
