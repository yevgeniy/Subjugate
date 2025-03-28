
using RimWorld;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using HarmonyLib;
using System.Runtime.Remoting.Messaging;
using Verse.Noise;
using Verse.Sound;

namespace Subjugate
{

    [StaticConstructorOnStartup]
    public class CompSubjugate : ThingComp
    {

        static CompSubjugate()
        {
            /*add subjugate comp to all defs having a race */
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null && thingDef.race.Humanlike))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(CompSubjugate) });
            }
        }


        public static Dictionary<Pawn, CompSubjugate[]> Repo = new Dictionary<Pawn, CompSubjugate[]>();
        public Pawn Pawn
        {
            get
            {
                if (parent is Pawn pawn)
                {
                    return pawn;
                }
                return null;
            }
        }

        private Need_Suppression supneed;
        public Need_Suppression SupNeed
        {
            get
            {
                supneed = supneed ?? Pawn.needs.TryGetNeed<Need_Suppression>();
                return supneed;
            }
        }

        public long ticks;
        private string girlShouldHave;
        public string GirlShouldHave
        {
            get
            {
                return girlShouldHave;
            }
            set
            {

                this.girlShouldHave = value;
            }
        }

        private float beatingRating;
        public float BeatingRating
        {
            get { return this.beatingRating; }
            set
            {
                this.beatingRating = value;
            }
        }

        private bool fortheladies;
        public bool ForTheLadies
        {
            get
            {
                return fortheladies;
            }
            set
            {
                if (fortheladies && !value)
                {
                    var t = Pawn.story.traits.GetTrait(Defs.Subj_ForTheLadies_Trait);
                    Pawn.story.traits.RemoveTrait(t);
                }
                else if (!fortheladies && value)
                {
                    Pawn.story.traits.GainTrait(new Trait(Defs.Subj_ForTheLadies_Trait, 0, true));
                }
                fortheladies = value;
            }
        }

        public float ForTheLadiesMult;



        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();


            Scribe_Values.Look(ref fortheladies, "subjugate-for-lad");
            Scribe_Values.Look(ref ForTheLadiesMult, "subjugate-for-lad-mult");
            Scribe_Values.Look(ref guiltyTicksLeft, "subjugate-guilty-ticks");
            Scribe_Values.Look(ref totalPussyInsertTicks, "subjugate-pussyrod-ticksins");
            Scribe_Values.Look(ref beatingRating, "subjugate-beating-rate");
            Scribe_Values.Look(ref girlShouldHave, "subjugate-should-have");
            Scribe_Values.Look(ref punishLimit, "subjugate-pun-lim");
            Scribe_Values.Look(ref lastTimePunished, "subjugate-last-pun");
            Scribe_Values.Look(ref totalTicks, "subjugate-total-ti");
            Scribe_Values.Look(ref isGoodColonyGirl, "subjugate-good-girl");
            

        }


        public float CalcForTheLadies()
        {
            if (Pawn.gender != Gender.Male)
                return 0f;

            if (!fortheladies)
                return 0f;

            if (!Pawn.Ideo.HasPrecept(Defs.Subj_SubjugateAllWomen_Precept))
                return 0f;

            return ForTheLadiesMult * .01f;
        }


        private int totalPussyInsertTicks = 0;
        public int TotalPussyInsert { get { return totalPussyInsertTicks; } }
        private int totalTicks;

        public override void CompTick()
        {
            base.CompTick();
            guiltyTicksLeft--;
            totalTicks++;

            if (totalTicks % GenDate.TicksPerHour == 0)
            {
                IsGoodColonyGirl = CalcForGoodColonyGirl();
                this.punishLimit = Math.Max(0, this.punishLimit - 1);


                if (Pawn.gender == Gender.Female)
                {


                    beatingRating = Mathf.Max(0f, beatingRating - 1f);

                    Pawn.GirlHasNeeds(Pawn.TryGet_GirlNeeds(out bool j1, out bool j2, out bool j4));

                    if (Pawn.TryGet_PussyShockRod(out var item) || Pawn.TryGet_Binders(out var itme2))
                    {
                        totalPussyInsertTicks += GenDate.TicksPerHour;
                    }



                    if (Pawn.IsSlaveOfColony)
                    {

                        if (SupNeed.CurLevel < MaxSupNeedSuppress && Find.TickManager.TicksGame - this.lastTimePunished > GenDate.TicksPerDay)
                        {
                            Pawn.NeedsPunishing();
                        }


                        Pawn.Subjugate_Hediff().AddSeverity(.01f / 12f);
                        if (Pawn.GirlNeedsPunishing())
                        {
                            guiltyTicksLeft = GenDate.TicksPerDay;
                        }
                    }

                }


            }

        }

        private bool CalcForGoodColonyGirl()
        {
            var pawn = Pawn;
            if (pawn.gender == Gender.Female && !pawn.Dead && pawn.ageTracker.Adult
                    && (pawn.IsSlaveOfColony || pawn.IsPrisoner || pawn.health.hediffSet.hediffs.Any(vv => vv.def.defName == "VPEP_Puppet")))
            {
                var isSlaveApparel = pawn.apparel.WornApparel.All(apparel =>
                {
                    return Subjugation(apparel.def) || !OnLegsOrTorso(apparel.def);
                });
                return isSlaveApparel;
            }

            return false;
        }
        private bool OnLegsOrTorso(ThingDef def)
        {
            return def.apparel.bodyPartGroups.Any(v => v == BodyPartGroupDefOf.Torso || v == BodyPartGroupDefOf.Legs);
        }

        private bool Subjugation(ThingDef def)
        {
            return def.thingCategories.Any(v => v == Defs.Subj_Subjugation_ThingCategory || v == Defs.Subj_SlaveGirl_ThingCategory);
        }

        public override string CompInspectStringExtra()
        {
            var needPunishing = NeedsPunishment ? "NEEDS PUNISHING!" : "";

            return string.Join(" ", new string[] { $"({BeatingRating})", needPunishing }).Trim();
        }
        private int guiltyTicksLeft = 0;

        public bool isGoodColonyGirl;
        public bool IsGoodColonyGirl
        {
            get { return this.isGoodColonyGirl; }
            set { this.isGoodColonyGirl = value; }
        }

        private int punishLimit;
        private int lastTimePunished;

        public bool NeedsPunishment => guiltyTicksLeft > 0;

        public float MaxSupNeedSuppress
        {
            get
            {
                var subjHediff = Pawn.Subjugate_Hediff();
                var subjugateLevel = subjHediff.Severity * 10f;
                var resistance = subjHediff.Resistance;
                var offset = ((10f - subjugateLevel) / 10f * resistance) * 2;
                var maxeffectiveLevel = 1f - offset * 2f;
                return maxeffectiveLevel;
            }
        }


        public void GetPunished(Pawn warden)
        {
            var girl = Pawn;
            Find.CurrentMap.GetComponent<Subjugate>().needsPunishing.Remove(girl);

            warden.Drawer.Notify_MeleeAttackOn(girl);

            var randomheight = Utils.RandomElement(new BodyPartHeight[] { BodyPartHeight.Middle, BodyPartHeight.Bottom });
            var bodyrecord = girl.health.hediffSet.GetRandomNotMissingPart(DamageDefOf.Blunt, randomheight, BodyPartDepth.Outside);

            girl.health.AddHediff(Defs.Subj_PunishTheGirl_Hediff, bodyrecord);

            Utils.ThrowMetaIconF(girl.Position, girl.Map, Defs.Subj_NoHeart_Fleck);

            var subjHediff = girl.Subjugate_Hediff();

            var gainMemory = BeatingRating == 0f;
            BeatingRating = Mathf.Min(100f, BeatingRating + (subjHediff.Severity * 10f));
            if (gainMemory)
            {
                Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(Defs.Subj_GotPunished_Thought);
                girl.needs.mood.thoughts.memories.TryGainMemory(thought);
            }

            if (NeedsPunishment && this.punishLimit < 10)
            {
                this.punishLimit++;
                girl.Subjugate_Hediff().AddSeverity(.01f);

            }

            this.lastTimePunished = Find.TickManager.TicksGame;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Pawn.TryGet_PussyShockRodRemote(out var _))
            {
                yield return new Command_Action
                {
                    defaultLabel = "Toggle Pussy Shocker",
                    defaultDesc = "Activate pussy shock rod.",
                    icon = ContentFinder<Texture2D>.Get("shockerRemote"),
                    action = delegate
                    {

                        SoundDefOf.Click.PlayOneShotOnCamera();
                        InMapTargeter.BeginTargeting(new TargetingParameters()
                        {
                            canTargetPawns = true,
                            canTargetBuildings = false,
                            neverTargetHostileFaction = true,
                            canTargetItems = false,
                            thingCategory = ThingCategory.Pawn,
                            validator = delegate (TargetInfo target)
                            {


                                if (!target.HasThing)
                                {
                                    return false;
                                }
                                if (target.Thing is Pawn p)
                                {
                                    Log.Message($"distance: {Pawn.Position.DistanceTo(target.Thing.Position)}");
                                    if (Pawn.Position.DistanceTo(target.Thing.Position) > 12f)
                                    {
                                        return false;
                                    }

                                    if (p.TryGet_PussyShockRod(out var _))
                                    {
                                        return true;
                                    }
                                    return false;
                                }
                                return false;
                            }
                        }, delegate (LocalTargetInfo target)
                        {
                            var girl = target.Pawn;
                            if (girl.TryGet_PussyShockRod(out var _, out var pussyShockComp))
                            {
                                if (pussyShockComp.IsShocking)
                                {
                                    pussyShockComp.ShockOff();
                                }
                                else
                                {
                                    pussyShockComp.ShockOn();
                                }
                            }
                        }, this.parent);
                    }
                };

            }

            if (Pawn.TryGet_Subjugate_Comp(out var comp) && comp.GirlShouldHave == Defs.Subj_PussyShockRod_Item.defName)
            {
                yield return new Command_Action
                {
                    defaultLabel = "No pussy shock rod",
                    defaultDesc = "Remove this girls pussy shock rod.",
                    icon = ContentFinder<Texture2D>.Get("removepussyrod"),
                    action = delegate
                    {
                        RemovePussyRody();
                    }
                };
            }
            if (Pawn.TryGet_Subjugate_Comp(out comp) && comp.GirlShouldHave == Defs.Subj_Bindings_Item.defName)
            {
                yield return new Command_Action
                {
                    defaultLabel = "No binders",
                    defaultDesc = "Remove binders from this girl.",
                    icon = ContentFinder<Texture2D>.Get("removepussyrod"),
                    action = delegate
                    {
                        RemoveBinders();
                    }
                };
            }

            if (Pawn.gender == Gender.Female && this.NeedsPunishment)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Punish",
                    defaultDesc = "Punish this girl!",
                    icon = ContentFinder<Texture2D>.Get("removepussyrod"),
                    action = delegate
                    {
                        Pawn.NeedsPunishing(true);
                    }
                };
            }

        }


        private void RemovePussyRody()
        {
            (this.parent as Pawn).GetComp<CompSubjugate>().GirlShouldHave = null;
        }
        private void RemoveBinders()
        {
            (this.parent as Pawn).GetComp<CompSubjugate>().GirlShouldHave = null;
        }


    }

}
