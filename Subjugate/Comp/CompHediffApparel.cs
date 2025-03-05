using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Subjugate.Comp
{
    public class CompHediffApparel : ThingComp
    {
        public const string AddHediffsToPawnSignal = "HediffApparel_AddHediffsToPawnSignal";
        public const string RemoveHediffsFromPawnSignal = "HediffApparel_RemoveHediffsFromPawnSignal";
        private float lastSeverity;
        private Pawn lastWearer;

        public CompProperties_HediffApparel Props
        {
            get
            {
                return (CompProperties_HediffApparel)this.props;
            }
        }

        public List<Hediff> MyGetHediffs(Pawn pawn)
        {
            IEnumerable<BodyPartRecord> partRecords = this.MyGetPartsToAffect(pawn);
            return pawn.health.hediffSet.hediffs.Where<Hediff>((Func<Hediff, bool>)(d =>
            {
                if (d.def != this.Props.hediffDef)
                    return false;
                return this.Props.global || partRecords.Contains<BodyPartRecord>(d.Part);
            })).ToList<Hediff>();
        }

        public IEnumerable<BodyPartRecord> MyGetPartsToAffect(Pawn pawn)
        {
            List<BodyPartRecord> source1 = new List<BodyPartRecord>();
            if (this.Props.partsToAffect.NullOrEmpty<BodyPartDef>() && this.Props.groupsToAffect.NullOrEmpty<BodyPartGroupDef>())
            {
                source1.Add((BodyPartRecord)null);
            }
            else
            {
                IEnumerable<BodyPartRecord> source2 = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null);
                if (!this.Props.filterTerms.NullOrEmpty<string>())
                {
                    switch (this.Props.filterMode)
                    {
                        case FilterMode.Contains:
                            source2 = source2.Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(r => this.Props.filterTerms.Any<string>((Predicate<string>)(f => r.Label.Contains(f)))));
                            break;
                        case FilterMode.StartsWith:
                            source2 = source2.Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(r => this.Props.filterTerms.Any<string>((Predicate<string>)(f => r.Label.StartsWith(f)))));
                            break;
                        case FilterMode.EndsWith:
                            source2 = source2.Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(r => this.Props.filterTerms.Any<string>((Predicate<string>)(f => r.Label.EndsWith(f)))));
                            break;
                        case FilterMode.Equals:
                            source2 = source2.Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(r => this.Props.filterTerms.Any<string>((Predicate<string>)(f => r.Label.Equals(f)))));
                            break;
                        case FilterMode.Excludes:
                            source2 = source2.Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(r => !this.Props.filterTerms.Any<string>((Predicate<string>)(f => r.Label.Contains(f)))));
                            break;
                    }
                }
                if (!this.Props.partsToAffect.NullOrEmpty<BodyPartDef>())
                    source1.AddRange(source2.Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(p => this.Props.partsToAffect.Contains(p.def))));
                if (!this.Props.groupsToAffect.NullOrEmpty<BodyPartGroupDef>())
                    source1.AddRange(source2.Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(p => this.Props.groupsToAffect.Intersect<BodyPartGroupDef>((IEnumerable<BodyPartGroupDef>)p.groups).Any<BodyPartGroupDef>())));
            }
            return source1.Distinct<BodyPartRecord>();
        }

        private void MyRemoveHediffs(Pawn pawn)
        {
            Log.Message(string.Format("renoving hediffs for: {0}", (object)pawn));
            if (pawn.DestroyedOrNull())
                return;
            foreach (Hediff hediff in this.MyGetHediffs(pawn))
                pawn.health.RemoveHediff(hediff);
        }

        private void MyAddHediffs(Pawn pawn)
        {
            if (pawn.DestroyedOrNull())
                return;
            IEnumerable<BodyPartRecord> partsToAffect = this.MyGetPartsToAffect(pawn);
            if (!partsToAffect.Any<BodyPartRecord>())
                return;
            foreach (BodyPartRecord bodyPartRecord in partsToAffect)
            {
                if (!pawn.health.hediffSet.HasHediff(this.Props.hediffDef, bodyPartRecord, false))
                    pawn.health.AddHediff(HediffMaker.MakeHediff(this.Props.hediffDef, pawn, bodyPartRecord), (BodyPartRecord)null, new DamageInfo?(), (DamageWorker.DamageResult)null);
            }
            this.MyUpdateSeverity(pawn, SeverityMode.Quality);
        }

        public void MyUpdateSeverity(Pawn pawn, SeverityMode severityMode)
        {
            if (severityMode != this.Props.severityMode || pawn.DestroyedOrNull())
                return;
            float num1 = this.Props.hediffDef.initialSeverity;
            switch (severityMode)
            {
                case SeverityMode.Durability:
                    num1 = (float)this.parent.HitPoints / (float)this.parent.MaxHitPoints;
                    break;
                case SeverityMode.Quality:
                    QualityCategory qc;
                    if (!this.parent.TryGetQuality(out qc))
                        Log.Warning("CompHediffApparel.MyUpdateSeverity: severityMode = Quality but " + this.parent.Label + " has no quality.");
                    num1 = ((float)qc + 1f) / (float)Enum.GetNames(typeof(QualityCategory)).Length;
                    break;
            }
            float num2 = (float)Math.Round((double)num1, 3);
            if ((double)this.lastSeverity == (double)num2)
                return;
            foreach (Hediff hediff in this.MyGetHediffs(pawn))
                hediff.Severity = num2;
            this.lastSeverity = num2;
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            this.MyRemoveHediffs(this.lastWearer);
        }

        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case "HediffApparel_AddHediffsToPawnSignal":
                    this.lastWearer = this.parent is Apparel parent ? parent.Wearer : (Pawn)null;
                    this.MyAddHediffs(this.lastWearer);
                    break;
                case "HediffApparel_RemoveHediffsFromPawnSignal":
                    this.MyRemoveHediffs(this.lastWearer);
                    this.lastWearer = (Pawn)null;
                    break;
                default:
                    base.ReceiveCompSignal(signal);
                    break;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Apparel parent = this.parent as Apparel;
            if (parent.Wearer != this.lastWearer)
            {
                this.MyRemoveHediffs(this.lastWearer);
                this.MyAddHediffs(parent.Wearer);
                this.lastWearer = parent.Wearer;
                this.lastSeverity = -1f;
            }
            if (parent.Wearer.DestroyedOrNull() || !parent.Wearer.IsHashIntervalTick(60))
                return;
            this.MyUpdateSeverity(parent.Wearer, SeverityMode.Durability);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Pawn>(ref this.lastWearer, "lastWearer", false);
        }
    }

}
