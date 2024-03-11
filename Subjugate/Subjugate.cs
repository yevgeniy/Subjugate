using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Subjugate
{
    [StaticConstructorOnStartup]
    public static class Subjugate
    {
        public static bool HasVanillaSkillMod;

        static Subjugate()
        {
            Log.Message("Subjugate STARTED.");


            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var classType = assemblies.SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(v => v.Name == "SkillsMod");
            if (classType != null)
                HasVanillaSkillMod = true;

            Harmony.DEBUG = true;  // Enable Harmony Debug
            Harmony harmony = new Harmony("nimm.Subjugate");

            harmony.PatchAll();

            Log.Message("Subjugate PATCHED.");
        }

    }

    public interface IHarmonyPatch {
        void Patch();
    }



}