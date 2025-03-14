using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;
using Verse.AI;

namespace Subjugate
{
    public class WorkGiver_GirlNeedsAttending : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            
            return Subjugate.GirlNeedsAttending(pawn);

        }

        public bool PreCheck(Pawn pawn, Thing t, out bool needShockRod, out bool needBinders, out bool needRemoval, out Thing goodNeedItem)
        {
            needShockRod = false;
            needRemoval = false;
            needBinders = false;
            goodNeedItem = null;

            if (!(t is Pawn girl))
            {
                return false;
            }

            if (!Utils.TryGet_GirlNeeds(girl, out  needShockRod, out needBinders, out needRemoval))
            {
                Log.Message($"does not need attention {girl}");
                return false;
            }
            Log.Message($"needs {girl} {needShockRod} {needBinders} {needRemoval}");

            if (!pawn.CanReserve(girl))
                return false;
            if (!pawn.CanReach(girl, PathEndMode.Touch, Danger.None))
                return false;


            if (needShockRod)
            {   
                goodNeedItem = GetClosestPussyRod(pawn);
                Log.Message($"pussy rod {goodNeedItem}");
                return goodNeedItem!=null;
            }

            if (needBinders)
            {
                goodNeedItem = GenClosesBinders(pawn);
                Log.Message($"binders {goodNeedItem}");
                return goodNeedItem != null;
            }

            if (needRemoval)
                return true;

            return false;

        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn == t)
                return false;

            return PreCheck(pawn, t, out var _, out var __, out var ___, out var j1);
        }

        public override Job JobOnThing(Pawn warden, Thing t, bool forced = false)
        {
            if (!PreCheck(warden, t, out bool needShockRod, out bool needBinding, out bool needRemoval, out var goodItem))
            {
                Log.Error($"{warden}! No job: {needShockRod} {needBinding} {needRemoval} {goodItem}");
                return null;
            }
            Log.Message($"looking {warden} {t} {needShockRod} {needBinding} {needRemoval} {goodItem}");

            var job = new AttendToGirlJob
            {
                def = new JobDef
                {
                    driverClass = typeof(JobDriver_AttendToGirl),
                    label = "Attending to slave girl needs"
                },
                targetA = goodItem,
                targetB = t,
                count = 1,
                needRemoval=needRemoval,
                needInstall=needShockRod || needBinding
            };

            return job;
        }
        
        private Thing GetClosestPussyRod(Pawn pawn)
        {
            var item = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
                PathEndMode.Touch, TraverseParms.For(pawn),
                9999f, possibleItem => possibleItem.def.defName == "Subj_PussyShockRod_Item"
                            && pawn.CanReserve(possibleItem)
                            && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, possibleItem, false)
                            && Subjugate.ReadyPussyShockRods.Contains(possibleItem)
                            && !possibleItem.IsForbidden(pawn));

            return item;
        }
        private Thing GenClosesBinders(Pawn pawn)
        {
            var item = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
                PathEndMode.Touch, TraverseParms.For(pawn),
                9999f, possibleItem => possibleItem.def.defName == "Subj_Bindings_Item"
                            && pawn.CanReserve(possibleItem)
                            && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, possibleItem, false)
                            && !possibleItem.IsForbidden(pawn));

            return item;
        }
    }


}

