using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate.SubjucationPerks
{
    public class PerkGainXPPool:Perk
    {
        public override string Name => "Gain XP Pool";
        float xpNeededToGainAverageLevel(Pawn pawn, ref int calclvl)
        {
            var numberOfSkills = pawn.skills.skills.Count();
            var totalxp = pawn.skills.skills.Select(v=> XPSystem.TotalXp(v)).Sum();
            var totalAveXp = totalxp / numberOfSkills;

            var xp = 0f;
            for (var i = 0; i < XPSystem.xpLvlUpData.Count(); i++)
            {

                if (xp <= totalAveXp && totalAveXp < xp + XPSystem.xpLvlUpData[i])
                {
                    calclvl = i+1;

                    return XPSystem.xpLvlUpData[i];
                    
                }

                xp += XPSystem.xpLvlUpData[i];
            }

            Log.Error("XP BUFFER CALCULATED 0.");

            return 0;
        }
        public override string NextLevelExplain(Pawn pawn)
        {
            int lvl = 0;
            float i = xpNeededToGainAverageLevel(pawn, ref lvl);
            if (i == 0f)
                return null;
            return $"{pawn} will gain xp pool of {i.ToString("N")} (based on current average lvl:{lvl})";
            
        }
 

        public override void Activate(Pawn pawn)
        {
            int i = 0;
            PoolXp = xpNeededToGainAverageLevel(pawn, ref i);
            Explain = $"{pawn} has reserve xp of {PoolXp.ToString("N")}";
        }

    }
}
