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

namespace Subjugate
{
    [HarmonyPatch(typeof(Pawn_GuestTracker), "SetGuestStatus")]
    public class slave_stat_changed
    {
        static FieldInfo PawnFieldInfo = typeof(Pawn_GuestTracker).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        private static Pawn GetPawn(Pawn_GuestTracker instance)
        {
            return (Pawn)PawnFieldInfo.GetValue(instance);
        }
        public static void Prefix(Faction newHost, GuestStatus guestStatus, Pawn_GuestTracker __instance)
        {
            var pawn = GetPawn(__instance);
            CompSubjugate.RemoveFromRepo(pawn);

        }
    }
    [HarmonyPatch(typeof(Pawn), "ChangeKind")]
    public class kind_change
    {
        public static void Prefix(PawnKindDef newKindDef, Pawn __instance)
        {
            faction_change_re_repo.Prefix(null, null, __instance);
        }
    }
    [HarmonyPatch(typeof(Pawn), "SetFaction")]
    public class faction_change_re_repo
    {
        public static void Prefix(Faction newFaction, Pawn recruiter, Pawn __instance)
        {
            CompSubjugate.RemoveFromRepo(__instance);
        }
        
    }


    [HarmonyPatch(typeof(PawnCapacityUtility), "CalculateCapacityLevel")]
    public class calc_mindmerge_capacity
    {
        static string[] caps = new string[] { "Consciousness", "Moving" };
        [HarmonyPostfix]
        public static void postfix(ref float __result, HediffSet diffSet, PawnCapacityDef capacity, List<PawnCapacityUtility.CapacityImpactor> impactors, bool forTradePrice)
        {
            var pawn = diffSet.pawn;
            if (!pawn.IsColonist)
                return;

            if (pawn.gender == Gender.Male && caps.Contains(capacity.defName))
            {
                var comp = CompSubjugate.GetComp(pawn);
                
                if (comp != null)
                {
                    float res = comp.CalcForTheLadies();
                    __result += res;
                }
            }

            if (capacity.defName== "Moving" && pawn.gender==Gender.Female)
            {
                if (diffSet.TryGetHediff(Defs.Subj_PussyShockRod_Hediff, out var h))
                {
                    var hh = h as Hediff_PussyShockRod;
                    if (__result>.7f && hh.HasPussyRod)
                    {
                        __result -= .3f;
                    }
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
