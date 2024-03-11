using Adjustments.SubjucationPerks;
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

namespace Adjustments
{


    [HarmonyPatch(typeof(StatExtension), "GetStatValue")]
    public class asjust_slave_suppression_rate
    {
        [HarmonyPostfix]
        public static void fixer(Thing thing, StatDef stat, bool applyPostProcess, int cacheStaleAfterTicks, ref float __result)
        {
            if (thing is Pawn pawn)
            {
                if (stat != StatDefOf.SlaveSuppressionFallRate)
                    return;

                var comp = SubjugateComp.GetComp(pawn);
                if (comp == null)
                    return;

                if (comp.IsContent)
                {
                    __result = 0;
                    return;
                }

                var percentleft = 100f - comp.ContentPercent;
                percentleft = percentleft / 100f;

                __result *= percentleft;

            }
        }
    }


    [HarmonyPatch(typeof(SlaveRebellionUtility), "CanParticipateInSlaveRebellion")]
    public class cant_participate_in_rebellion
    {
        [HarmonyPrefix]
        public static bool patch(Pawn pawn, ref bool __result)
        {
            var comp = SubjugateComp.GetComp(pawn);

            if (comp!=null && comp.IsContent)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public class check_thigs_produced_for_tailoring
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

    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    public class check_for_disabled_crafting_work_types
    {
        [HarmonyPostfix]
        public static void patcher(Pawn __instance, ref List<WorkTypeDef> __result)
        {
            if (PerkTailoring.HasTailoringPerk(__instance))
            {
                __result.AddDistinct(WorkTypeDefOf.Smithing);

                var disableDefs = DefDatabase<WorkTypeDef>.AllDefs.Where(v => new string[] { "Smithing", "Crafting" }.Contains(v.defName));

                foreach(var i in disableDefs)
                    __result.AddDistinct(i);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    public class check_for_wearing_armor
    {
        [HarmonyPrefix]
        public static bool prepatcher(Apparel apparel, Pawn_ApparelTracker __instance)
        {
            if (apparel.def.tradeTags.Any(v => v == "Armor") && PerkNegHateArmor.HatesArmor(__instance.pawn) )
            {
                __instance.pawn.needs.mood.thoughts.memories.TryGainMemory(SubjugatedDefs.SubjugatePutOnArmour);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(GuestUtility), "GetDisabledWorkTypes")]
    public class activate_artistic_for_applicable_slaves
    {
        private static Pawn GetPawn(Pawn_GuestTracker instance)
        {
            Type type = typeof(Pawn_GuestTracker);

            // Get the private field info
            FieldInfo fieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

            return (Pawn)fieldInfo.GetValue(instance);

        }
        [HarmonyPostfix]
        public static void postfix(Pawn_GuestTracker guest, ref List<WorkTypeDef> __result)
        {
            var pawn = GetPawn(guest);
            if (pawn.gender != Gender.Female)
                return;

            var shouldDoArt = PerkArtistic.ShouldDoArt(pawn);
            if (shouldDoArt)
                __result.RemoveAll(v => v.defName == WorkTypeDefOf.Art.defName);
        }
    }

    [HarmonyPatch(typeof(Trait), "TipString")]
    public class trait_should_include_perk_descriptions_and_subjugation_notes
    {
        [HarmonyPostfix]
        public static void postfix(Trait __instance, ref string __result, Pawn pawn)
        {
            if (__instance.def != SubjugatedDefs.Subjugated)
                return;

            var comp = SubjugateComp.GetComp(pawn);


            var content = comp.ContentStr;

            __result += "\n\n" + content;

            var explanations = comp.Perks.Select(v => v.Describe(pawn)).ToList();
            __result = __result + "\n\n" + string.Join("\n", explanations);
        }
    }

    [HarmonyPatch(typeof(SkillRecord), "CalculatePermanentlyDisabled")]
    public class disabling_skill_based_on_perks_for_perm
    {
        [HarmonyPrefix]
        public static bool prefixer(SkillRecord __instance, ref bool __result)
        {
            return disabling_skills_based_on_perks.prefixer(__instance, ref __result);
        }

    }

    [HarmonyPatch(typeof(SkillRecord), "CalculateTotallyDisabled")]
    public class disabling_skills_based_on_perks
    {
        [HarmonyPrefix]
        public static bool prefixer(SkillRecord __instance, ref bool __result)
        {
            var comp = SubjugateComp.GetComp(__instance.Pawn);

            if (comp != null)
            {

                var forcedisable = comp.Perks.Any(v => v.IsDisabled(__instance));
                if (forcedisable)
                {

                    __result = true;
                    return false;
                }


                var forceenable = comp.Perks.Any(v => v.IsEnabled(__instance));
                if (forceenable)
                {
                    __result = false;
                    return false;
                }

                return true;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(HediffSet), "AddDirect")]
    public class register_severity_for_beating
    {
        [HarmonyPrefix]
        public static bool Patch(Hediff hediff, DamageInfo dinfo, DamageWorker.DamageResult damageResult, HediffSet __instance)
        {
            if (hediff!=null)
            {
                var pawn = __instance.pawn;
                if (pawn.gender == Gender.Female && dinfo.Instigator is Pawn bypawn)
                {
                    var comp = SubjugateComp.GetComp(pawn);
                    if (comp != null )
                    {
                        comp.RegisterSeverity(hediff.Severity, bypawn);
                    }
                }

            }

            return true;
        }

    }

    //public void SetGuestStatus(Faction newHost, GuestStatus guestStatus = 0)
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    public class register_prisoner_start
    {
        [HarmonyPrefix]
        public static bool Patch(Faction newHost, GuestStatus guestStatus, Pawn_GuestTracker __instance)
        {
            if (!__instance.IsPrisoner && guestStatus == GuestStatus.Prisoner)
            {
                var pawn = GetPawn(__instance);
                if (pawn.gender==Gender.Female /*&& pawn.guilt.IsGuilty*/)
                {
                    var comp = pawn.GetComp<SubjugateComp>();
                    if (comp!=null)
                    {
                        comp.ActivateSubjugation();
                    }
                }

            }

            return true;
        }

        private static Pawn GetPawn(Pawn_GuestTracker instance)
        {
            Type type = typeof(Pawn_GuestTracker);

            // Get the private field info
            FieldInfo fieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

            return (Pawn)fieldInfo.GetValue(instance);

        }
    }
}
