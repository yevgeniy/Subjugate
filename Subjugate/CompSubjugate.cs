
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
        private bool shouldHaveShockrod;
        public bool ShouldHaveShockrod
        {
            get
            {
                return this.shouldHaveShockrod;
            }
            set
            {
                this.shouldHaveShockrod = value;
            }
        }


        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();


            Scribe_Values.Look(ref fortheladies, "subjugate-for-lad");
            Scribe_Values.Look(ref ForTheLadiesMult, "subjugate-for-lad-mult");
            Scribe_Values.Look(ref shouldHaveShockrod, "subjugate-should-have-rod");
            Scribe_Values.Look(ref guiltyTicksLeft, "subjugate-guilty-ticks");
            Scribe_Values.Look(ref totalPussyRodTicksInserted, "subjugate-pussyrod-ticksins");
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


        private int totalPussyRodTicksInserted = 0;
        public int TotalPussyRodTicksInserted { get { return totalPussyRodTicksInserted; } }

        public override void CompTick()
        {
            base.CompTick();
            guiltyTicksLeft--;

            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                if (Pawn.TryGet_GirlNeeds(out bool needInsert, out bool needRemoval))
                {
                    if (needInsert)
                        Pawn.NeedPussyRodInsert();
                    else if (needRemoval)
                        Pawn.NeedPussyRodRemoval();
                }
                else
                {
                    Pawn.NeedPussyRodInsert(false);
                    Pawn.NeedPussyRodRemoval(false);
                }


                if (Pawn.GirlNeedsPunishing())
                {
                    guiltyTicksLeft = GenDate.TicksPerDay;
                }

                if (Pawn.TryGet_PussyShockRod(out var item))
                {
                    totalPussyRodTicksInserted += GenDate.TicksPerHour;
                }

                if (Pawn.IsSlaveOfColony && Pawn.gender==Gender.Female)
                    Pawn.Subjugate_Hediff().AddSeverity(.01f / 12f);

            }

        }
        public override string CompInspectStringExtra()
        {
            return NeedsPunishment ? "NEEDS PUNISHING!" : "";
        }
        private int guiltyTicksLeft = 0;
        public bool NeedsPunishment => guiltyTicksLeft > 0;

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
                                    if (Pawn.Position.DistanceTo(target.Thing.Position)>12f)
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

            if ( Pawn.TryGet_Subjugate_Comp(out var comp) && comp.ShouldHaveShockrod)
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

        }


        private void RemovePussyRody()
        {
            (this.parent as Pawn).GetComp<CompSubjugate>().ShouldHaveShockrod = false;
        }

    }

}
