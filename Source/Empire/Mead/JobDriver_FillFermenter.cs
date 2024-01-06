using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class JobDriver_FillFermenter : JobDriver
    {
        private const TargetIndex FermenterInd = TargetIndex.A;
        private const TargetIndex MustInd = TargetIndex.B;
        private const int Duration = 200;

        protected Building_FermenterMead Fermenter => (Building_FermenterMead)this.job.GetTarget(FermenterInd).Thing;
        protected Thing Must => this.job.GetTarget(MustInd).Thing;

        public override bool TryMakePreToilReservations(bool yeaa) =>
            this.pawn.Reserve(this.Fermenter, this.job, 1, -1, null) && this.pawn.Reserve(this.Must, this.job, 1, -1, null);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(FermenterInd);
            this.FailOnBurningImmobile(FermenterInd);
            base.AddEndCondition(() => (Fermenter.SpaceLeftForMust > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = Fermenter.SpaceLeftForMust;
            });
            Toil reserveMust = Toils_Reserve.Reserve(MustInd, 1, -1, null);
            yield return reserveMust;
            yield return Toils_Goto.GotoThing(MustInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(MustInd).FailOnSomeonePhysicallyInteracting(MustInd);
            yield return Toils_Haul.StartCarryThing(MustInd, false, true, false).FailOnDestroyedNullOrForbidden(MustInd);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveMust, MustInd, TargetIndex.None, true, null);
            yield return Toils_Goto.GotoThing(FermenterInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(MustInd).FailOnDestroyedNullOrForbidden(FermenterInd).FailOnCannotTouch(FermenterInd, PathEndMode.Touch).WithProgressBarToilDelay(FermenterInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Fermenter.AddMust(Must);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

    }
}
