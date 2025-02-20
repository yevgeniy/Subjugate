
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

        static CompSubjugate()
        {
            /*add subjugate comp to all defs having a race */
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null && thingDef.race.Humanlike))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(CompSubjugate) });
            }
        }

        public CompSubjugate()
        {
        }

        public long ticks;


        public override void PostDeSpawn(Map map)
        {
            RemoveFromRepo(this);
            base.PostDeSpawn(map);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();


            Scribe_Values.Look(ref fortheladies, "subjugate-for-lad");
            Scribe_Values.Look(ref ForTheLadiesMult, "subjugate-for-lad-mult");

        }



        public static CompSubjugate GetComp(Pawn pawn)
        {

            if (!Repo.ContainsKey(pawn))
            {
                var comp = pawn.GetComp<CompSubjugate>();

                if (comp == null)
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, null, null });
                }
                else if (pawn.gender == Gender.Male && pawn.Ideo != null && pawn.Ideo.HasPrecept(Defs.Subj_SubjugateAllWomen_Precept))
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, comp, null });
                }
                else if (pawn.gender == Gender.Female)
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, null, comp });
                }
                else
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, null, null });
                }

            }
            return Repo[pawn][(byte)pawn.gender]; /*0:none, 1:male, 2:female*/
        }

        public static void RemoveFromRepo(Pawn pawn)
        {
            if (!Repo.ContainsKey(pawn))
                return;

            Repo.Remove(pawn);
        }
        public static void RemoveFromRepo(CompSubjugate comp)
        {
            Repo.RemoveAll(keyval =>
            {
                return keyval.Value.Contains(comp);
            });
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


        public override void CompTick()
        {
            base.CompTick();

            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {

                Log.Message("girl should have pussy insert " + this.parent);
                if (PussyRodUtils.GirlNeedsAttention(this.parent as Pawn, out bool needInsert, out bool needRemoval))
                {
                    Log.Message($"girl needs attention {this.parent} {needInsert} {needRemoval}");

                    if (needInsert && !Subjugate.GirlsNeedingInsert.Contains(this.parent))
                        Subjugate.GirlsNeedingInsert.Add(this.parent as Pawn);
                    else if (needRemoval && !Subjugate.GirlsNeedingRemoval.Contains(this.parent))
                        Subjugate.GirlsNeedingRemoval.Add(this.parent as Pawn);
                }

            }

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var pawn = this.parent as Pawn;
            if (pawn.inventory.innerContainer.ContainsAny(v=>v.def.defName== "Subj_PussyShockRodRemote_Item"))
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
                                    if (p.health.hediffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var h))
                                    {
                                        var hediff = h as Hediff_PussyShockRod;
                                        if (hediff.HasPussyRod)
                                        {
                                            return true;
                                        }
                                        return false;
                                    }
                                    return false;
                                }
                                return false;
                            }
                        }, delegate (LocalTargetInfo target)
                        {
                            var girl = target.Pawn;
                            if (girl.health.hediffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var h))
                            {
                                var hediff = h as Hediff_PussyShockRod;
                                if (hediff.HasPussyRod)
                                {
                                    if (hediff.IsShocking)
                                    {
                                        hediff.ShockOff();
                                    }
                                    else
                                    {
                                        hediff.ShockOn();
                                    }
                                    
                                }

                            }
                        }, this.parent);
                    }
                };

            }

            if (Subjugate.ShouldHaveShockRod.Contains(this.parent))
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
            Subjugate.ShouldHaveShockRod.Remove(this.parent as Pawn);
        }

    }

}
