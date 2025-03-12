using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static UnityEngine.GraphicsBuffer;

namespace Subjugate
{

    public static class Utils
    {
        public static T RandomElement<T>(T[] elements)
        {
            if (elements == null || elements.Length == 0)
            {
                throw new ArgumentException("Array cannot be null or empty");
            }

            Random random = new Random();
            int index = random.Next(0, elements.Length);
            return elements[index];
        }

        public static void Punish(this Pawn warden, Pawn girl)
        {
            Find.CurrentMap.GetComponent<Subjugate>().needsPunishing.Remove(girl);

            warden.Drawer.Notify_MeleeAttackOn(girl);

            var randomheight = RandomElement(new BodyPartHeight[] { BodyPartHeight.Middle, BodyPartHeight.Bottom });
            var bodyrecord = girl.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Blunt, randomheight, BodyPartDepth.Outside);

            girl.health.AddHediff(Defs.Subj_PunishTheGirl_Hediff, bodyrecord);

            Utils.ThrowMetaIconF(girl.Position, girl.Map, Defs.Subj_NoHeart_Fleck);


            if (girl.TryGet_Subjugate_Comp(out var subjComp) && subjComp.NeedsPunishment)
            {
                girl.Subjugate_Hediff().AddSeverity(.01f);
                subjComp.BeatingRating++;
            }


        }
        public static bool GotBeatings(this Pawn girl, out float rating)
        {
            girl.TryGet_Subjugate_Comp(out var comp);
            rating = comp.BeatingRating;
            return rating > 0f;
        }
        public static void NeedsPunishing(this Pawn girl, bool f = true)
        {
            if (f)
                Find.CurrentMap.GetComponent<Subjugate>().needsPunishing[girl] = true;
            else
                Find.CurrentMap.GetComponent<Subjugate>().needsPunishing.Remove(girl);
        }
        public static bool IsValidWear(this Pawn girl, float min, float maxMoodOffset, out float moodOffset)
        {
            var subjHediff = girl.Subjugate_Hediff();
            moodOffset = 0f;
            if (girl.IsPuppet())
            {
                return true;
            }

            var severity = subjHediff.Severity;
            if (severity >= min)
            {
                return true;
            }

            var diff = min - severity;
            var rat = diff / min;
            moodOffset = maxMoodOffset * rat;

            return false;
        }
        public static bool IsPuppet(this Pawn girl)
        {
            return girl.health.hediffSet.hediffs.Any(vv => vv.def.defName == "VPEP_Puppet");
        }
        public static bool TryGet_PussyShockRod_Hediff(this Pawn girl, out Hediff_PussyShockRod hediff)
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

        public static void NeedPussyRodInsert(this Pawn girl, bool f = true)
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
        public static IEnumerable<Toil> StripNaked(Job job, Pawn pawn, TargetIndex girlTarget, Toil next, out Dictionary<Apparel, bool> droppedClothing)
        {
            var droppedclothing = new Dictionary<Apparel, bool>();
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

            var checkForClothing = Toils_Jump.JumpIf(next, () => girl.apparel.WornApparel
                .Where(v =>
                    v.def != Defs.Subj_PussyShockRod_Item
                    && !girl.apparel.IsLocked(v)
                 ).Count() == 0);

            var takeIfOffWait = Toils_General.Wait(50, TargetIndex.None).WithProgressBarToilDelay(girlTarget);
            var takeItOff = new Toil
            {
                initAction = () =>
                {
                    Apparel apparel = girl.apparel.WornApparel
                        .FirstOrDefault(v => v.def != Defs.Subj_PussyShockRod_Item && !girl.apparel.IsLocked(v));

                    var isForced = girl.outfits.forcedHandler.IsForced(apparel);

                    // Try to remove the apparel
                    girl.apparel.TryDrop(apparel, out Apparel resultingApparel, girl.Position);

                    droppedclothing[resultingApparel] = isForced;

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

        public static IEnumerable<Toil> GetDressed(Job job, Pawn pawn, TargetIndex girlTarget, Dictionary<Apparel, bool> droppedClothing, Toil next)
        {
            var checkWhatToPutOn = Toils_Jump.JumpIf(next, () => droppedClothing.Count == 0);
            var putOnProgBar = Toils_General.Wait(50, TargetIndex.None).WithProgressBarToilDelay(girlTarget);
            var putOn = new Toil
            {
                initAction = () =>
                {
                    var girl = job.GetTarget(girlTarget).Pawn;
                    (Apparel apparel, bool forced) = droppedClothing.First();
                    droppedClothing.Remove(apparel);

                    girl.apparel.Wear(apparel);
                    if (forced)
                        girl.outfits.forcedHandler.SetForced(apparel, true);
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
                            driverClass = typeof(JobDriver_GirlBendOver)
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
                    if (false == girl.GetComp<CompSubjugate>().ShouldHaveShockrod)
                    {
                        if (girl.TryGet_PussyShockRod(out var _))
                        {
                            needRemoval = true;
                            return true;
                        }
                        return false;
                    }

                    if (false == girl.TryGet_PussyShockRod(out var _))
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
            if (pawn.gender == Gender.Female)
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
        public static bool TryGet_PussyShockRod(this Pawn pawn, out Apparel item, out CompInsertPussyShockRod comp)
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
    public class PunishGirlJob : Job
    {

    }

}
