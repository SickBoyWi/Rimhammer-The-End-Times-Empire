using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class JobDriver_TakeMeadOutOfFermenter : JobDriver
    {
        private const TargetIndex FermenterInd = TargetIndex.A;
        private const TargetIndex MeadToHaulInd = TargetIndex.B;
        private const TargetIndex StorageCellInd = TargetIndex.C;
        private const int Duration = 200;

        protected Building_FermenterMead Fermenter =>
            (Building_FermenterMead)job.GetTarget(FermenterInd).Thing;

        public override bool TryMakePreToilReservations(bool yeaa) => pawn.Reserve(Fermenter, job, 1, -1, null);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(FermenterInd);
            this.FailOnBurningImmobile(FermenterInd);
            yield return Toils_Goto.GotoThing(FermenterInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(FermenterInd)
                .FailOnCannotTouch(FermenterInd, PathEndMode.Touch).FailOn(() => !Fermenter.Fermented)
                .WithProgressBarToilDelay(FermenterInd, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing thing = Fermenter.TakeOutMead();
                    GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near, null);
                    StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                    if (StoreUtility.TryFindBestBetterStoreCellFor(thing,
                        pawn, Map, currentPriority, pawn.Faction, out var c, true))
                    {
                        job.SetTarget(TargetIndex.C, c);
                        job.SetTarget(TargetIndex.B, thing);
                        job.count = thing.stackCount;
                    }
                    else
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return Toils_Reserve.Reserve(MeadToHaulInd, 1, -1, null);
            yield return Toils_Reserve.Reserve(StorageCellInd, 1, -1, null);
            yield return Toils_Goto.GotoThing(MeadToHaulInd, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(MeadToHaulInd, false, false, false);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carryToCell, true);
            yield break;
        }
    }
}