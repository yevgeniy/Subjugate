using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Subjugate
{


    [StaticConstructorOnStartup]
    public class CompInsertPussyShockRod : ThingComp
    {

        static CompInsertPussyShockRod()
        {
            var def = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(v => v.defName == "Subj_PussyShockRod_Item");
            if (def != null)
            {
                def.comps.Add(new CompProperties
                {
                    compClass = typeof(CompInsertPussyShockRod)
                });
                def.tickerType = TickerType.Normal;
            }

        }
        public CompInsertPussyShockRod()
        {
            this.proxy = new ShockRodProxy();
        }

        public ShockRodProxy proxy;

        public CompProperties Props => (CompProperties)props;

        private bool isShocking = false;
        public bool IsShocking => this.isShocking;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            yield return new Command_Action
            {
                defaultLabel = "Insert",
                defaultDesc = "Insert Pussy Shock Rod into a girl.",
                icon = ContentFinder<Texture2D>.Get("CavityShocker"),
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
                            if (target.Thing is Pawn pawn)
                            {
                                if (pawn.gender == Gender.Female && (pawn.IsColonist || pawn.IsSlaveOfColony || pawn.IsPrisoner))
                                {
                                    return true;
                                }
                                return false;
                            }
                            return false;
                        }
                    }, delegate (LocalTargetInfo target)
                    {
                        target.Pawn.GetComp<CompSubjugate>().ShouldHaveShockrod = true;
                        Log.Message($"target {target.Pawn}");
                    }, this.parent);
                }
            };

            var gizs = base.CompGetGizmosExtra();
            foreach (var i in gizs)
            {
                yield return i;
            }
        }

        
        public float Charge { get { return this.proxy.charge; } }
        public override string CompInspectStringExtra()
        {
            var s = base.CompInspectStringExtra() + "Charged: " + (this.proxy.charge * 100f) + "%";

            return s;
        }

        public Pawn Wearer=>this.parent is Apparel apparel ? apparel.Wearer : null;
        

        public override void CompTick()
        {
            base.CompTick();

            if (Find.TickManager.TicksGame % (GenDate.TicksPerHour / 5) == 0 && IsShocking && Wearer!=null)
            {
                if (!TryShock())
                {
                    ShockOff();
                }
                
            }

            this.proxy.Tick(this.parent);
        }

        public bool TryShock()
        {
            
            if (this.proxy.TryShock()) {
                var torso = Wearer.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso);
                Wearer.health.AddHediff(Defs.Subj_ShockTheGirl_Hediff, torso);

                Utils.ThrowMetaIconF(Wearer.Position, Wearer.Map, Defs.Subj_NoHeart_Fleck);

                if (Wearer.TryGet_PussyShockRod_Hediff(out var shockRodHediff))
                {
                    shockRodHediff.Shock();
                }

                if (Wearer.TryGet_Subjugate_Comp(out var subjComp) && subjComp.NeedsPunishment)
                {
                    Wearer.Subjugate_Hediff().AddSeverity(.01f);
                }

                return true;
            }
            return false;
        }

        public void ShockOff()
        {
            this.isShocking = false;
        }

        public void ShockOn()
        {
            this.isShocking = true;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Deep.Look(ref this.proxy, "comp-ins-puss-shock-rod-proxy", new object[] { });
            Scribe_Values.Look(ref isShocking, "comp-ins-puss-shock-rod-isshocking");

        }

    }

    public class ShockRodProxy : IExposable
    {
        public float charge = .1f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.charge, "pussrodcomp-charge");
        }

        private static float TicksToFullCharge = GenDate.TicksPerHour * 3;
        private static float TicksToFullDischarge = GenDate.TicksPerDay * 2;
        private static float ChargePerTick = 1f / TicksToFullCharge;
        private static float DischargePerTick = 1f / TicksToFullDischarge;

        public void Tick(ThingWithComps shockRod=null)
        {
            var shelf = shockRod?.StoringThing();


            if (shelf != null && shelf.TryGetComp<CompPowerTrader>(out var powerComp) && powerComp.PowerOn)
            {
                this.charge = Mathf.Min(1f, this.charge + ChargePerTick);
            }
            else
            {
                this.charge = Mathf.Max(0f, this.charge - DischargePerTick);
            }

            if (shockRod!=null)
            {
                if (this.charge > .3f)
                {
                    if (!Subjugate.ReadyPussyShockRods.Contains(shockRod))
                        Subjugate.ReadyPussyShockRods.Add(shockRod);
                }
                else
                {
                    if (Subjugate.ReadyPussyShockRods.Contains(shockRod))
                        Subjugate.ReadyPussyShockRods.Remove(shockRod);
                }
            }
            
        }
        public bool TryShock()
        {
            if (this.charge > .0f)
            {
                this.charge -= .05f;
                return true;
            }

            this.charge = 0f;
            return false;
        }
    }

}
