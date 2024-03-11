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

            PatchAll(harmony);

            harmony.PatchAll();

            Log.Message("Subjugate PATCHED.");
        }

        private static void PatchAll(Harmony harmony)
        {
            // Get all assemblies in the current application domain
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Use LINQ to select types that implement IHarmonyPatch interface
            Type[] patchClasses = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IHarmonyPatch).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                .ToArray();

            foreach ( Type patchClass in patchClasses )
            {
                var inst = (IHarmonyPatch) Activator.CreateInstance(patchClass);
                Log.Message("A");
                inst.Patch(harmony);
            }
        }
    }

    public interface IHarmonyPatch {
        void Patch(Harmony harmony);
    }



}