using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Subjugate
{
    public class Hediff_Binders : HediffWithComps
    {
        public Pawn Girl => this.Girl;

        private int takedown = 0;
        public void TakeDown()
        {
            this.takedown = 200;
        }

        public override void Tick()
        {
            base.Tick();

            takedown--;
            if (takedown > 0)
            {
                this.Severity = 1f;
            }
            else
            {
                this.Severity = .01f;
            }

        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (dinfo.Def.defName=="Blunt")
            {
                TakeDown();
            }
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref takedown, "hed-binders-shocked");

        }


    }


}
