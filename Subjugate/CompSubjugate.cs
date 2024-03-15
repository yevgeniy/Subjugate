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
        public static Dictionary<Pawn, CompSubjugate[]> Repo = new Dictionary<Pawn, CompSubjugate[]>();

        public int Level;
        private bool IsPrimed = false;
        private float CurrentRating;
        private float RatingCap;

        private double CurrentContentRating;
        private double ContentCap;
        private double ContentGainPerTick = Convert.ToDouble(20) / Convert.ToDouble(GenDate.TicksPerSeason);

        public float ContentRatio => (float)(CurrentContentRating / ContentCap);
        public bool IsContent => CurrentContentRating == ContentCap;
        public object ContentStr => IsContent
            ? Pawn.Name.ToStringShort + " is happy being a slave"
            : "Content: " + ContentRatio * 100f + "%";

        public float PunishmentDealtRating;

        public static List<Perk> AllPerks = new List<Perk> {
            new PerkArtistic()
        };
        public List<Perk> Perks = new List<Perk>();

        private Need_Suppression supneed;
        public Need_Suppression SupNeed { get
            {
                supneed = supneed ?? Pawn.needs.TryGetNeed<Need_Suppression>();
                return supneed;
            } }

        public Pawn Pawn
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

        public XPSystem xp;

        static CompSubjugate()
        {
            /*add subjugate comp to all defs having a race */
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race!=null && thingDef.race.Humanlike))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(CompSubjugate) });
            }

            /* Add global thoughts to precept */
            foreach (PreceptDef preceptDef in DefDatabase<PreceptDef>.AllDefs)
            {
                preceptDef.comps.Add(new PreceptComp_SituationalThought { thought = Defs.NeedAdmonishing });
            }

            /*replace all fat and hulking female bodytypes to normal*/
            var femaleBodyType = DefDatabase<BodyTypeDef>.AllDefs.First(v => v.defName == "Female");
            Log.Message("FEMALE BODY TYPE: " + femaleBodyType);
            foreach(var i in DefDatabase<BackstoryDef>.AllDefs)
            {
                if (i.bodyTypeFemale!=null && (i.bodyTypeFemale.defName == "Fat" || i.bodyTypeFemale.defName == "Hulk"))
                    i.bodyTypeFemale = femaleBodyType;
            }
            

            posstats = posstatsnames.Select(v => DefDatabase<StatDef>.AllDefs.FirstOrDefault(vv => vv.defName == v)).Where(v=>v!=null).ToHashSet<StatDef>();
            negstats= negstatsnames.Select(v => DefDatabase<StatDef>.AllDefs.FirstOrDefault(vv => vv.defName == v)).Where(v => v != null).ToHashSet<StatDef>();


        }
        public static HashSet<StatDef> posstats;
        public static HashSet<StatDef> negstats;
        
        public static string[] posstatsnames = new string[]
{
            "MaxHitPoints",
            "MeleeDPS",
            "MeleeHitChance",
            "MeleeDodgeChance",
            "MoveSpeed",
            "GlobalLearningFactor",
            "EatingSpeed",
            "ImmunityGainSpeed",
            "InjuryHealingFactor",
            "CarryingCapacity",
            "MeditationFocusGain",
            "NegotiationAbility",
            "MiningSpeed",
            "ResearchSpeed",
            "ConstructionSpeed",
            "ConstructSuccessChance",
            "SmeltingSpeed",
            "ButcheryFleshSpeed",
            "ConversionPower",
            "SocialIdeoSpreadFrequencyFactor",
            "WorkSpeedGlobal",
};
        public static string[] negstatsnames = new string[]
        {
            "Ability_CastingTime",
            "Ability_PsyfocusCost",
            "EquipDelay",
            "AimingDelayFactor",
        };

        public CompSubjugate()
        {
            CurrentRating = 0f;
            RatingCap = 0f;

            xp = new XPSystem(this);
        }

        long ticks;
        public override void CompTick()
        {
            ticks++;
        }

        private double ticksInRareTick = 250;
        public Action<CompSubjugate> ticker = delegate { };
        public Action<CompSubjugate> masterTicker = (comp) =>
        {
            if (comp.ticks % 2000 == 0) /* long tick shim */
            {
                comp.PunishmentDealtRating = Mathf.Max(0, comp.PunishmentDealtRating - 1);
            }
        };
        public Action<CompSubjugate> slaveTicker = (comp) =>
        {
            comp.xp.TickRare();

            if (!comp.IsContent)
            {
                double toadd = Convert.ToDouble(comp.Level) * comp.ContentGainPerTick * comp.ticksInRareTick;
                comp.CurrentContentRating += toadd;
                if (comp.CurrentContentRating > comp.ContentCap)
                    comp.CurrentContentRating = comp.ContentCap;   
            } else
            {
                comp.SupNeed.CurLevel = 1f;
            }
        };
        public override void CompTickRare()
        {
            base.CompTickRare();
            ticker(this);

        }

        public override void PostDeSpawn(Map map)
        {
            RemoveFromRepo(Pawn);
            base.PostDeSpawn(map);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.Look(ref Perks, "subjugate-perks");
            Scribe_Values.Look(ref Level, "subjugate-lvl");
            Scribe_Values.Look(ref IsPrimed, "subjugate-is-hot");
            Scribe_Values.Look(ref CurrentRating, "subjugate-cur-rat");
            Scribe_Values.Look(ref RatingCap, "subjugate-rat-cap");
            Scribe_Values.Look(ref PunishmentDealtRating, "subjugate-dic-dealt-rat");
            Scribe_Values.Look(ref CurrentContentRating, "subjugate-cur-cont-rat");
            Scribe_Values.Look(ref ContentCap, "subjugate-cont-cap");
            Scribe_Values.Look(ref fortheladies, "subjugate-for-lad");
            Scribe_Values.Look(ref ForTheLadiesMult, "subjugate-for-lad-mult");

            xp.ExposeData();

            if (Perks == null)
                Perks = new List<Perk>();
        }

        public void Prime()
        {
            if (IsPrimed)
            {
                return;
            }

            IsPrimed = true;
            RemoveFromRepo(Pawn);

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
            RemoveFromRepo(Pawn);
            Level++;

            var t = Pawn.story.traits.GetTrait(Defs.Subjugated);
            if (t == null) {
                Pawn.story.traits.GainTrait(new Trait(Defs.Subjugated, 0, true));
                ActivateSubjugation();
            }

            Messages.Message(Pawn + " was subjugated x" + Level, MessageTypeDefOf.PositiveEvent);
        }

        private void ActivateSubjugation()
        {
            /*appathy skills */
            var appathyskills = new SkillDef[] { SkillDefOf.Construction, SkillDefOf.Mining };
            foreach(var skilldef in appathyskills)
            {
                var skill = Pawn.skills.GetSkill(skilldef);

                if (!Subjugate.HasVanillaSkillMod)
                    skill.passion = Passion.None;
                else
                {
                    byte p = 3;
                    skill.passion = (Passion)p;
                }

                skill.Notify_SkillDisablesChanged();
            }
            
        }

        
        public static CompSubjugate GetComp(Pawn pawn)
        {
            if (!Repo.ContainsKey(pawn))
            {
                var comp = pawn.GetComp<CompSubjugate>();
                if (comp == null)
                    return null;

                if (pawn.IsColonist && pawn.gender == Gender.Male && pawn.Ideo.HasPrecept(Defs.SubjugateAllWomen))
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, comp, null });
                    comp.ticker = comp.masterTicker;

                } else if (pawn.IsColonist && pawn.gender == Gender.Female && comp.Level>0 && pawn.IsSlave)
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, null, comp });
                    comp.ticker = comp.slaveTicker;
                }
                else
                    Repo.Add(pawn, new CompSubjugate[] { null, null, null });
            }

            return Repo[pawn][(byte)pawn.gender];
        }
        public static void ClearRepo()
        {
            foreach(var i in Repo.ToList())
            {
                RemoveFromRepo(i.Key);   
            }
        }
        public  static void RemoveFromRepo(Pawn pawn)
        {
            var i = Repo[pawn];
            if (i[1] != null) i[1].ticker = delegate { };
            else if (i[2] != null) i[2].ticker = delegate { };

            Repo.Remove(pawn);
        }

        static string[] depricatedskills = new string[]{
            SkillDefOf.Mining.defName,
            SkillDefOf.Construction.defName
        };
        public bool HadDepricatedSkillCaps(SkillDef def, ref int skillcap)
        {
            if (Level>0 && depricatedskills.Contains(def.defName))
            {
                skillcap = 10;
                return true;
            }
            return false;
        }

        public bool CanOnlyTailor()
        {
            return Level > 0;
        }

        public static string[] basediswork = new string[] { "Smithing", "Crafting" };
        WorkTypeDef[] dissdefsCache;
        public WorkTypeDef[] DisabledWorkTypes()
        {
            if (Level>0)
            {
                dissdefsCache = dissdefsCache ?? DefDatabase<WorkTypeDef>.AllDefs.Where(v => basediswork.Contains(v.defName)).ToArray();
                return dissdefsCache;
            }
            return null;
            
        }
        public WorkTypeDef[] GetEnabledWorkTypes()
        {
            var worktypes = new List<WorkTypeDef>();

            if (Perks.Any(v => v is PerkArtistic))
                worktypes.Add(WorkTypeDefOf.Art);

            return worktypes.ToArray();
        }

        public bool HatesWearingArmor()
        {
            return Level > 0;
        }

        static string[] basedisabledskills = new string[] { SkillDefOf.Shooting.defName, SkillDefOf.Melee.defName };
        public bool DisabledSkill(SkillRecord instance)
        {
            return Level > 0 && basedisabledskills.Contains(instance.def.defName);
        }


        public int AvailablePerks()
        {
            return Mathf.Max(Level - Perks.Count, 0);
        }

        public void AddPerk(Perk newperk)
        {
            newperk.Activate(Pawn);
            Perks.Add(newperk);
            dissdefsCache = null;

            Pawn.skills.skills.ForEach(v => v.Notify_SkillDisablesChanged());
            Pawn.Notify_DisabledWorkTypesChanged();
            /*clear learnrate factors in skills expanded*/
            if (Subjugate.HasVanillaSkillMod)
                ClearSkillsModCache();
        }

        
        private bool fortheladies;
        public bool ForTheLadies { get
            {
                return fortheladies;
            }
            set
            {
                if (fortheladies && !value)
                {
                    var t = Pawn.story.traits.GetTrait(Defs.ForTheLadies);
                    Pawn.story.traits.RemoveTrait(t);
                }
                else if(!fortheladies && value)
                {
                    Pawn.story.traits.GainTrait(new Trait(Defs.ForTheLadies, 0, true));
                }
                fortheladies = value;
            }
        }
        public float ForTheLadiesMult;
        private MethodInfo clearCachemeth;

        private void ClearSkillsModCache()
        {
            clearCachemeth = clearCachemeth
                ?? AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .First(v => v.Name == "LearnRateFactorCache")
                    .GetMethod("ClearCache", BindingFlags.Public | BindingFlags.Static);

            clearCachemeth.Invoke(null,null);
        }

        public bool EnabledSkill(SkillRecord instance)
        {
            return Perks.Any(v=>v.IsSkillForceEnabled(instance));
        }

        public static float CalcRestMultiplier(Pawn pawn)
        {
            if (!pawn.InBed())
                return 0f;
            if (pawn.gender != Gender.Female)
                return 0f;

            var bed = pawn.CurrentBed();
            var slots = bed.SleepingSlotsCount;
            if (slots <= 1)
                return 0f;
            var numberSubmissiveOccupants = 0;
            var hasSubmissivePartner = false;

            for(var i=0; i < bed.SleepingSlotsCount;i++)
            {
                var occ = bed.GetCurOccupant(i);
                if (occ == null)
                    continue;
                var occcomp = CompSubjugate.GetComp(occ);
                if (occcomp == null)
                    continue;
                var occIsSubmissive = occcomp.Perks.Any(v => v is PerkGentle);
                if (occIsSubmissive)
                {
                    numberSubmissiveOccupants++;
                    if (occ != pawn)
                        hasSubmissivePartner = true;
                }
            }
            if (!hasSubmissivePartner)
                return 0f;
            return .2f + (numberSubmissiveOccupants - 1) * .05f;
        }



        public float CalcGlobalStatMult(StatDef stat, float curval)
        {
            if (Pawn.gender != Gender.Male)
                return curval;

            if (!fortheladies)
                return curval;



            if (!Pawn.Ideo.HasPrecept(Defs.SubjugateAllWomen))
                return curval;

            var apt = curval * (ForTheLadiesMult * .01f);
            var res = curval;
            if (posstats.Contains(stat))
            {
                res= curval + apt;
            }
            else if (negstats.Contains(stat))
            {
                res=curval - apt;
            }
            Log.Message(Pawn+ " stat:" + stat.defName + " orig:" + curval+ " new:" + res);

            return res;
        }
    }

    public class XPSystem : IExposable
    {
        public XPSystem(CompSubjugate comp)
        {
            Comp = comp;

        }
        public float XPBuffer;
        public float XPExtractedThisCycle;
        public CompSubjugate Comp;
        public string SelectedSkill;

        public void TickRare()
        {

            if (Comp.Level == 0)
                return;

            var depricatedSkills = GetDepricatedSkills();

            /*apply xp deprication for this cycle to a random skill. */
            if (XPExtractedThisCycle>0)
            {
                var depricatingSkill = depricatedSkills.RandomElement();
                var skill = Comp.Pawn.skills.skills.FirstOrDefault(v => v.def.defName == depricatingSkill && (v.levelInt > 0 || v.xpSinceLastLevel > 0));
                if (skill != null)
                {
                    float resultingxp = Mathf.Max(0, TotalXp(skill) - XPExtractedThisCycle);

                    Log.Message(skill.def.defName + " extracted:" + XPExtractedThisCycle + " l:" + skill.levelInt + " need:" + skill.xpSinceLastLevel);

                    XpToLevel(skill, resultingxp);
                    XPExtractedThisCycle = 0;
                }
            }
            /*if we were not able to find a skill to apply expanded xp, look at persk */
            if (XPExtractedThisCycle > 0)
            {
                var skillpoolperk = Comp.Perks.FirstOrFallback(v => v.PoolXp > 0);
                if (skillpoolperk!=null)
                {
                    var xp = skillpoolperk.DrainXP(Comp.Pawn, XPExtractedThisCycle);

                    XPExtractedThisCycle = 0;
                }
            }
            XPExtractedThisCycle = 0;


            /*revalidate total xp buffer*/
            var validbufferskills = Comp.Pawn.skills.skills.Where(v => depricatedSkills.Contains(v.def.defName));
            float i = 0;
            foreach(var skill in validbufferskills)
            {
                i += TotalXp(skill);
            }
            XPBuffer = i + Comp.Perks.Select(v => v.PoolXp).Sum();
        }

        public static void XpToLevel(SkillRecord skill, float resultingxp)
        {
            var xp = 0f;
            for (var i = 0; i < skill.levelInt; i++)
            {
                
                if (xp <= resultingxp && resultingxp < xp+xpLvlUpData[i])
                {
                    skill.Level = i;
                    skill.xpSinceLastLevel = resultingxp - xp;
                    break;
                }

                xp += xpLvlUpData[i];

            }
        }
        public static float TotalXp(SkillRecord skill)
        {
            var xp = 0f;
            for (var i = 0; i < skill.levelInt; i++)
            {
                xp += xpLvlUpData[i];
            }
            xp += skill.xpSinceLastLevel;
            return xp;
        }

        public static float[] xpLvlUpData = new float[]
        {
            SkillRecord.XpRequiredToLevelUpFrom(0),
            SkillRecord.XpRequiredToLevelUpFrom(1),
            SkillRecord.XpRequiredToLevelUpFrom(2),
            SkillRecord.XpRequiredToLevelUpFrom(3),
            SkillRecord.XpRequiredToLevelUpFrom(4),
            SkillRecord.XpRequiredToLevelUpFrom(5),
            SkillRecord.XpRequiredToLevelUpFrom(6),
            SkillRecord.XpRequiredToLevelUpFrom(7),
            SkillRecord.XpRequiredToLevelUpFrom(8),
            SkillRecord.XpRequiredToLevelUpFrom(9),
            SkillRecord.XpRequiredToLevelUpFrom(10),
            SkillRecord.XpRequiredToLevelUpFrom(11),
            SkillRecord.XpRequiredToLevelUpFrom(12),
            SkillRecord.XpRequiredToLevelUpFrom(13),
            SkillRecord.XpRequiredToLevelUpFrom(14),
            SkillRecord.XpRequiredToLevelUpFrom(15),
            SkillRecord.XpRequiredToLevelUpFrom(16),
            SkillRecord.XpRequiredToLevelUpFrom(17),
            SkillRecord.XpRequiredToLevelUpFrom(18),
            SkillRecord.XpRequiredToLevelUpFrom(19),
        };
    

        private string[] GetDepricatedSkills()
        {
            return new string[]
            {
                SkillDefOf.Shooting.defName,
                SkillDefOf.Melee.defName,
                SkillDefOf.Mining.defName,
                SkillDefOf.Construction.defName
            };
        }
        public string[] GetTargetSkills()
        {
            return new string[] {
                SkillDefOf.Plants.defName,
                SkillDefOf.Cooking.defName,
                SkillDefOf.Crafting.defName,
                SkillDefOf.Artistic.defName,
            };
        }

        public float ExtractXP(float amnt)
        {
            var n = Mathf.Min(XPBuffer, amnt*Comp.Level);
            XPBuffer -= n;
            XPExtractedThisCycle += n;

            return n;
        }
        public float TryExtractXP(string skillname, float amnt)
        {
            if (skillname==SelectedSkill)
            {
                return ExtractXP(amnt);
            }
            return 0;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref XPBuffer, "subj-xp-buffer");
            Scribe_Values.Look(ref XPExtractedThisCycle, "subj-xp-extacted");
            Scribe_Values.Look(ref SelectedSkill, "subj-sel-skill");
            
        }
    }

}
