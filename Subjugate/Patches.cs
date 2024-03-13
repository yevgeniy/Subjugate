using Subjugate.SubjucationPerks;
using HarmonyLib;
using RimWorld;
using Subjugate.SubjucationPerks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Subjugate;
using Verse.Sound;

namespace Adjustments
{
    [HarmonyPatch(typeof(SkillRecord), "Learn")]
    public class subjugated_ladies_distribute_depricated_skills
    {
        public static void Prefix(ref float xp, bool direct, SkillRecord __instance)
        {
            var comp = CompSubjugate.GetComp(__instance.Pawn);
            if (comp!=null && comp.Level>0)
            {
                var amt = comp.xp.TryExtractXP(__instance.def.defName, xp);
                if (amt>0)
                    Log.Message(__instance.Pawn + " norm:" + xp + " amt:" + amt);
                xp += amt;
            }

        }
    }

    [HarmonyPatch(typeof(SkillRecord), "GetLevel")]
    public class subjugated_ladies_have_a_skill_cap_on_mining_and_crafting
    {
        public static void Postfix(bool includeAptitudes, SkillRecord __instance, ref int __result)
        {
            var pawn = __instance.Pawn;
            var comp = CompSubjugate.GetComp(pawn);
            if (comp!=null)
            {
                int skillcap=-1;
                var hasSkillCap = comp.Perks.Any(v => v.HasSkillCap(__instance.def, ref skillcap));
                if (hasSkillCap )
                {
                    __result = Mathf.Min(skillcap, __result);
                }
            }

        }
    }

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

                var comp = CompSubjugate.GetComp(pawn);
                if (comp == null)
                    return;

                if (comp.IsContent)
                {
                    __result = 0;
                    return;
                }

                var percentleft = 1f - comp.ContentRatio;
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
            var comp = CompSubjugate.GetComp(pawn);


            if (comp != null && comp.Level>0)
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

            //if (worker.CurJob.workGiverDef.workType.defName == "Tailoring")
            //{
            //    var hasTailoringTrait = PerkTailoring.HasTailoringPerk(worker);

            //    if (ingredients != null && hasTailoringTrait)
            //        foreach (var i in ingredients)
            //        {
            //            var stackcount = Mathf.Floor(i.stackCount * .2f);
            //            if (stackcount > 0)
            //            {
            //                var thing = ThingMaker.MakeThing(i.def, i.Stuff);
            //                thing.stackCount = (int)stackcount;
            //                GenPlace.TryPlaceThing(thing, worker.Position, worker.Map, ThingPlaceMode.Near);
            //            }
            //        }
            //}
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    public class subjugated_ladies_can_only_craft_tailoring
    {
        public static string[] disswork = new string[] { "Smithing", "Crafting" };
        public static WorkTypeDef[] dissdefs;
        [HarmonyPostfix]
        public static void patcher(Pawn __instance, ref List<WorkTypeDef> __result)
        {
            if (PerkTailoringConstraint.HasTailoringPerk(__instance))
            {
                dissdefs = dissdefs ?? DefDatabase<WorkTypeDef>.AllDefs.Where(v => disswork.Contains(v.defName)).ToArray();

                foreach (var i in dissdefs)
                    __result.AddDistinct(i);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    public class subjugated_ladies_hate_wearing_armor
    {
        [HarmonyPrefix]
        public static bool prepatcher(Apparel apparel, Pawn_ApparelTracker __instance)
        {
            if (apparel.def.tradeTags.Any(v => v == "Armor") && PerkHatesArmor.HatesArmor(__instance.pawn))
            {
                __instance.pawn.needs.mood.thoughts.memories.TryGainMemory(Defs.SubjugatePutOnArmour);
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
            if (__instance.def != Defs.Subjugated)
                return;

            var comp = CompSubjugate.GetComp(pawn);

            __result += "\n\n" + comp.ContentStr;

            var explanations = comp.Perks.Select(v => v.Describe(pawn)).ToList();
            __result += "\n\n" + string.Join("\n", explanations);
        }
    }

    [HarmonyPatch(typeof(SkillRecord), "CalculatePermanentlyDisabled")]
    public class disabling_skill_based_on_perks_for_perm
    {
        [HarmonyPrefix]
        public static bool prefixer(SkillRecord __instance, ref bool __result)
        {
            return disabled_or_enable_skills.prefixer(__instance, ref __result);
        }

    }

    [HarmonyPatch(typeof(SkillRecord), "CalculateTotallyDisabled")]
    public class disabled_or_enable_skills
    {
        [HarmonyPrefix]
        public static bool prefixer(SkillRecord __instance, ref bool __result)
        {
            var comp = CompSubjugate.GetComp(__instance.Pawn);

            if (comp != null)
            {

                var forcedisable = comp.Perks.Any(v => v.IsSkillDisabled(__instance));
                if (forcedisable)
                {

                    __result = true;
                    return false;
                }


                var forceenable = comp.Perks.Any(v => v.IsSkillEnabled(__instance));
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
            if (hediff != null)
            {
                var pawn = __instance.pawn;
                if (pawn.gender == Gender.Female && dinfo.Instigator is Pawn bypawn)
                {
                    var comp = CompSubjugate.GetComp(pawn);
                    if (comp != null)
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
                if (pawn.gender == Gender.Female /*&& pawn.guilt.IsGuilty*/)
                {
                    var comp = pawn.GetComp<CompSubjugate>();
                    if (comp != null)
                    {
                        comp.Prime();
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
