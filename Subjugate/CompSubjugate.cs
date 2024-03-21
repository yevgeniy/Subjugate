
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

        public static Dictionary<Pawn, CompSubjugate[]> Repo = new Dictionary<Pawn, CompSubjugate[]>();
        public static Dictionary<string, byte> PassionRecords = new Dictionary<string, byte>();
        private List<string> DisabledSkills = new List<string>();
        public float XPGoal;
        public Dictionary<string, float> CurrentXp = new Dictionary<string, float>();

        public int Level;
        private bool isprimed;
        private bool IsPrimed
        {
            get
            {
                return isprimed;
            }
            set
            {
                var t = Pawn.story.traits.GetTrait(Defs.SubjugatedPrimed);
                if (value == true && t == null)
                {
                    Pawn.story.traits.GainTrait(new Trait(Defs.SubjugatedPrimed));
                }
                else if (value == false)
                {
                    if (t != null)
                        Pawn.story.traits.RemoveTrait(t);
                }

                isprimed = value;
            }
        }
        private float CurrentRating;
        private float RatingCap;

        public object DisciplinedStr => CurrentRating / RatingCap * 100f + "%";

        public float PunishmentDealtRating;

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

        private Need_Suppression supneed;
        public Need_Suppression SupNeed
        {
            get
            {
                supneed = supneed ?? Pawn.needs.TryGetNeed<Need_Suppression>();
                return supneed;
            }
        }


        static CompSubjugate()
        {
            /*add subjugate comp to all defs having a race */
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(thingDef =>
                    thingDef.race != null && thingDef.race.Humanlike))
            {
                thingDef.comps.Add(new CompProperties { compClass = typeof(CompSubjugate) });
            }

            /* Add global thoughts to precept */
            foreach (PreceptDef preceptDef in DefDatabase<PreceptDef>.AllDefs)
            {
                preceptDef.comps.Add(new PreceptComp_SituationalThought { thought = Defs.NeedAdmonishing });
            }



            posstats = posstatsnames.Select(v => DefDatabase<StatDef>.AllDefs.FirstOrDefault(vv => vv.defName == v)).Where(v => v != null).ToHashSet<StatDef>();
            negstats = negstatsnames.Select(v => DefDatabase<StatDef>.AllDefs.FirstOrDefault(vv => vv.defName == v)).Where(v => v != null).ToHashSet<StatDef>();


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
        }

        public long ticks;
        public override void CompTick()
        {
            ticks++;
        }

        public override void CompTickRare()
        {

            if (Pawn.gender == Gender.Male)
            {
                if (ticks % 2000 == 0) /* long tick shim */
                {
                    PunishmentDealtRating = Mathf.Max(0, PunishmentDealtRating - 1);
                }
            }
            else if (Pawn.gender==Gender.Female && Level>0 && Pawn.IsSlave)
            {
                var reachedcap = CurrentXp.Any(v => v.Value >= XPGoal);
                if (reachedcap)
                {
                    var skillname = CurrentXp.First(v => v.Value >= XPGoal).Key;
                    UpgradeSkill(skillname);
                }
            }
            
        }

        public override void PostDeSpawn(Map map)
        {
            RemoveFromRepo(Pawn);
            base.PostDeSpawn(map);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref Level, "subjugate-lvl");
            Scribe_Values.Look(ref isprimed, "subjugate-is-hot");
            Scribe_Values.Look(ref CurrentRating, "subjugate-cur-rat");
            Scribe_Values.Look(ref RatingCap, "subjugate-rat-cap");
            Scribe_Values.Look(ref PunishmentDealtRating, "subjugate-dic-dealt-rat");
            Scribe_Values.Look(ref fortheladies, "subjugate-for-lad");
            Scribe_Values.Look(ref ForTheLadiesMult, "subjugate-for-lad-mult");
            Scribe_Collections.Look(ref PassionRecords, "subj-pas-cols");
            Scribe_Collections.Look(ref DisabledSkills, "subj-dis-skills");
            Scribe_Values.Look(ref XPGoal, "subj-xp-buffer");
            Scribe_Collections.Look(ref CurrentXp, "subj-xp-sys-cur");

            if (CurrentXp == null)
                CurrentXp = new Dictionary<string, float>();

            if (PassionRecords == null)
                PassionRecords = new Dictionary<string, byte>();

            if (DisabledSkills == null)
                DisabledSkills = new List<string>();


        }

        public void Prime()
        {
            if (IsPrimed || Level > 0)
            {
                return;
            }

            IsPrimed = true;
            RemoveFromRepo(Pawn);

            CurrentRating = 0f;
            if (RatingCap == 0)
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
            if (bypawn == null)
            {
                return;
            }


            if (bypawn.IsColonist && !bypawn.IsSlave && bypawn.Ideo != null && bypawn.Ideo.HasPrecept(Defs.SubjugateAllWomen))
            {
                var bypawncomp = CompSubjugate.GetComp(bypawn);
                if (bypawncomp == null)
                {
                    Log.Error("NO COMP FOR BYPAWN");
                    return;
                }

                bypawncomp.PunishmentDealtRating += severity;

                if (Pawn.IsPrisoner)
                {
                    Pawn.guest.will = Mathf.Max(.1f, Pawn.guest.will - severity * .01f);
                }

                if (IsPrimed)
                {
                    CurrentRating = Mathf.Min(RatingCap, CurrentRating + severity * .1f);

                    if (CurrentRating >= RatingCap)
                    {
                        LevelUp();
                        IsPrimed = false;
                    }
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
                WireXPGoal();
            }

            Messages.Message(Pawn + " was subjugated x" + Level, MessageTypeDefOf.PositiveEvent);
        }

        private void ActivateSubjugation()
        {
            DisabledSkills.Add(SkillDefOf.Shooting.defName);
            DisabledSkills.Add(SkillDefOf.Melee.defName);
            var ignored = new string[]
            {
                SkillDefOf.Shooting.defName,
                SkillDefOf.Melee.defName,
            };

            foreach (var skill in Pawn.skills.skills)
            {
                if (ignored.Contains(skill.def.defName))
                    continue;
                if (skill.PermanentlyDisabled)
                    continue;

                var skillname = skill.def.defName;
                byte passion = (byte)skill.passion;
                PassionRecords.Add(skillname, passion);

                if (new byte[] { 2, 4, 5 }.Contains(passion)) /* major, natural, burning */
                {
                    passion = 1;
                }
                else if (passion == 1)
                {
                    passion = 0;
                }
                else if (passion == 0)
                {
                    passion = 3;
                }
                else if (passion == 3) /* apathy */
                {
                    passion = 0;
                    DisabledSkills.Add(skill.def.defName);
                }

                skill.passion = (Passion)passion;

            }

            Pawn.skills.skills.ForEach(v => v.Notify_SkillDisablesChanged());
            Pawn.Notify_DisabledWorkTypesChanged();
            /*clear learnrate factors in skills expanded*/
            if (Subjugate.HasVanillaSkillMod)
                ClearSkillsModCache();

        }
        public void WireXPGoal()
        {
            var totalSkills = 0f;
            var skillcount = 0;
            var ignored = new string[]
            {
                SkillDefOf.Shooting.defName,
                SkillDefOf.Melee.defName,
            };

            CurrentXp.Clear();
            foreach (var skill in Pawn.skills.skills)
            {
                if (ignored.Contains(skill.def.defName))
                    continue;
                totalSkills += TotalXp(skill);
                skillcount++;

                if (PassionRecords.ContainsKey(skill.def.defName))
                    CurrentXp.Add(skill.def.defName, 0f);
            }
            XPGoal = totalSkills / skillcount;
        }
        public void RegisterXP(string skillname, float amnt)
        {
            if (CurrentXp != null && CurrentXp.ContainsKey(skillname))
                CurrentXp[skillname] += amnt;
        }

        private void UpgradeSkill(string skillname)
        {
            var pass = PassionRecords[skillname];
            var skill = Pawn.skills.skills.First(v => v.def.defName == skillname);
            skill.passion = (Passion)pass;

            PassionRecords.Remove(skillname);

            WireXPGoal();

            Pawn.skills.skills.ForEach(v => v.Notify_SkillDisablesChanged());
            Pawn.Notify_DisabledWorkTypesChanged();
            /*clear learnrate factors in skills expanded*/
            if (Subjugate.HasVanillaSkillMod)
                ClearSkillsModCache();
        }
        public WorkTypeDef[] GetEnabledWorkTypes()
        {
            var worktypes = new List<WorkTypeDef>()
            {
                WorkTypeDefOf.Art,
                WorkTypeDefOf.Research,
            };


            return worktypes.ToArray();
        }

        public bool HatesWearingArmor()
        {
            return Level > 0;
        }

        public bool GetIsSkillDisabled(SkillRecord instance)
        {
            return Level > 0 && DisabledSkills.Contains(instance.def.defName);
        }
        public bool GetIsSkillEnabled(SkillRecord instance)
        {
            var skills = new string[] { SkillDefOf.Artistic.defName, SkillDefOf.Intellectual.defName };
            return Level > 0 && skills.Contains(instance.def.defName);
        }


        public static CompSubjugate GetComp(Pawn pawn)
        {

            if (!Repo.ContainsKey(pawn))
            {
                var comp = pawn.GetComp<CompSubjugate>();

                if (comp == null)
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, null, null });
                }
                else if (pawn.gender == Gender.Male && pawn.Ideo != null && pawn.Ideo.HasPrecept(Defs.SubjugateAllWomen))
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, comp, null });
                } 
                else if (pawn.gender == Gender.Female)
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, null, comp });
                }
                else
                {
                    Repo.Add(pawn, new CompSubjugate[] { null, null, null });
                }

            }
            return Repo[pawn][(byte)pawn.gender]; /*0:none, 1:male, 2:female*/
        }
        public static void ClearRepo()
        {
            foreach (var i in Repo.ToList())
            {
                RemoveFromRepo(i.Key);
            }
        }
        public static void RemoveFromRepo(Pawn pawn)
        {
            if (!Repo.ContainsKey(pawn))
                return;

            Repo.Remove(pawn);
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
                else if (!fortheladies && value)
                {
                    Pawn.story.traits.GainTrait(new Trait(Defs.ForTheLadies, 0, true));
                }
                fortheladies = value;
            }
        }

        public string SkillGloalStr { get
            {
                var str = "";
                foreach(var rec in CurrentXp)
                {
                    str += rec.Key + ": " + (rec.Value / XPGoal * 100f) + "%\n";
                }

                return str;
            } }

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
                var occIsSubmissive = occcomp.Level > 0;
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

            return res;
        }
 

        public static void XpToLevel(SkillRecord skill, float resultingxp)
        {
            var xp = 0f;
            for (var i = 0; i < skill.levelInt; i++)
            {

                if (xp <= resultingxp && resultingxp < xp + xpLvlUpData[i])
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

    }

    public class XPSystem : IExposable
    {

        


        

        public void ExposeData()
        {
            
        }
    }

}
