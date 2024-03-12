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
        



        
        public static readonly TraitDef SubjugatedTrait = DefDatabase<TraitDef>.GetNamed("Subjugated");
        public static Dictionary<Pawn, CompSubjugate> Repo = new Dictionary<Pawn, CompSubjugate>();

        public int Level;
        private bool IsPrimed=false;
        private float CurrentRating;
        private float RatingCap;

        private double CurrentContentRating;
        private double ContentCap;
        private double ContentGainPerTick = Convert.ToDouble( 20) / Convert.ToDouble( GenDate.TicksPerSeason);

        public float ContentRatio => (float)( CurrentContentRating / ContentCap);
        public bool IsContent => CurrentContentRating == ContentCap;
        public object ContentStr => IsContent
            ? Pawn.Name.ToStringShort + " is happy being a slave"
            : "Content: " + ContentRatio*100f + "%";


        public float PunishmentDealtRating;

        public List<Perk> Perks = new List<Perk>();

        private Need_Suppression supneed;
        public Need_Suppression SupNeed { get
            {
                supneed = supneed ?? Pawn.needs.TryGetNeed<Need_Suppression>();
                return supneed;
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

        static CompSubjugate()
        {
            /*add subjugate comp to all defs having a race */
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(CompSubjugate) });
            }

            /* Add global thoughts to precept */
            foreach (PreceptDef preceptDef in DefDatabase<PreceptDef>.AllDefs)
            {
                preceptDef.comps.Add(new PreceptComp_SituationalThought { thought = Defs.NeedAdmonishing });
            }


        }
        public CompSubjugate()
        {
            CurrentRating = 0f;
            RatingCap = 0f;
        }

        private double ticksInRareTick = 250;
        public override void CompTickRare()
        {
            base.CompTickRare();

            if (Find.TickManager.TicksGame % 2000==0) /* long tick shim */
            {
                PunishmentDealtRating = Mathf.Max(0, PunishmentDealtRating - 1);
            }

            if (Level>0 && Pawn.IsSlave && Pawn.gender==Gender.Female)
            {
                double toadd = Convert.ToDouble(Level) * ContentGainPerTick * ticksInRareTick;
                CurrentContentRating += toadd;
                if (CurrentContentRating > ContentCap)
                    CurrentContentRating = ContentCap;

                if (IsContent)
                {
                    SupNeed.CurLevel = 1f;
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
            Scribe_Values.Look(ref Level, "subjugate-lvl" );
            Scribe_Values.Look(ref IsPrimed, "subjugate-is-hot" );
            Scribe_Values.Look(ref CurrentRating, "subjugate-cur-rat" );
            Scribe_Values.Look(ref RatingCap, "subjugate-rat-cap" );
            Scribe_Values.Look(ref PunishmentDealtRating, "subjugate-dic-dealt-rat");
            Scribe_Values.Look(ref CurrentContentRating, "subjugate-cur-cont-rat");
            Scribe_Values.Look(ref ContentCap, "subjugate-cont-cap");

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
                ContentCap += severity * .1f;

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
                Perks.Add(new PerkConstructionApathy());
                Perks.Add(new PerkMiningApathy());

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
