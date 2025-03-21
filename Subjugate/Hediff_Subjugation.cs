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
        private float resistance;
        public float Resistance
        {
            get
            {
                if (this.resistance == default(float))
                {
                    this.resistance = this.pawn.GenerateResistance();
                }
                return this.resistance;
            }

        }

        public Pawn Girl => this.Girl;

        public override bool ShouldRemove => !IsValid;

        bool IsValid => this.pawn.IsSlave;

        public void AddSeverity(float s)
        {
            var rat = ResistanceRat();

            var ss = s * rat;

            this.Severity += ss;
        }

        private float ResistanceRat()
        {
            var r = (Resistance - 10f) * .1f;
            if (r == 0f)
                return 1;
            if (r < 0f)
                return r + 1f;

            return r;

        }

        public void RemoveSeverity(float s)
        {
            this.Severity -= s;
        }
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref this.resistance, "hed-subj-resist");
        }
    }


}
