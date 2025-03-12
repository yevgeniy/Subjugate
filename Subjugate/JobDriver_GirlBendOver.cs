using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace Subjugate
{

    public class JobDriver_GirlBendOver : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //var turncorrectly = new Toil
            //{
            //    initAction = () =>
            //    {

            //        var warden = this.job.targetA.Pawn;

            //        Log.Message($"turn correctly {warden.Rotation} {this.pawn.Rotation}");
            //        //warden.rotationTracker.Face(girl.DrawPos);
            //        warden.rotationTracker.FaceCell(this.pawn.Position);
            //        warden.rotationTracker.UpdateRotation();

            //        Log.Message($"turn correctly {warden.Rotation} {this.pawn.Rotation}");
            //    }
            //};
            var sayouch = new Toil
            {
                initAction = () =>
                {
                    Utils.ThrowMetaIconF(pawn.Position, pawn.Map, Defs.Subj_NoHeart_Fleck);
                }
            };

            //yield return turncorrectly;
            yield return sayouch;
            yield return Toils_General.Wait(100);
            yield return Toils_Jump.Jump(sayouch);
        }


    }

}
