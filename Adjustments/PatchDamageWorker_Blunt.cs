using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Adjustments
{


    [HarmonyPatch(typeof(DamageWorker_Blunt), "ChooseHitPart")]
    public class PatchDamageWorker_Blunt
    {

        [HarmonyPostfix]
        public static void NonLethalBluntDamage(ref BodyPartRecord __result, DamageWorker_Blunt __instance, DamageInfo dinfo, Pawn pawn)
        {
            //"Brain", "Head", "Heart", "Jaw", "Neck", "Nose", "Right ear", "Right eye", "Left ear", "Left eye", "Skull", "Tongue", "Torso"

            if (dinfo.Instigator is Pawn srcPawn && IsPlayerPawn(srcPawn) && IsPlayerPawn(pawn))
            {
                var curPart = __result;
                var curHediff = pawn.health.hediffSet.hediffs.Where(v=>v.Part!=null)
                    .FirstOrDefault(v => v.Part.Label == curPart.Label);
                
                if (curHediff != null)
                {
                    var sev = curHediff.Severity;                    

                    if (sev>7f)
                    {
                        var sevParts = pawn.health.hediffSet.hediffs.Where(v => v.Severity >= 7f).Select(v => v.Part?.Label).ToList();

                        var newPart = pawn.health.hediffSet.pawn.def.race.body.AllParts
                            .FirstOrDefault(v => !sevParts.Contains(v.Label) );

                        if (newPart != null)
                        {
                            __result = newPart;
                            dinfo.SetIgnoreArmor(true);
                        }
                    }
                }

            }

        }

        private static bool IsPlayerPawn(Pawn srcPawn)
        {
            return srcPawn.IsColonist || srcPawn.IsSlave || srcPawn.IsPrisonerOfColony;
        }
    }
}
