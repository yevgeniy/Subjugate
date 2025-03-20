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
        Install=0,
        Remove=1,
    }


    [StaticConstructorOnStartup]
    public class Subjugate : MapComponent
    {
        public static bool HasVanillaSkillMod;

        public static HediffDef VPEP_Puppet;

        
        public Dictionary<Pawn, bool> needsPunishing = new Dictionary<Pawn, bool>();

        public Dictionary<Pawn,bool> girlNeedsAttending=new Dictionary<Pawn, bool>();


        HashSet<Thing> readyPussyShockRods = new HashSet<Thing>();

        public static List<Pawn> GirlsNeedingPunishment(Pawn warden)
        {
            return warden.Map.GetComponent<Subjugate>().needsPunishing.Where(v => v.Value).Select(v => v.Key).ToList();
        }
        public static List<Pawn> GirlNeedsAttending(Pawn warden)
        {
            return warden.Map.GetComponent<Subjugate>().girlNeedsAttending.Where(v=>v.Value).Select(v => v.Key).ToList();
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


            harmony.Patch(original: typeof(Need).GetProperty("CurLevel", BindingFlags.Public | BindingFlags.Instance).GetGetMethod(),
                postfix: new HarmonyMethod(typeof(Subjugate), nameof(Need_Suppression_CurLevel)));

            harmony.PatchAll();

            Log.Message("Subjugate PATCHED.");
        }

        static FieldInfo PawnField = typeof(Need).GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void Need_Suppression_CurLevel(ref float __result, Need_Suppression __instance)
        {

            if (__instance.def.defName== "Suppression" && PawnField.GetValue(__instance) is Pawn pawn && pawn.gender==Gender.Female && pawn.IsSlaveOfColony)
            {
                pawn.TryGet_Subjugate_Comp(out var comp);

                var beatingOffset = comp.BeatingRating / 100f;
                var subjugateLevel = pawn.Subjugate_Hediff().Severity*10f;
                var resistance = comp.Resistance;
                Log.Message($"{pawn} RESISTANCE: {resistance} {subjugateLevel}");


                __result = Mathf.Min(1.0f, __result + (beatingOffset));
                //Log.Message($"{pawn} {__instance.def.defName}: {__result} {comp.BeatingRating}");
            }
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