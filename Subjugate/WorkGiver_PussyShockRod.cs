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
    public class WorkGiver_PussyShockRod : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var needInsert = Subjugate.GirlsNeedingInsert;
            var needRemoval = Subjugate.GirlsNeedingRemoval;
            Log.Message($"potential: {Subjugate.ReadyPussyShockRods.Count} {needInsert.Count} {needRemoval.Count}");
            if (Subjugate.ReadyPussyShockRods.Count > 0)
            {
                foreach (var i in needInsert) yield return i;
            }

            foreach(var i in needRemoval) yield return i;
        }

        public bool PreCheck(Pawn pawn, Thing t, out bool needInsert, out bool needRemoval, out Thing goodPussyRod)
        {
            needInsert = false;
            needRemoval = false;
            goodPussyRod = null;

            if (!(t is Pawn girl))
            {
                return false;
            }

            if (!PussyRodUtils.TryGet_GirlNeeds(girl, out  needInsert, out  needRemoval))
            {
                Log.Message($"does not need attention {girl}");
                return false;
            }
            Log.Message($"needs {girl} {needInsert} {needRemoval}");

            if (!pawn.CanReserve(girl))
                return false;
            if (!pawn.CanReach(girl, PathEndMode.Touch, Danger.None))
                return false;


            if (needInsert)
            {
                
                goodPussyRod = GetClosestPussyRod(pawn);
                Log.Message($"pussy rod {goodPussyRod}");
                return goodPussyRod!=null;
            }

            if (needRemoval)
                return true;

            return false;

        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn == t)
                return false;

            return PreCheck(pawn, t, out var _, out var __, out var ___);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!PreCheck(pawn, t, out bool needInsert, out bool needRemoval, out var goodPussyRod))
            {
                return null;
            }
            Log.Message($"looking {pawn} {t} {needInsert} {needRemoval} {goodPussyRod}");

            var job = new AttendToGirlJob
            {
                def = new JobDef
                {
                    driverClass = typeof(AttendToGirl),
                    label = "Insert pussy shock rod"
                },
                targetA = goodPussyRod,
                targetB = t,
                count = 1,
                needRemoval=needRemoval,
                needInsert=needInsert
            };
            Subjugate.ReadyPussyShockRods.Remove(goodPussyRod);

            return job;
        }

        private Thing GetClosestPussyRod(Pawn pawn)
        {
            var pussyrod = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
                PathEndMode.Touch, TraverseParms.For(pawn),
                9999f, possibleItem => possibleItem.def.defName == "Subj_PussyShockRod_Item"
                            && pawn.CanReserve(possibleItem)
                            && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, possibleItem, false)
                            && Subjugate.ReadyPussyShockRods.Contains(possibleItem)
                            && !possibleItem.IsForbidden(pawn));

            return pussyrod;
        }
    }


}

