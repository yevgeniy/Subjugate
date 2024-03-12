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
using HarmonyLib;
using System.Runtime.Remoting.Messaging;

namespace Subjugate
{
    [StaticConstructorOnStartup]
    public class CompSubjugate : ThingComp
    {
        public int Level;



        
        public static readonly TraitDef SubjugatedTrait = DefDatabase<TraitDef>.GetNamed("Subjugated");
        public static Dictionary<Pawn, CompSubjugate> Repo = new Dictionary<Pawn, CompSubjugate>();
        
        private bool IsPrimed=false;
        private float CurrentRating;
        private float RatingCap;

        public float PunishmentDealtRating;

        public List<Perk> Perks = new List<Perk>();
        
        private Need_Suppression SupNeed;

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
            RatingCap = 0f;
        }
        
        public override void CompTickRare()
        {
            base.CompTickRare();

            if (Find.TickManager.TicksGame % 2000==0) /* long tick shim */
            {
                PunishmentDealtRating = Mathf.Max(0, PunishmentDealtRating - 1);
            }


            if (Level>0 && Pawn.IsSlave)
            {
                SupNeed = SupNeed ?? Pawn.needs.TryGetNeed<Need_Suppression>();
                SupNeed.CurLevel = 1f;
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
            Scribe_Values.Look(ref Level, "subjugate-lvl" );
            Scribe_Values.Look(ref IsPrimed, "subjugate-is-hot" );
            Scribe_Values.Look(ref CurrentRating, "subjugate-cur-rat" );
            Scribe_Values.Look(ref RatingCap, "subjugate-rat-cap" );
            Scribe_Values.Look(ref PunishmentDealtRating, "subjugate-dic-dealt-rat");

            if (Perks == null)
                Perks = new List<Perk>();

            /* no need for animals */
            if (Pawn.RaceProps.Animal)
            {
                Pawn.AllComps.Remove(this);
            }
        }

        public void Prime()
        {
            if (IsPrimed)
            {
                return;
            }

            IsPrimed = true;

            CurrentRating = 0f;

            RatingCap = GenResistance();

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
            if (!IsPrimed)
                return;

            if (bypawn.IsColonist && !bypawn.IsSlave)
            {
                CompSubjugate.GetComp(bypawn).PunishmentDealtRating += severity;

                CurrentRating = Mathf.Min(RatingCap, CurrentRating + severity * .1f);

                /*lower resistance by the beating amount*/
                Pawn.guest.will = Mathf.Max(.1f, Pawn.guest.will - severity * .01f);

                if (CurrentRating >= RatingCap)
                {
                    LevelUp();
                    IsPrimed = false;
                }
            }
            
        }

        private void LevelUp()
        {
            Level++;

            var t = Pawn.story.traits.GetTrait(Defs.Subjugated);
            if (t==null) {
                Pawn.story.traits.GainTrait(new Trait(Defs.Subjugated, 0, true));

                Perks.Add(new PerkDenyMelee());
                Perks.Add(new PerkDenyShooting());
                Perks.Add(new PerkHatesArmor());
                Perks.Add(new PerkTailoringConstraint());

                foreach(var i in Perks)
                    i.Activate(Pawn);
            }

            Messages.Message(Pawn + " was subjugated x" + Level, MessageTypeDefOf.PositiveEvent);
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

}
