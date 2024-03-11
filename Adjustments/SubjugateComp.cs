using Adjustments.SubjucationPerks;
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

namespace Adjustments
{
    [StaticConstructorOnStartup]
    public class SubjugateComp : ThingComp
    {
        
        public static readonly TraitDef SubjugatedTrait = DefDatabase<TraitDef>.GetNamed("Subjugated");
        public static Dictionary<Pawn, SubjugateComp> Repo = new Dictionary<Pawn, SubjugateComp>();
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


        public int CurrentSubjugationLevel;
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

        static SubjugateComp()
        {
            /*add subjugate comp to all defs having a race */
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(SubjugateComp) });
            }

            ///* Add thought to precept */
            //foreach (PreceptDef preceptDef in DefDatabase<PreceptDef>.AllDefs )
            //{
            //    preceptDef.comps.Add(new PreceptComp_SituationalThought { thought=SubjugatedDefs.UnsubjugatedWomen });
            //}


        }
        public SubjugateComp()
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
                SubjugateComp.GetComp(bypawn).DisciplineDealtRating += severity;
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

            var t = Pawn.story.traits.GetTrait(SubjugatedDefs.Subjugated);
            if (t==null) {
                Pawn.story.traits.GainTrait(new Trait(SubjugatedDefs.Subjugated, 0, true));
            } else
            {
                Pawn.story.traits.RemoveTrait(t);
                Pawn.story.traits.GainTrait(new Trait(SubjugatedDefs.Subjugated));
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

        public static SubjugateComp GetComp(Pawn pawn)
        {
            if (!Repo.ContainsKey(pawn))
            {
                var comp = pawn.GetComp<SubjugateComp>();
                if (comp == null)
                    return null;

                Repo.Add(pawn, comp);

            }
            return Repo[pawn];

        }
    }

}
