using Subjugate.SubjucationPerks;
using RimWorld;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Subjugate
{
    [StaticConstructorOnStartup]
    public class CompSubjugate : ThingComp
    {
        public int CurrentSubjugationLevel;



        
        public static readonly TraitDef SubjugatedTrait = DefDatabase<TraitDef>.GetNamed("Subjugated");
        public static Dictionary<Pawn, CompSubjugate> Repo = new Dictionary<Pawn, CompSubjugate>();
        public static List<BasePerk> NegPerks = new List<BasePerk>
        {
            new PerkNegSkillConstruction(),
            new PerkNegSkillMelee(),
            new PerkNegSkillMining(),
            new PerkNegSkillShooting(),
            new PerkNegHateArmor()
        };
        public static List<BasePerk> OtherPerks = new List<BasePerk>
        {
            new PerkPlant(),
            new PerkCooking(),
            new PerkArtistic(),
            new PerkNudistTrait(),
            new PerkTailoring()
        };


        
        private bool SubjugationActive=false;
        private float CurrentRating;
        private float ResistanceCap;
        private double CurrentContentScore;
        private double ContentScoreLimit;
        static double gainPerTickPerPerk = Convert.ToDouble(20) / Convert.ToDouble(GenDate.TicksPerYear/2);


        public float DisciplineDealtRating;

        public List<BasePerk> Perks = new List<BasePerk>();
        public bool IsContent;
        private Need_Suppression SupNeed;

        public float ContentPercent { get
            {
                if (IsContent)
                    return 100f;
                return Mathf.Floor( (float)( CurrentContentScore / ContentScoreLimit * 100f) ) ;
            } }

        private Pawn Pawn
        {
            get
            {
                if (parent is Pawn pawn)
                {
                    return pawn;
                }
                return null;
            }
        }

        public object ContentStr => IsContent
            ? Pawn.Name.ToStringShort + " is happy being a slave"
            : "Content: " + ContentPercent + "%";

        static CompSubjugate()
        {
            /*add subjugate comp to all defs having a race */
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(CompSubjugate) });
            }

            ///* Add thought to precept */
            //foreach (PreceptDef preceptDef in DefDatabase<PreceptDef>.AllDefs )
            //{
            //    preceptDef.comps.Add(new PreceptComp_SituationalThought { thought=Defs.UnsubjugatedWomen });
            //}


        }
        public CompSubjugate()
        {
            CurrentRating = 0f;
            ResistanceCap = 0f;
        }
        
        public override void CompTickRare()
        {
            base.CompTickRare();

            if (Find.TickManager.TicksGame % 2000==0) /* long tick shim */
            {
                DisciplineDealtRating = Mathf.Max(0, DisciplineDealtRating - 1);
            }
                

            if (Pawn.gender==Gender.Female)
            {
                if (!IsContent && Pawn.IsSlave)
                {
                    double newval = CurrentContentScore + Convert.ToDouble(CurrentSubjugationLevel) * gainPerTickPerPerk * 250f; /*250 ticks in rare tick*/

                    CurrentContentScore = newval > ContentScoreLimit ? ContentScoreLimit : newval;

                    if (CurrentContentScore == ContentScoreLimit)
                    {
                        IsContent = true;
                        SupNeed = SupNeed ?? Pawn.needs.TryGetNeed<Need_Suppression>();
                        SupNeed.CurLevel = 1f;
                    }
                    
                }
            }

        }

        public override void PostDeSpawn(Map map)
        {
            Repo.Remove(Pawn);
            base.PostDeSpawn(map);
        }
     


      
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref Perks, "subjugate-perks");
            Scribe_Values.Look(ref CurrentSubjugationLevel, "subjugate-current-level" );
            Scribe_Values.Look(ref SubjugationActive, "subjugate-active" );
            Scribe_Values.Look(ref CurrentRating, "subjugate-current-rating" );
            Scribe_Values.Look(ref ResistanceCap, "subjugate-res" );
            Scribe_Values.Look(ref CurrentContentScore, "subjugate-cur-cont-scor");
            Scribe_Values.Look(ref ContentScoreLimit, "subjugate-cont-scor-lim");
            Scribe_Values.Look(ref IsContent, "subjugate-is-cont");
            Scribe_Values.Look(ref DisciplineDealtRating, "subjugate-dic-dealt-rat");

            if (!Repo.ContainsKey(Pawn))
                Repo.Add(Pawn, this);

            if (Perks == null)
                Perks = new List<BasePerk>();

            if (Pawn.RaceProps.Animal)
            {
                Pawn.AllComps.Remove(this);
            }
        }

        public void ActivateSubjugation()
        {

            if (SubjugationActive)
            {
                return;
            }

            SubjugationActive = true;

            CurrentRating = 0f;

            ResistanceCap = GenResistance();

        }
        private float GenResistance()
        {
            FloatRange value = Pawn.kindDef.initialResistanceRange.Value;
            float single = value.RandomInRange;
            if (Pawn.royalty != null)
            {
                RoyalTitle mostSeniorTitle = Pawn.royalty.MostSeniorTitle;
                if (mostSeniorTitle != null)
                {
                    single += mostSeniorTitle.def.recruitmentResistanceOffset;
                }
            }
            return (float)GenMath.RoundRandom(single);
        }

        public void RegisterSeverity(float severity, Pawn bypawn)
        {
            if (!SubjugationActive)
                return;

            if (bypawn.IsColonist && !bypawn.IsSlave)
            {
                CompSubjugate.GetComp(bypawn).DisciplineDealtRating += severity;
                ContentScoreLimit += severity;
                IsContent = false;

                CurrentRating = Mathf.Min(ResistanceCap, CurrentRating + severity * .1f);

                /*lower resistance by the beating amount*/
                Pawn.guest.will = Mathf.Max(.1f, Pawn.guest.will - severity * .01f);

                if (CurrentRating >= ResistanceCap)
                {
                    UpgradeSubjugation();
                }
            }
            
        }

        private void UpgradeSubjugation()
        {

            var t = Pawn.story.traits.GetTrait(Defs.Subjugated);
            if (t==null) {
                Pawn.story.traits.GainTrait(new Trait(Defs.Subjugated, 0, true));
            } else
            {
                Pawn.story.traits.RemoveTrait(t);
                Pawn.story.traits.GainTrait(new Trait(Defs.Subjugated));
            }
            

            CurrentSubjugationLevel++;

            AddPerks();

            SubjugationActive=false;
        }

        private void AddPerks()
        {
            /* Negative perk */
            var perkType = NegPerks.Where(v => v.CanHandle(Pawn)).RandomElement();
            if (perkType!=null)
            {
                var perk = Perks.FirstOrDefault(v => v.GetType().Name == perkType.GetType().Name);
                if (perk == null)
                {
                    perk = (BasePerk)Activator.CreateInstance(perkType.GetType());
                    Perks.Add(perk);
                }
                perk.Activate(parent as Pawn);
            }
            

            /*Other perk*/
            perkType = OtherPerks.Where(v => v.CanHandle(Pawn)).RandomElement();
            if (perkType!=null)
            {
                var perk = Perks.FirstOrDefault(v => v.GetType().Name == perkType.GetType().Name);
                if (perk == null)
                {
                    perk = (BasePerk)Activator.CreateInstance(perkType.GetType());
                    Perks.Add(perk);
                }

                perk.Activate(parent as Pawn);
            }
            
            
        }

        public static CompSubjugate GetComp(Pawn pawn)
        {
            if (!Repo.ContainsKey(pawn))
            {
                var comp = pawn.GetComp<CompSubjugate>();
                if (comp == null)
                    return null;

                Repo.Add(pawn, comp);

            }
            return Repo[pawn];

        }
    }

    public class patch_for_comp_subjugate:IHarmonyPatch {

        public void Patch(Harmony harmony) {
            var t=typeof(patch_for_comp_subjugate);
            harmony.Patch(
                typeof(GuestUtTraitility).GetMethod("TipString"),
                postfix: new HarmonyMethod(t, "subjugated_trait_tip_string")
            );
            harmony.Patch(
                typeof(SlaveRebellionUtility).GetMethod("CanParticipateInSlaveRebellion"),
                prefix: new HarmonyMethod(t, "subjugated_ladies_dont_rebel")
            );
            harmony.Patch(
                typeof(StatExtension).GetMethod("GetStatValue"),
                postfix: new HarmonyMethod(t, "subjugated_ladies_dont_have_suppress_rating")
            );
            harmony.Patch(
                typeof(Pawn).GetMethod("GetDisabledWorkTypes"),
                postfix: new HarmonyMethod(t, "subjugated_ladies_can_only_do_tailoring")
            );
            harmony.Patch(
                typeof(Pawn_ApparelTracker).GetMethod("Notify_ApparelAdded"),
                prefix: new HarmonyMethod(t, "subjugated_ladies_dont_like_wearking_armor")
            );
            // harmony.Patch(
            //     typeof(GuestUtility).GetMethod("GetDisabledWorkTypes"),
            //     postfix: new HarmonyMethod(t, "subjugated_ladies_can_do_art")
            // );
            harmony.Patch(
                typeof(SkillRecord).GetMethod("CalculateTotallyDisabled"),
                prefix: new HarmonyMethod(t, "subjugated_ladies_cant_do_ranged_and_melee_enable_applicable")
            );
            harmony.Patch(
                typeof(SkillRecord).GetMethod("CalculatePermanentlyDisabled"),
                prefix: new HarmonyMethod(t, "subjugated_ladies_cant_do_ranged_and_melee_enable_applicable")
            );
            harmony.Patch(
                typeof(HediffSet).GetMethod("AddDirect"),
                prefix: new HarmonyMethod(t, "rejister_severity_when_ladies_are_punished")
            );
            harmony.Patch(
                typeof(Pawn_GuestTracker).GetMethod("SetGuestStatus"),
                prefix: new HarmonyMethod(t, "activate_subjugation_when_ladies_are_imprisoned")
            );
            
        }

        public static bool activate_subjugation_when_ladies_are_imprisoned(Faction newHost, GuestStatus guestStatus, Pawn_GuestTracker __instance)
        {
            if (!__instance.IsPrisoner && guestStatus == GuestStatus.Prisoner)
            {
                var pawn = GetPawn(__instance);
                if (pawn.gender==Gender.Female /*&& pawn.guilt.IsGuilty*/)
                {
                    var comp = pawn.GetComp<CompSubjugate>();
                    if (comp!=null)
                    {
                        comp.ActivateSubjugation();
                    }
                }

            }

            return true;
        }

        public static bool rejister_severity_when_ladies_are_punished(Hediff hediff, DamageInfo dinfo, DamageWorker.DamageResult damageResult, HediffSet __instance)
        {
            if (hediff!=null)
            {
                var pawn = __instance.pawn;
                if (pawn.gender == Gender.Female && dinfo.Instigator is Pawn bypawn)
                {
                    var comp = CompSubjugate.GetComp(pawn);
                    if (comp != null )
                    {
                        comp.RegisterSeverity(hediff.Severity, bypawn);
                    }
                }

            }

            return true;
        }

        static new string[] disskills=new string[]{SkillDefOf.Melee.defName, SkillDefOf.Shooting.defName};
        public static bool subjugated_ladies_cant_do_ranged_and_melee_enable_applicable(SkillRecord __instance, ref bool __result)
        {
            var comp = CompSubjugate.GetComp(__instance.Pawn);

            if (comp != null)
            {
                if (comp.CurrentSubjugationLevel>0) {
                    if (disskills.Contains(__instance.def.defName)) {
                        __result=true;
                        return false;
                    }

                    /* TODO: here parse for enabled skills */
                }
            }

            return true;
        }
    
    

 
        private static Pawn GetPawn(Pawn_GuestTracker instance)
        {
            Type type = typeof(Pawn_GuestTracker);

            // Get the private field info
            FieldInfo fieldInfo = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.Instance);

            return (Pawn)fieldInfo.GetValue(instance);

        }
        public static void subjugated_ladies_can_do_art(Pawn_GuestTracker guest, ref List<WorkTypeDef> __result)
        {
            var pawn = GetPawn(guest);
            if (pawn.gender != Gender.Female)
                return;

            var comp = CompSubjugate.GetComp(pawn);
            if (comp == null)
                return true;

            if (comp.CurrentSubjugationLevel>0) {
                __result.RemoveAll(v => v.defName == WorkTypeDefOf.Art.defName);
            }

        }
    

        public static bool subjugated_ladies_dont_like_wearking_armor(Apparel apparel, Pawn_ApparelTracker __instance)
        {
            var comp = CompSubjugate.GetComp(__instance.pawn);
            if (comp == null)
                return true;

            if (comp.CurrentSubjugationLevel>0 && apparel.def.tradeTags.Any(v => v == "Armor")) {
                __instance.pawn.needs.mood.thoughts.memories.TryGainMemory(Defs.SubjugatePutOnArmour);
            }

            return true;
        }
    

        public static string[] diswork =new string[] { "Smithing", "Crafting" };
        public static void subjugated_ladies_can_only_do_tailoring(Pawn __instance, ref List<WorkTypeDef> __result)
        {
            var comp = CompSubjugate.GetComp(__instance);
            if (comp == null)
                return;

            if (comp.CurrentSubjugationLevel>0) {

                var disableDefs = DefDatabase<WorkTypeDef>.AllDefs
                    .Where(v => diswork.Contains(v.defName));

                foreach(var i in disableDefs)
                    __result.AddDistinct(i);
                return;
            }

        }
    
        public static void subjugated_ladies_dont_have_suppress_rating(Thing thing, StatDef stat, bool applyPostProcess, int cacheStaleAfterTicks, ref float __result)
        {
            if (thing is Pawn pawn)
            {
                if (stat != StatDefOf.SlaveSuppressionFallRate)
                    return;

                var comp = CompSubjugate.GetComp(pawn);
                if (comp == null)
                    return;

                if (comp.CurrentSubjugationLevel>0) {
                    __result=0;
                    return;
                }

            }
        }
     
        public static bool subjugated_ladies_dont_rebel(Pawn pawn, ref bool __result)
        {
            var comp = CompSubjugate.GetComp(pawn);

            if (comp!=null && comp.CurrentSubjugationLevel>0)
            {
                __result = false;
                return false;
            }

            return true;
        }
    

        
        public static void subjugated_trait_tip_string(Trait __instance, ref string __result, Pawn pawn)
        {
            if (__instance.def != Defs.Subjugated)
                return;

            var comp = CompSubjugate.GetComp(pawn);


            var content = comp.ContentStr;

            __result += "\n\n" + content;

            var explanations = comp.Perks.Select(v => v.Describe(pawn)).ToList();
            __result = __result + "\n\n" + string.Join("\n", explanations);
        }
    }

}
