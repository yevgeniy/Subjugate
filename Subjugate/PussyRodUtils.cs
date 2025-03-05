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
using Verse.AI.Group;

namespace Subjugate
{

    public static class PussyRodUtils
    {
        public static bool TryGet_PussyShockRod_Hediff(this Pawn girl, out Hediff_PussyShockRod hediff )
        {
            return girl.health.hediffSet.TryGetHediff<Hediff_PussyShockRod>(out hediff);
            
        }
        public static Hediff_Subjugation Subjugate_Hediff(this Pawn girl)
        {
            if (!girl.health.hediffSet.TryGetHediff<Hediff_Subjugation>(out var hediff))
            {
                hediff = HediffMaker.MakeHediff(Defs.Subj_Subjugation_Hediff, girl,
                                girl.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Head)) as Hediff_Subjugation;

                girl.health.AddHediff(hediff, girl.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Head));
            }
            return hediff;
        }

        public static bool TryGet_Subjugate_Comp(this Pawn pawn, out CompSubjugate comp)
        {
            return pawn.TryGetComp<CompSubjugate>(out comp);
        }

        public static void NeedPussyRodInsert(this Pawn girl, bool f=true)
        {
            if (f)
                Find.CurrentMap.GetComponent<Subjugate>().girlNeed[girl] = NeedType.Insert;
            else
                Find.CurrentMap.GetComponent<Subjugate>().girlNeed.Remove(girl);
        }
        public static void NeedPussyRodRemoval(this Pawn girl, bool f = true)
        {
            if (f)
                Find.CurrentMap.GetComponent<Subjugate>().girlNeed[girl] = NeedType.Remove;
            else
                Find.CurrentMap.GetComponent<Subjugate>().girlNeed.Remove(girl);
        }

        public static void ThrowMetaIconF(IntVec3 pos, Map map, FleckDef icon)
        {
            FleckMaker.ThrowMetaIcon(pos, map, icon);
        }
        public static IEnumerable<Toil> StripNaked(Job job, Pawn pawn, TargetIndex girlTarget, Toil next, out List<Apparel> droppedClothing)
        {
            var droppedclothing = new List<Apparel>() { };
            droppedClothing = droppedclothing;

            var girl = job.GetTarget(girlTarget).Pawn;

            /* make girl turn around */
            var faceCorrectly = new Toil
            {
                initAction = () =>
                {
                    pawn.rotationTracker.Face(girl.DrawPos);

                    girl.Rotation = pawn.Rotation;
                }
            };

            var checkForClothing = Toils_Jump.JumpIf(next, () => girl.apparel.WornApparel.Where(v => v.def != Defs.Subj_PussyShockRod_Item).Count() == 0);
            var takeIfOffWait = Toils_General.Wait(50, TargetIndex.None).WithProgressBarToilDelay(girlTarget);
            var takeItOff = new Toil
            {
                initAction = () =>
                {
                    Apparel apparel = girl.apparel.WornApparel.FirstOrDefault(v => v.def != Defs.Subj_PussyShockRod_Item);
                    
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
                        targetA = warden,
                        targetB = job.GetTarget(girlTarget),
                        count = 0
                    };
                    girl.jobs.StartJob(bendOver);
                }
            };
        }

        public static bool TryGet_GirlNeeds(this Pawn girl, out bool needInsert, out bool needRemoval)
        {
            needInsert = false;
            needRemoval = false;

            if (girl.IsColonistPlayerControlled || girl.IsColonyMech || girl.IsColonyMutantPlayerControlled || girl.IsPrisonerInPrisonCell())
            {
                AcceptanceReport allowsDrafting = girl.GetLord()?.AllowsDrafting(girl) ?? ((AcceptanceReport)true);
                if (allowsDrafting)
                {
                    if (girl.Dead)
                    {
                        return false;
                    }
                    if (false==girl.GetComp<CompSubjugate>().ShouldHaveShockrod)
                    {
                        if (girl.TryGet_PussyShockRod(out var _))
                        {
                            needRemoval = true;
                            return true;
                        }
                        return false;
                    }

                    if (false==girl.TryGet_PussyShockRod(out var _))
                    {
                        needInsert = true;
                        return true;
                    }

                    if (girl.TryGet_PussyShockRod(out var item) && item.TryGet_PussyShockRod_Comp(out var comp)
                        && comp.Charge < .3f)
                    {
                        needInsert = true;
                        return true;
                    }
                }
            }

            return false;
        }
        public static MentalStateDef[] NeedPunishmentStates = new MentalStateDef[] {
                MentalStateDefOf.Manhunter,
                MentalStateDefOf.Berserk,
                MentalStateDefOf.HumanityBreak,
                MentalStateDefOf.PanicFlee,
                MentalStateDefOf.Rebellion,
                MentalStateDefOf.SocialFighting,
                MentalStateDefOf.Wander_OwnRoom,
                MentalStateDefOf.Wander_Psychotic,
                MentalStateDefOf.Wander_Sad
        };

        public static bool GirlNeedsPunishing(this Pawn pawn)
        {
            if (pawn.gender==Gender.Female)
            {
                return pawn.guilt.IsGuilty || NeedPunishmentStates.Contains(pawn.MentalStateDef);
            }

            return false;
        }

        public static bool TryGet_PussyShockRodRemote(this Pawn pawn, out Thing item)
        {
            item = pawn.inventory.innerContainer.FirstOrDefault(v => v.def.defName == "Subj_PussyShockRodRemote_Item");
            return item != null;
        }
        public static bool TryGet_PussyShockRod(this Pawn pawn, out Apparel item)
        {
            item = pawn.apparel.WornApparel.FirstOrDefault(v => v.def.defName == "Subj_PussyShockRod_Item") as Apparel;
            return item != null;
        }
        public static bool TryGet_PussyShockRod(this Pawn pawn, out Apparel item, out CompInsertPussyShockRod comp )
        {
            comp = null;
            if (pawn.TryGet_PussyShockRod(out item))
            {
                return item.TryGet_PussyShockRod_Comp(out comp);
            }
            return false;
        }
        public static bool TryGet_PussyShockRod_Comp(this Thing item, out CompInsertPussyShockRod comp)
        {
            if (item.TryGetComp<CompInsertPussyShockRod>(out comp))
            {
                return true;
            }
            return false;
        }
    }

    public class AttendToGirlJob : Job
    {
        public bool needRemoval;
        public bool needInsert;
    }

    public class EmptyJob : JobDriver
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
                    PussyRodUtils.ThrowMetaIconF(pawn.Position, pawn.Map, Defs.Subj_NoHeart_Fleck);
                }
            };

            //yield return turncorrectly;
            yield return sayouch;
            yield return Toils_General.Wait(100);
            yield return Toils_Jump.Jump(sayouch);
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

            var letGirlGo = PussyRodUtils.LetGirlGo(job, girlTarget);

            var dressToils = PussyRodUtils.GetDressed(job, pawn, girlTarget, droppedClothing, letGirlGo);
            foreach (var i in dressToils)
                yield return i;

            /* make girl reset her jobs */
            yield return letGirlGo;

            yield return Toils_General.Wait(2);

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
