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
using Subjugate;
using Verse.Sound;
using static UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;
using System.Security.Cryptography;
using Verse.AI.Group;

namespace Subjugate
{
    //[HarmonyPatch(typeof(PawnRenderer), "GetDrawParms")]
    //public static class lay_down
    //{
    //    [HarmonyPostfix]
    //    public static void postfix(ref PawnDrawParms __result, Vector3 rootLoc, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
    //    {
    //        if (__result.pawn.gender==Gender.Female)
    //        {
    //            var girl = __result.pawn;
    //            if (girl.health.hediffSet.TryGetHediff(Defs.Subj_ShockTheGirl_Hediff, out var _))
    //            {
    //                Log.Message($"CRAWLING {girl}");
    //                __result.crawling = true;
    //            }
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    //public class slave_stat_changed
    //{
    //    static FieldInfo PawnFieldInfo = typeof(Pawn_GuestTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
    //    private static Pawn GetPawn(Pawn_GuestTracker instance)
    //    {
    //        return (Pawn)PawnFieldInfo.GetValue(instance);
    //    }
    //    public static void Prefix(Faction newHost, GuestStatus guestStatus, Pawn_GuestTracker __instance)
    //    {
    //        var pawn = GetPawn(__instance);
    //        CompSubjugate.RemoveFromRepo(pawn);

    //    }
    //}
    //[HarmonyPatch(typeof(Pawn), "ChangeKind")]
    //public class kind_change
    //{
    //    public static void Prefix(PawnKindDef newKindDef, Pawn __instance)
    //    {
    //        faction_change_re_repo.Prefix(null, null, __instance);
    //    }
    //}
    //[HarmonyPatch(typeof(Pawn), "SetFaction")]
    //public class faction_change_re_repo
    //{
    //    public static void Prefix(Faction newFaction, Pawn recruiter, Pawn __instance)
    //    {
    //        CompSubjugate.RemoveFromRepo(__instance);
    //    }

    //}

    //[HarmonyPatch(typeof(PrisonBreakUtility), "InitiatePrisonBreakMtbDays")]
    //public class prison_break_adjust
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(ref float __result, Pawn pawn, StringBuilder sb, bool ignoreAsleep)
    //    {
    //        if (pawn.health.hediffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var h))
    //        {
    //            var hediff = h as Hediff_PussyShockRod;
    //            var reg = __result;
    //            __result *= hediff.MTBEventDaysMultiplyer();
    //            Log.Message($"prison: {pawn} reg: {reg} adj: {__result}");
    //        }
    //    }

    //}

    //[HarmonyPatch(typeof(SlaveRebellionUtility), "InitiateSlaveRebellionMtbDays")]
    //public class slave_break_adjust
    //{
    //    [HarmonyPostfix]
    //    public static void Postfix(ref float __result, Pawn pawn)
    //    {
    //        if (pawn.health.hediffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var h))
    //        {
    //            var hediff = h as Hediff_PussyShockRod;
    //            var reg = __result;
    //            __result *= hediff.MTBEventDaysMultiplyer();
    //            Log.Message($"slave: {pawn} reg: {reg} adj: {__result}");
    //        }
    //    }

    //}
    //[HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
    //public static class capture_pawn
    //{
    //    public static Pawn CurrenPawn;
    //    public static bool Prefix(Pawn pawn)
    //    {
    //        capture_pawn.CurrenPawn = pawn;
    //        return true;
    //    }
    //}
    //[HarmonyPatch(typeof(ThingFilter), "Allows", new Type[]{typeof(ThingDef) } )]
    //public static class allows_apparel
    //{
    //    public static bool Prefix(ref bool __result, ThingDef def)
    //    {
    //        if (def.thingCategories.Contains(Defs.Subj_SlaveGirl_ThingCategory))
    //        {
    //            __result = false;
    //            return false;
    //        }

    //        return true;
    //    }
    //}


    [HarmonyPatch(typeof(PawnCapacityUtility), "CalculateCapacityLevel")]
    public class cal_capability
    {
        static string[] caps = new string[] { "Consciousness", "Moving", "Talking" };
        [HarmonyPostfix]
        public static void postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity, List<PawnCapacityUtility.CapacityImpactor> impactors, bool forTradePrice)
        {
            var pawn = diffSet.pawn;
            if (!pawn.IsColonist)
                return;

            if (pawn.gender == Gender.Male && caps.Contains(capacity.defName))
            {
                if (pawn.TryGet_Subjugate_Comp(out var comp))
                {
                    float res = comp.CalcForTheLadies();
                    __result += res;
                }
            }
            
            __result = GenMath.RoundedHundredth(__result);
        }
    }

    [HarmonyPatch(typeof(SlaveRebellionUtility), "CanParticipateInSlaveRebellion")]
    public class subjugated_ppl_dont_rebell
    {
        [HarmonyPrefix]
        public static bool patch(Pawn pawn, ref bool __result)
        {

            if (pawn.health.hediffSet.HasHediff(Subjugate.VPEP_Puppet))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(JobMaker), "MakeJob", new Type[] {typeof( JobDef ), typeof( LocalTargetInfo )})]
    public static class detect_wear
    {
        public static void Postfix(ref Job __result, JobDef def, LocalTargetInfo targetA)
        {
            if (def==JobDefOf.Wear)
            {

                if (targetA.HasThing && targetA.Thing.def.thingCategories.Contains(Defs.Subj_Subjugation_ThingCategory))
                {
                    Log.Message($"SOMEONE IS ATTEMPTING OT EQUIP SUBJUGATION APPAREL.");
                    __result = new Job
                    {
                        def = new JobDef
                        {
                            driverClass = typeof(JobDriver_EmptyWear)
                        }
                    };
                }

            }
            else if (def==JobDefOf.RemoveApparel)
            {
                if (targetA.HasThing && targetA.Thing.def.thingCategories.Contains(Defs.Subj_Subjugation_ThingCategory))
                {
                    Log.Message($"SOMEONE IS ATTEMPTING TO REMOVE SUBJUGATION APPAREL.");
                    __result = new Job
                    {
                        def = new JobDef
                        {
                            driverClass = typeof(JobDriver_EmptyWear)
                        }
                    };
                }
            }
        }
    }
    [HarmonyPatch(typeof(JobGiver_GetRest), "TryGiveJob")]
    public static class slaves_take_off_clothing_when_sleeping
    {
        public static void Postfix(ref Job __result, Pawn pawn)
        {
            if (__result!=null)
            {
                AcceptanceReport allowsDrafting = pawn.GetLord()?.AllowsDrafting(pawn) ?? ((AcceptanceReport)true);

                if (pawn.gender == Gender.Female && pawn.IsSlaveOfColony && allowsDrafting && pawn.HasClothingToTakeOff() )
                {
                    __result = new GetNakedToSleepJob
                    {
                        def = new JobDef
                        {
                            driverClass = typeof(JobDriver_GetNakedToSleep),
                            description="Getting naked to sleep",
                            label="Getting naked to sleep"
                        },
                        sleepJob = __result,
                        bed = __result.targetA
                    };
                }
            }

        }
    }

    //[HarmonyPatch(typeof(StoreUtility), "TryFindBestBetterNonSlotGroupStorageFor")]
    //public class pussy_rod_charger
    //{


    //    [HarmonyPrefix]
    //    private static bool TryFindBestBetterNonSlotGroupStorageFor(ref bool __result, Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, out IHaulDestination haulDestination, bool acceptSamePriority = false, bool requiresDestReservation = true)
    //    {
    //        haulDestination = null;
    //        if (t.def.defName== "Subj_PussyShockRod_Item")
    //        {
    //            var building = PussyRodUtils.FindNearestChargeStation(t.Position, t.Map);
    //            haulDestination = building;
    //            if (building!=null)
    //            {
    //                return false;
    //            }
    //            return false;                
    //        }

    //        return true;
    //    }


    //}

    //[HarmonyPatch(typeof(ThingOwnerUtility), "TryGetInnerInteractableThingOwner")]
    //public class pussy_rod_charger_inner_container
    //{

    //    [HarmonyPrefix]
    //    private static bool TryGetInnerInteractableThingOwner(ref ThingOwner __result, Thing thing)
    //    {
    //        if (thing.def.defName=="Subj_PussyShockRodCharger_Item")
    //        {
    //            __result = new CriticalHaulThingOwner(thing, __result);
    //            return false;
    //        }
    //        return true;

    //    }


    //}


    //[HarmonyPatch(typeof(GuestUtility), "GetDisabledWorkTypes")]
    //public class subjugated_ppl_can_do_art_and_research
    //{
    //    private static Pawn GetPawn(Pawn_GuestTracker instance)
    //    {
    //        Type type = typeof(Pawn_GuestTracker);

    //        // Get the private field info
    //        FieldInfo fieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

    //        return (Pawn)fieldInfo.GetValue(instance);

    //    }
    //    [HarmonyPostfix]
    //    public static void postfix(Pawn_GuestTracker guest, ref List<WorkTypeDef> __result)
    //    {
    //        var pawn = GetPawn(guest);

    //        if (pawn.health.hediffSet.HasHediff(Subjugate.VPEP_Puppet))
    //        {
    //            __result.RemoveAll(v => v == WorkTypeDefOf.Research || v.defName == "Art");
    //        }
    //    }
    //}
}
