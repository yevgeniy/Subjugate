using RimWorld;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static HarmonyLib.Code;

namespace Adjustments
{
    public class Driver_ReloadInStorage : JobDriver
    {

        private GunProxy _gun;

        private GunProxy Gun { get
            {
                if (_gun == null)
                    _gun = new GunProxy(this.job.targetB.Thing as ThingWithComps);
                return _gun;
            } }
        private AmmoProxy _ammo;
        private AmmoProxy Ammo { get {
                if (_ammo == null)
                    _ammo = new AmmoProxy(this.job.targetA.Thing as ThingWithComps);
                return _ammo;
            
            } }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {

            var a = pawn.Reserve(Gun.Thing, job);
            var b= pawn.Reserve(Ammo.Thing, job, 1, Mathf.Min(Ammo.Thing.stackCount, job.count), null, errorOnFailed);
            return a && b;

        }
        public override string GetReport()
        {
            return "reloading guns";
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {

            if (Gun.Thing == null)
            {
                Log.Error("No gun in driver");
                yield return null;
            }
            if (Gun.CurrentAmmo!=Ammo.Thing.def)
            {
                Log.Error("Wrong ammo type set for the gun.");
                yield return null;
            }

            AddEndCondition(delegate
            {
                return pawn.Downed || pawn.Dead || pawn.InMentalState || pawn.IsBurning()
                    ? JobCondition.Incompletable
                    : JobCondition.Ongoing;
            });

            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            var pos = Gun.Thing.Position;
            this.FailOn(() =>
            {
                return !Gun.Thing.Position.Equals(pos);
            });

            var toilGoToCell = Toils_Goto.GotoCell(Ammo.Thing.Position, PathEndMode.Touch)
                .FailOnBurningImmobile(TargetIndex.A)
                .FailOnDespawnedNullOrForbidden(TargetIndex.A)
                .FailOnDestroyedNullOrForbidden(TargetIndex.A);
            toilGoToCell.AddEndCondition(() => Ammo.IsCookingOff ? JobCondition.Incompletable : JobCondition.Ongoing);
            yield return toilGoToCell;

            var toilCarryThing = Toils_Haul.StartCarryThing(TargetIndex.A).FailOnBurningImmobile(TargetIndex.A);
            toilCarryThing.AddEndCondition(() => Ammo.IsCookingOff ? JobCondition.Incompletable : JobCondition.Ongoing);
            toilCarryThing.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return toilCarryThing;


            yield return Toils_Goto.GotoCell(Gun.Thing.Position, PathEndMode.Touch);

            var reloadWait= ReloadWaitTask();
            yield return reloadWait;
            yield return ReloadLogicTask();
            yield return ReloadDecideTask(reloadWait);

        }

        private Toil ReloadDecideTask(Toil reloadWait)
        {
            Toil reloadDecide = Toils_Jump.JumpIf(reloadWait, () =>
            {
                return pawn.carryTracker.CarriedThing != null && Gun.CurrentMagCount < Gun.TotalMagCount;
            });

            return reloadDecide;

        }

        private Toil ReloadLogicTask()
        {
            Toil reloadLogic = ToilMaker.MakeToil("reload-logic");
            reloadLogic.initAction = () =>
            {

                int carrying = pawn.carryTracker.CarriedThing.stackCount;
                int currentMag = Gun.CurrentMagCount;
                int total = Gun.TotalMagCount;
                var needed = total - currentMag;
                int toAdd = Gun.OneAtATimeReload ? Mathf.Min(needed, carrying, 1) : Mathf.Min(needed, carrying);

                Gun.AddAmmo(toAdd);
                if (Gun.SoundInteract!=null )
                    Gun.SoundInteract.PlayOneShot(new TargetInfo(Gun.Thing.Position, Find.CurrentMap, false));

                pawn.carryTracker.CarriedThing.stackCount -= toAdd;
                if (pawn.carryTracker.CarriedThing.stackCount <= 0)
                    pawn.carryTracker.DestroyCarriedThing();
            };
            reloadLogic.defaultCompleteMode = ToilCompleteMode.Instant;

            return reloadLogic;
        }

        private Toil ReloadWaitTask()
        {
            Toil reloadWait = ToilMaker.MakeToil("reload-wait");
            reloadWait.defaultCompleteMode = ToilCompleteMode.Delay;
            reloadWait.defaultDuration = Mathf.CeilToInt(Gun.ReloadTime.SecondsToTicks() / pawn.GetStatValue(Adjustments.ReloadSpeed));
            reloadWait.WithProgressBarToilDelay(TargetIndex.B);

            return reloadWait;
        }

    }
}
