using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjustments.SubjucationPerks
{
    public interface INegSkillPerk
    {
        bool IsDisabled(SkillRecord skill);
        bool Disabled { get; set; }
        SkillDef SkillDef { get; }
    }
}
