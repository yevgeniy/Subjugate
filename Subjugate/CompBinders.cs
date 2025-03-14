using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Subjugate
{
    [StaticConstructorOnStartup]
    public class CompBinders : ThingComp
    {
        static CompBinders()
        {
            var def = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(v => v.defName == "Subj_Bindings_Item");
            if (def != null)
            {
                def.comps.Add(new CompProperties
                {
                    compClass = typeof(CompBinders)
                });
                def.tickerType = TickerType.Normal;
            }

        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            yield return new Command_Action
            {
                defaultLabel = "Bind",
                defaultDesc = "Bind girl.",
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
                        target.Pawn.GetComp<CompSubjugate>().GirlShouldHave = Defs.Subj_Bindings_Item.defName;
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
        public override string CompInspectStringExtra()
        {
            return "BOUND";
        }

    }
}
