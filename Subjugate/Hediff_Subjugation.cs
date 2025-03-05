using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate
{
    public class Hediff_Subjugation : HediffWithComps
    {
        public Pawn Girl => this.Girl;


        public override bool ShouldRemove => false;

        public void AddSeverity(float s)
        {
            this.Severity += s;
        }
        public void RemoveSeverity(float s)
        {
            this.Severity -= s;
        }

    }


}
