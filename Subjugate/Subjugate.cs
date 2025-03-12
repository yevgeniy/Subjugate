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
    public enum NeedType:byte
    {
        Insert=0,
        Remove=1,
    }


    [StaticConstructorOnStartup]
    public class Subjugate : MapComponent
    {
        public static bool HasVanillaSkillMod;

        public static HediffDef VPEP_Puppet;

        public Dictionary<Pawn, NeedType> girlNeed = new Dictionary<Pawn, NeedType>();
        public Dictionary<Pawn, bool> needsPunishing = new Dictionary<Pawn, bool>();


        HashSet<Thing> readyPussyShockRods = new HashSet<Thing>();

        public static List<Pawn> GirlsNeedingInsert=> Find.CurrentMap.GetComponent<Subjugate>().girlNeed.Where(v=>v.Value==NeedType.Insert).Select(v=>v.Key).ToList();
        public static List<Pawn> GirlsNeedingRemoval=> Find.CurrentMap.GetComponent<Subjugate>().girlNeed.Where(v => v.Value == NeedType.Remove).Select(v => v.Key).ToList();
        public static List<Pawn> GirlsNeedingPunishment=> Find.CurrentMap.GetComponent<Subjugate>().needsPunishing.Where(v => v.Value).Select(v => v.Key).ToList();

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