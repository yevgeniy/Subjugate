using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Subjugate
{
    [StaticConstructorOnStartup]
    public class Subjugate : MapComponent
    {
        public static bool HasVanillaSkillMod;

        public static HediffDef VPEP_Puppet;

        HashSet<Pawn> girlsNeedingInsert = new HashSet<Pawn>();
        HashSet<Pawn> girlsNeedingRemoval = new HashSet<Pawn>();
        HashSet<Thing> readyPussyShockRods = new HashSet<Thing>();

        public static HashSet<Pawn> GirlsNeedingInsert
        {
            get
            {
                return Find.CurrentMap.GetComponent<Subjugate>().girlsNeedingInsert;
            }
        }
        public static HashSet<Pawn> GirlsNeedingRemoval
        {
            get
            {
                return Find.CurrentMap.GetComponent<Subjugate>().girlsNeedingRemoval;
            }
        }
        public static HashSet<Thing> ReadyPussyShockRods
        {
            get
            {
                return Find.CurrentMap.GetComponent<Subjugate>().readyPussyShockRods;
            }
        }

        public static Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
        static Subjugate()
        {
            VPEP_Puppet = DefDatabase<HediffDef>.AllDefs.FirstOrDefault(v => v.defName == "VPEP_Puppet");

            Log.Message("Subjugate STARTED.");
            var classType = Assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "SkillsMod");
            if (classType != null)
                HasVanillaSkillMod = true;

            Harmony.DEBUG = true;  // Enable Harmony Debug
            Harmony harmony = new Harmony("nimm.Subjugate");


            harmony.Patch(original: AccessTools.Method(typeof(Targeter), nameof(InMapTargeter.TargeterOnGUI)),
                postfix: new HarmonyMethod(typeof(Subjugate),
                nameof(DrawTargeters)));
            harmony.Patch(original: AccessTools.Method(typeof(Targeter), nameof(InMapTargeter.ProcessInputEvents)),
                postfix: new HarmonyMethod(typeof(Subjugate),
                nameof(ProcessTargeterInputEvents)));
            harmony.Patch(original: AccessTools.Method(typeof(Targeter), nameof(InMapTargeter.TargeterUpdate)),
                postfix: new HarmonyMethod(typeof(Subjugate),
                nameof(TargeterUpdate)));
            harmony.Patch(original: AccessTools.Method(typeof(Targeter), nameof(InMapTargeter.StopTargeting)),
                postfix: new HarmonyMethod(typeof(Subjugate),
                nameof(TargeterStop)));




            harmony.PatchAll();

            Log.Message("Subjugate PATCHED.");
        }

        public Subjugate(Map map) : base(map)
        {
        }

        private static void DrawTargeters()
        {

            Targeters.OnGUITargeters();
        }
        private static void ProcessTargeterInputEvents()
        {

            Targeters.ProcessTargeterInputEvents();
        }
        private static void TargeterUpdate()
        {

            Targeters.UpdateTargeters();
        }
        private static void TargeterStop()
        {

            Targeters.StopAllTargeters();
        }

        public override void ExposeData()
        {
            base.ExposeData();

        }


    }





}