using Subjugate.SubjucationPerks;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Subjugate
{

    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public class tailoring_by_subjugated_ladies_has_material_left_over
    {
        [HarmonyPrefix]
        public static bool patch(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver, Precept_ThingStyle precept, ThingStyleDef style, int? overrideGraphicIndex)
        {

            if (worker.CurJob.workGiverDef.workType.defName=="Tailoring")
            {
                var hasTailoringTrait = PerkTailoring.HasTailoringPerk(worker);

                if (ingredients != null && hasTailoringTrait)
                    foreach(var i in ingredients)
                    {
                        var stackcount = Mathf.Floor(i.stackCount * .2f);
                        if (stackcount > 0)
                        {
                            var thing = ThingMaker.MakeThing(i.def, i.Stuff);
                            thing.stackCount = (int)stackcount;
                            GenPlace.TryPlaceThing(thing, worker.Position, worker.Map, ThingPlaceMode.Near);
                        }
                    }
            }
            return true;
        }
    }










}
