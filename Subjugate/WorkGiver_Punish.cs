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
using Verse.AI.Group;

namespace Subjugate
{
    public class WorkGiver_Punish : WorkGiver_Scanner
    {

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var needPunishing = Subjugate.GirlsNeedingPunishment;
            Log.Message($"potential punishment: {needPunishing.Count}");
            

            foreach(var i in needPunishing) yield return i;
        }

        public bool PreCheck(Pawn warden, Thing subject)
        {
            if (!(subject is Pawn girl))
            {
                return false;
            }

            if (girl.Drafted)
            {
                return false;
            }

            if (girl.IsSlaveOfColony)
            {
                if (!warden.CanReserve(girl))
                    return false;
                if (!warden.CanReach(girl, PathEndMode.Touch, Danger.None))
                    return false;

                return true;

            }
            return false;

        }
        public override bool HasJobOnThing(Pawn warden, Thing subject, bool forced = false)
        {
            if (warden == subject)
                return false;

            return PreCheck(warden, subject);
        }

        public override Job JobOnThing(Pawn warden, Thing t, bool forced = false)
        {
            if (!PreCheck(warden, t))
            {
                return null;
            }
            var girl = t as Pawn;
            Log.Message($"looking to punish: {warden} {girl}");

            var job = new PunishGirlJob
            {
                def = new JobDef
                {
                    driverClass = typeof(JobDriver_PunishGirl),
                    label = "Punish Girl"
                },
                targetA = girl,
                count = 1,
            };

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

