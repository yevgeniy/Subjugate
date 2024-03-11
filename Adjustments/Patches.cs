using HarmonyLib;
using RimWorld;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Adjustments
{
    

    /* body part items on failed surgeries are not destroyed */
    public class ApplyOnPawn_CheckSurgeryFail
    {
        public static void Wire(Harmony harmony)
        {
            var methInfo = typeof(Recipe_Surgery).GetMethod("CheckSurgeryFail", BindingFlags.NonPublic | BindingFlags.Instance);
            harmony.Patch(methInfo, postfix: new HarmonyMethod(typeof(ApplyOnPawn_CheckSurgeryFail), nameof(Postfix)));
        }

        public static void Postfix(ref bool __result, Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
        {
            if (__result)
            {
                var ingBodyPart = ingredients.FirstOrDefault(v => v.def.thingCategories.Any(vv => vv.defName.Contains("BodyParts")));
                if (ingBodyPart!=null)
                {
                    var thing = ThingMaker.MakeThing(ingBodyPart.def, ingBodyPart.Stuff);
                    GenSpawn.Spawn(thing, surgeon.Position, surgeon.Map);
                }
            }
        }
    }

    /* Register guns placed in an inventory */
    public class Pawn_CarryTracker_TryDropCarriedThing
    {
        public static void Wire(Harmony harmony)
        {
            
            var methInfo = typeof(Pawn_CarryTracker).GetMethod("TryDropCarriedThing"
                , new Type[] { typeof(IntVec3), typeof(ThingPlaceMode), typeof(Thing).MakeByRefType(), typeof(Action<Thing, int>) });
            
            harmony.Patch(methInfo, postfix: new HarmonyMethod(typeof(Pawn_CarryTracker_TryDropCarriedThing), nameof(Postfix)));
            
        }
        //HaulAIUtility
        //public static void UpdateJobWithPlacedThings(Job curJob, Thing th, int added)

        public static void Postfix(IntVec3 dropLoc, ThingPlaceMode mode, Thing resultingThing, Pawn_CarryTracker __instance, ref bool __result)
        {
            if (!__result)
                return;
            var thing = resultingThing;

            if (thing != null && thing is ThingWithComps compsThing )
            { 

                var gun = new GunProxy(compsThing);
                var comp = gun.CompAmmoUser;

                if (comp!=null)
                {
                    ManagerReloadWeapons.AddWeapon(compsThing);
                }

            }

        }
    }
}


//SlotGroup slotGroup = pawn.Map.haulDestinationManager.SlotGroupAt(cell);