using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class JobDriver_CollectHoney : JobDriver
    {
        private int duration = 700;
        protected const TargetIndex ThingInd = TargetIndex.A;

        protected int Duration
        {
            get
            {
                return this.duration;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, (ReservationLayerDef)null, errorOnFailed);
        }

        protected CompHasGatherableBodyResource GetComp(Thing thing)
        {
            return (CompHasGatherableBodyResource)thing.TryGetComp<CompBeeHive>();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden<JobDriver_CollectHoney>(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, (ReservationLayerDef)null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = this.Duration;
            toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return toil;
            yield return new Toil()
            {
                defaultCompleteMode = ToilCompleteMode.Instant,
                initAction = (Action)(() => this.GetComp(this.job.targetA.Thing).Gathered(this.pawn))
            };
        }
    }
}
