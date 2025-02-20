using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Verse;
using Verse.AI;

namespace Subjugate
{
    public class AttendToGirlJob : Job
    {
        public bool needRemoval;
        public bool needInsert;
    }

    public class Girl_Stop : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Wait(9999999);
        }
    }
    public class Girl_BendOver : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //var turncorrectly = new Toil
            //{
            //    initAction = () =>
            //    {
                    
            //        var warden = this.job.targetA.Pawn;

            //        Log.Message($"turn correctly {warden.Rotation} {this.pawn.Rotation}");
            //        //warden.rotationTracker.Face(girl.DrawPos);
            //        warden.rotationTracker.FaceCell(this.pawn.Position);
            //        warden.rotationTracker.UpdateRotation();

            //        Log.Message($"turn correctly {warden.Rotation} {this.pawn.Rotation}");
            //    }
            //};
            var sayouch = new Toil
            {
                initAction = () =>
                {
                    ThrowMetaIconF(pawn.Position, pawn.Map, Defs.Subj_NoHeart_Fleck);
                }
            };

            //yield return turncorrectly;
            yield return sayouch;
            yield return Toils_General.Wait(100);
            yield return Toils_Jump.Jump(sayouch);
        }

        public void ThrowMetaIconF(IntVec3 pos, Map map, FleckDef icon)
        {
            FleckMaker.ThrowMetaIcon(pos, map, icon);
        }
    }

    public class AttendToGirl : JobDriver
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

            //AddFinishAction(c =>
            //{
            //    job.GetTarget(girlTarget).Pawn.jobs.StopAll();
            //});

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
                            driverClass = typeof(Girl_Stop)
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

            var makeGirlSquirm = PussyRodUtils.MakeGirlSquirm(job, pawn, girlTarget);

            var stripNakedToils = PussyRodUtils.StripNaked(job, pawn, girlTarget, makeGirlSquirm, out var droppedClothing);
            foreach (var i in stripNakedToils)
                yield return i;

            yield return makeGirlSquirm;

            /* Take out old pussy rod */
            var checkedForOldRod = Toils_General.Wait(2);
            yield return Toils_Jump.JumpIf(checkedForOldRod, () =>
            {
                if (!job.GetTarget(girlTarget).Pawn.health.hediffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var h))
                    return true;

                if (h is Hediff_PussyShockRod rodHediff && !rodHediff.HasPussyRod)
                    return true;

                return false;
            });

            yield return Toils_General.Wait(500, TargetIndex.None).WithProgressBarToilDelay(girlTarget);

            yield return new Toil
            {
                initAction = () =>
                {
                    var girl = job.GetTarget(girlTarget).Pawn;
                    var hediff = girl.health.hediffSet.GetFirstHediffOfDef(Defs.Subj_PussyShockRod_Hediff) as Hediff_PussyShockRod;
                    var pussyShockRod = hediff.ExtractPussyShockRod();
                    Subjugate.GirlsNeedingRemoval.Remove(girl);

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
                        var hediff = girl.health.hediffSet.GetFirstHediffOfDef(Defs.Subj_PussyShockRod_Hediff) as Hediff_PussyShockRod;
                        if (hediff == null)
                        {
                            hediff = HediffMaker.MakeHediff(Defs.Subj_PussyShockRod_Hediff, girl,
                                girl.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso)) as Hediff_PussyShockRod;

                            girl.health.AddHediff(hediff, girl.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso));
                        }
                        hediff.InsertPussyShockRod(newThing as ThingWithComps);
                        Subjugate.GirlsNeedingInsert.Remove(girl);

                        job.SetTarget(pussyRodTarget, newThing);
                    }
                };
                yield return new Toil
                {
                    initAction = () =>
                    {
                        var pussyRod = job.GetTarget(pussyRodTarget).Thing;
                        pussyRod.DeSpawn();
                    }
                };
            }

            var letGirlGo = PussyRodUtils.LetGirlGo(job, girlTarget);

            var dressToils = PussyRodUtils.GetDressed(job, pawn, girlTarget, droppedClothing, letGirlGo);
            foreach (var i in dressToils)
                yield return i;

            /* make girl reset her jobs */
            yield return letGirlGo;

            yield return Toils_General.Wait(2);

        }
    }

    public static class PussyRodUtils
    {
        public static IEnumerable<Toil> StripNaked(Job job, Pawn pawn, TargetIndex girlTarget, Toil next, out List<Apparel> droppedClothing)
        {
            var droppedclothing = new List<Apparel>() { };
            droppedClothing = droppedclothing;

            /* make girl turn around */
            var faceCorrectly = new Toil
            {
                initAction = () =>
                {
                    var girl = job.GetTarget(girlTarget).Pawn;
                    pawn.rotationTracker.Face(girl.DrawPos);

                    girl.Rotation = pawn.Rotation;
                }
            };


            var checkForClothing = Toils_Jump.JumpIf(next, () => job.GetTarget(girlTarget).Pawn.apparel.WornApparelCount == 0);
            var takeIfOffWait = Toils_General.Wait(50, TargetIndex.None).WithProgressBarToilDelay(girlTarget);
            var takeItOff = new Toil
            {
                initAction = () =>
                {
                    var girl = job.GetTarget(girlTarget).Pawn;
                    Apparel apparel = girl.apparel.WornApparel[0];
                    // Try to remove the apparel
                    girl.apparel.TryDrop(apparel, out Apparel resultingApparel, girl.Position);
                    droppedclothing.Add(resultingApparel);
                }
            };



            return new List<Toil>()
            {
                faceCorrectly,
                checkForClothing,
                takeIfOffWait,
                takeItOff,
                Toils_Jump.Jump(faceCorrectly)
            };

        }

        public static IEnumerable<Toil> GetDressed(Job job, Pawn pawn, TargetIndex girlTarget, List<Apparel> droppedClothing, Toil next)
        {
            var checkWhatToPutOn = Toils_Jump.JumpIf(next, () => droppedClothing.Count == 0);
            var putOnProgBar = Toils_General.Wait(50, TargetIndex.None).WithProgressBarToilDelay(girlTarget);
            var putOn = new Toil
            {
                initAction = () =>
                {
                    var girl = job.GetTarget(girlTarget).Pawn;
                    Apparel apparel = droppedClothing[0];
                    droppedClothing.Remove(apparel);

                    // Try to remove the apparel
                    girl.apparel.Wear(apparel);
                }
            };

            return new List<Toil>()
            {
                checkWhatToPutOn,
                putOnProgBar,
                putOn,
                Toils_Jump.Jump(checkWhatToPutOn)
            };

        }

        public static Toil LetGirlGo(Job job, TargetIndex girlTarget)
        {
            return new Toil
            {
                initAction = () =>
                {
                    Log.Message($"reset jobs");
                    var girl = job.GetTarget(girlTarget).Pawn;
                    if (girl.pather.Destination != null)
                    {
                        girl.jobs.StopAll();
                    }
                }
            };
        }

        public static Toil MakeGirlSquirm(Job job, Pawn warden, TargetIndex girlTarget)
        {
            return new Toil
            {
                initAction = () =>
                {
                    Log.Message($"make mote");
                    var girl = job.GetTarget(girlTarget).Pawn;
                    var bendOver = new Job
                    {
                        def = new JobDef
                        {
                            driverClass = typeof(Girl_BendOver)
                        },
                        targetA=warden,
                        targetB=job.GetTarget(girlTarget),
                        count = 0
                    };
                    girl.jobs.StartJob(bendOver);
                }
            };
        }

        public static bool GirlNeedsAttention(Pawn girl, out bool needInsert, out bool needRemoval)
        {
            needInsert = false;
            needRemoval = false;

            if (!Subjugate.ShouldHaveShockRod.Contains(girl))
            {
                if (girl.health.hediffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var h))
                {
                    needRemoval = true;
                    return true;
                }
                return false;
            }

            var hediff = girl.health.hediffSet.GetFirstHediffOfDef(Defs.Subj_PussyShockRod_Hediff) as Hediff_PussyShockRod;
            if (hediff == null || !hediff.HasPussyRod || hediff.pussyRodProxy.charge < .3f)
            {
                needInsert = true;
                return true;
            }

            return false;
        }
    }


    //public class CriticalThingHaulDestination : IHaulDestination
    //{
    //    private Thing thing;
    //    private Func<Thing, bool> accepts;

    //    public CriticalThingHaulDestination(Thing t, Func<Thing, bool> accepts)
    //    {
    //        this.thing = t;
    //        this.accepts = accepts;
    //    }

    //    public Thing Thing => thing;
    //    public int? CountNeeded = null;
    //    public int? ProgressBarDelay = null;

    //    public static explicit operator Thing(CriticalThingHaulDestination haulableDestination)
    //    {
    //        return haulableDestination.Thing;
    //    }

    //    public IntVec3 Position => Thing.Position;

    //    public Map Map => Thing.Map;

    //    public bool StorageTabVisible => false;

    //    public virtual bool Accepts(Thing t)
    //    {
    //        return this.accepts(t);
    //    }

    //    public StorageSettings GetParentStoreSettings()
    //    {
    //        return null;
    //    }

    //    public StorageSettings GetStoreSettings()
    //    {
    //        return new StorageSettings
    //        {
    //            Priority = StoragePriority.Critical
    //        };

    //    }

    //    public void Notify_SettingsChanged()
    //    {

    //    }
    //}

    //public class CriticalHaulThingOwner : ThingOwner
    //{
    //    public CriticalHaulThingOwner(Thing t, ThingOwner orig)
    //    {
    //        Log.Message($"thing {t}, orig {orig}");
    //        Thing = t;
    //        Original = orig;
    //    }

    //    public override int Count
    //    {
    //        get
    //        {
    //            Log.Message($"GETTING COUNT");
    //            return 0;
    //            //return Original.Count;
    //        }
    //    }

    //    public Thing Thing { get; set; }
    //    public ThingOwner Original { get; }

    //    public override int GetCountCanAccept(Thing item, bool canMergeWithExistingStacks = true)
    //    {
    //        Log.Message($"GET COUNT CAN ACCEPT");
    //        return 0;

    //        //var project = Thing as IConstructible;

    //        //var numberOfThisThingNeeded = project.ThingCountNeeded(item.def);
    //        //return numberOfThisThingNeeded;

    //    }

    //    public override int IndexOf(Thing item)
    //    {
    //        Log.Message($"INDEX OF {item}");
    //        return -1;
    //        //return Original.IndexOf(item);
    //    }

    //    public override bool Remove(Thing item)
    //    {
    //        Log.Message($"REMOVE {item}");
    //        return false;
    //        //return Original.Remove(item);
    //    }

    //    public override int TryAdd(Thing item, int count, bool canMergeWithExistingStacks = true)
    //    {
    //        Log.Message($"TRI ADD a");
    //        return 1;
    //        //return Original.TryAdd(item, count, canMergeWithExistingStacks);
    //    }

    //    public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
    //    {
    //        Log.Message($"TRI ADD b");
    //        return false;
    //        //return Original.TryAdd(item, canMergeWithExistingStacks);
    //    }

    //    public override Thing GetAt(int index)
    //    {
    //        Log.Message($"GET AT");
    //        return null;
    //        //return Original.GetAt(index);
    //    }
    //}



    //public class CarryToCharger : JobDriver
    //{
    //    public override bool TryMakePreToilReservations(bool errorOnFailed)
    //    {
    //        return this.pawn.Reserve(TargetA, this.job);
    //    }

    //    protected override IEnumerable<Toil> MakeNewToils()
    //    {
    //        /* take pussy rod to charging station */
    //        yield return new Toil
    //        {
    //            initAction = () =>
    //            {
    //                if (!StoreUtility.TryFindBestBetterStorageFor(job.targetA.Thing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var cell, out var destination))
    //                {
    //                    Log.Message("COULD NOT FIND PLACE");
    //                    pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
    //                    return;
    //                }
    //                if (destination is ISlotGroupParent)
    //                {
    //                    /* cell */
    //                    Log.Message("COULD NOT FIND CHARGER");
    //                    pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
    //                    return;
    //                }
    //                job.SetTarget(TargetIndex.B, destination as Thing);
    //            }
    //        };
    //        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
    //        yield return Toils_Goto.GotoCell(job.targetB.Cell, PathEndMode.Touch);

    //        yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.B);

    //        yield return Toils_General.Wait(2);
    //    }
    //}

}
