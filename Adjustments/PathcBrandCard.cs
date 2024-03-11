using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace Adjustments
{
    [HarmonyPatch(typeof(CharacterCardUtility), "DrawCharacterCard")]
    public class PathcBrandCard
    {

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo LifestageAndXenotypeOptions = AccessTools.Method(typeof(CharacterCardUtility), "LifestageAndXenotypeOptions");
            MethodInfo SexualityCardToggle = AccessTools.Method(typeof(PathcBrandCard), nameof(DesigCardToggle));
            bool traits = false;
            bool found = false;
            foreach (CodeInstruction i in instructions)
            {
                if (found)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0) { labels = i.labels.ListFullCopy() };//rect
                    yield return new CodeInstruction(OpCodes.Ldarg_1);//pawn
                    yield return new CodeInstruction(OpCodes.Ldarg_3);//creationRect
                    yield return new CodeInstruction(OpCodes.Call, SexualityCardToggle);
                    found = false;
                    i.labels.Clear();
                }
                if (i.opcode == OpCodes.Call && i.operand as MethodInfo == LifestageAndXenotypeOptions)
                {
                    found = true;
                }
                if (i.opcode == OpCodes.Ldstr && i.operand.Equals("Traits"))
                {
                    traits = true;
                }
                if (traits && i.opcode == OpCodes.Ldc_R4 && i.operand.Equals(2f))//replaces rect y calculation
                {
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    traits = false;
                    continue;
                }
                yield return i;
            }
        }



        public static void DesigCardToggle(Rect rect, Pawn pawn, Rect creationRect)
        {
            if (pawn == null) return;
            if (BrandComp.Comp(pawn) == null) return;

            Rect rectNew = new Rect(CharacterCardUtility.BasePawnCardSize.x - 50f, 2f, 24f, 24f);
            
            if (Current.ProgramState != ProgramState.Playing)
            {
                rectNew = new Rect(creationRect.width - 24f, 108f, 24f, 24f);
            }
            Color old = GUI.color;

            GUI.color = rectNew.Contains(Event.current.mousePosition) ? new Color(0.25f, 0.59f, 0.75f) : new Color(1f, 1f, 1f);
            
            GUI.DrawTexture(rectNew, ContentFinder<Texture2D>.Get("looksgood"));

            TooltipHandler.TipRegion(rectNew, "branding");
            if (Widgets.ButtonInvisible(rectNew))
            {


                SoundDefOf.InfoCard_Open.PlayOneShotOnCamera();
                BrandComp.ShowDialog(pawn);
            }
            GUI.color = old;
        }

    }
}
