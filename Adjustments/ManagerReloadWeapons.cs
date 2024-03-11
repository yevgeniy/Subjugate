using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class ManagerReloadWeapons: MapComponent
    {
        private static HashSet<ThingWithComps> weaponsInStorage=new HashSet<ThingWithComps>();
        

        public static void AddWeapon(ThingWithComps t)
        {
            weaponsInStorage.Add(t);
        }
        public static void RemoveWeapon(ThingWithComps t)
        {
            weaponsInStorage.Remove(t);
        }

        public static bool IsThingInConsideration(ThingWithComps thing)
        {
            return weaponsInStorage.Contains(thing);
        }
        public static IEnumerable<ThingWithComps>  ConsiderWeapons()
        {
            if (weaponsInStorage.Count() == 0)
                return null;

            var mapWeapons = weaponsInStorage.Where(v => v.Map == Find.CurrentMap).ToList();

            if (mapWeapons.Count == 0)
                return null;

            var areaManager = new AreaManager(Find.CurrentMap);
            
            foreach (var t in mapWeapons)
            {
                var gun = new GunProxy(t);
                /* clean up despawned weapons */
                if (!gun.Thing.Spawned)
                {
                    RemoveWeapon(gun.Thing);
                    continue;
                }


                /* Removed weapons not in home zone */
                var homeArea = gun.Thing.Map.areaManager.Home;
                if (homeArea == null)
                {
                    Log.Error("NO HOME AREA?");
                    RemoveWeapon(gun.Thing);
                    continue;
                }
                if (!homeArea.ActiveCells.Any(v => v == gun.Thing.Position))
                {
                    RemoveWeapon(gun.Thing);
                    continue;
                }

                if (!gun.HasMagazine)
                {
                    RemoveWeapon(gun.Thing);
                    continue;
                }

                if (gun.CurrentMagCount==gun.TotalMagCount)
                {
                    RemoveWeapon(gun.Thing);
                    continue;
                }
            }

            return weaponsInStorage.Where(v => v.Map == Find.CurrentMap);
        }

        static ManagerReloadWeapons()
        {

        }
        public ManagerReloadWeapons(Map map):base(map)
        { 
        
        }

        
    }
}
