using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public static class Adjustments
    {
        public static bool HasCombatExtended;
        public static bool HasAllowTool;
        public static bool HassVanillaSkillMod;
        

        public static PropertyInfo SelectedAmmoPropInfo = null;
        
        
        
        public static StatDef ReloadSpeed = null;

        static Adjustments()
        {
            Log.Message("ADJUSTMENTS STARTED.");


            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            /* find 'haul urgently' class */
            var compAmmoUserType = assemblies.SelectMany(v => v.GetTypes()).FirstOrDefault(v => v.Name == "CompAmmoUser");
            if (compAmmoUserType != null)
            {
                HasCombatExtended = true;
                ReloadSpeed = StatDef.Named("ReloadSpeed");

            }

            var classType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "Designator_HaulUrgently");
            if (classType != null)
            {
                HasAllowTool = true;
            }

            classType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "SkillsMod");
            if (classType != null)
                HassVanillaSkillMod = true;


            Log.Message("HAS ALLOW TOOL: " + HasAllowTool);
            Log.Message("HAS CE: " + HasCombatExtended);

            Harmony.DEBUG = true;  // Enable Harmony Debug
            Harmony harmony = new Harmony("nimm.adjustments");

            if (HasCombatExtended)
                Pawn_CarryTracker_TryDropCarriedThing.Wire(harmony);

            ApplyOnPawn_CheckSurgeryFail.Wire(harmony);

            harmony.PatchAll();


            Log.Message("ADJUSTMENTS PATCHED.");

            AttachComps();
        }

        private static void AttachComps()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(BrandComp) });
            }
        }
    }


}