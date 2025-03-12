using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace Subjugate
{

    public class JobDriver_GirlStop : JobDriver
    {
        private Toil gotoLocation;
        private Action atLocation;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var wait = Toils_General.Wait(9999999);
            yield return wait;

            /*to make girl goto different location*/
            this.gotoLocation = Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
            yield return gotoLocation;

            yield return Toils_General.Wait(100);

            yield return Toils_General.Do(() => this.atLocation());

            yield return Toils_Jump.Jump(wait);
        }

        public void GoToLocation(IntVec3 location, Action value)
        {
            this.atLocation = value;
            this.job.SetTarget(TargetIndex.C, location);
            this.JumpToToil(this.gotoLocation);
        }
    }


}
