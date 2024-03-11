using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Adjustments
{
    public class WorkGiver_ReloadInStorage: WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.OnCell;
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            if (!Adjustments.HasCombatExtended)
                return null;

            return ManagerReloadWeapons.ConsiderWeapons();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!ManagerReloadWeapons.IsThingInConsideration(t as ThingWithComps))
                return null;

            if (pawn.CurJob != null && pawn.CurJob.def.driverClass == JobDefOf.ReloadInStorage.driverClass)
                return null;

            var gun = new GunProxy(t as ThingWithComps);

            if (!pawn.CanReserveAndReach(gun.Thing, PathEndMode.Touch, Danger.Deadly))
                return null;

            var ammoDef = gun.CurrentAmmo;
            int howMuchNeededForFullReload = gun.TotalMagCount - gun.CurrentMagCount;

            if (ammoDef==null)
            {
                Log.Error("Somehow got a gun with no ammoDef");
                return null;
            }
            if (howMuchNeededForFullReload == 0)
            {
                return null;
            }

            Thing ammoThing = FindClosestReachableAmmoThing(ammoDef, pawn, ThingRequestGroup.Pawn)
                ?? FindClosestReachableAmmoThing(ammoDef, pawn, ThingRequestGroup.HaulableEver);

            if (ammoThing == null)
                return null;

            var job = JobMaker.MakeJob(JobDefOf.ReloadInStorage, ammoThing, gun.Thing);
            job.count = howMuchNeededForFullReload;

            return job;
        }

        private Thing FindClosestReachableAmmoThing(object ammoDef, Pawn pawn, ThingRequestGroup group)
        {
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(group),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                validator: (Thing thing) => thing.def==ammoDef
                    && pawn.CanReserve(thing)
                    && !thing.IsForbidden(pawn.Faction)
            );
        }

    }
}
