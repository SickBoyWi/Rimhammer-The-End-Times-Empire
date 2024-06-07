using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Empire
{
    public class JobDriver_EffigyAttendCeremony : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private TargetIndex Build = TargetIndex.A;
        private TargetIndex Facing = TargetIndex.B;
        private TargetIndex Spot = TargetIndex.C;

        private Pawn setInitiator = null;

        protected Building_EffigyOfSigmar Effigy
        {
            get
            {
                return (Building_EffigyOfSigmar)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Pawn InstigatorPawn
        {
            get
            {
                if (setInitiator != null) return setInitiator;
                if (Effigy.Instigator != null) { setInitiator = Effigy.Instigator; return Effigy.Instigator; }
                else
                {
                    foreach (Pawn pawn in this.pawn.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (pawn.CurJob.def == RH_TET_EmpireDefOf.RH_TET_Empire_EffigyBeginHoldBeginPoweringCere) { setInitiator = pawn; return pawn; }
                    }
                }
                return null;
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.setInitiator, "setInitiator");
            base.ExposeData();
        }


        private string report = "";
        public override string GetReport()
        {
            if (report != "")
            {
                return base.ReportStringProcessed(report);
            }
            return base.GetReport();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            rotateToFace = Build;

            this.AddEndCondition(delegate
            {
                if (InstigatorPawn == null)
                {
                    return JobCondition.Incompletable;
                }
                if (InstigatorPawn.CurJob == null)
                {
                    return JobCondition.Incompletable;
                }

                if (InstigatorPawn.CurJob.def != RH_TET_EmpireDefOf.RH_TET_Empire_EffigyBeginHoldBeginPoweringCere)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });

            this.EndOnDespawnedOrNull(Spot, JobCondition.Incompletable);
            this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);

            yield return Toils_Reserve.Reserve(Spot, 1, -1);

            //Toil 1: Go to the locations
            Toil gotoEffigy;
            if (this.TargetC.HasThing)
            {
                gotoEffigy = Toils_Goto.GotoThing(Spot, PathEndMode.OnCell);
            }
            else
            {
                gotoEffigy = Toils_Goto.GotoCell(Spot, PathEndMode.OnCell);
            }
            yield return gotoEffigy;

            //Toil 2: 'Attend'
            var effigyToil = new Toil();
            effigyToil.defaultCompleteMode = ToilCompleteMode.Delay;
            effigyToil.defaultDuration = EmpireUtil.RITUAL_DURATION;
            effigyToil.AddPreTickAction(() =>
            {
                this.pawn.GainComfortFromCellIfPossible();
                this.pawn.rotationTracker.FaceCell(TargetB.Cell);
                if (report == "") report = "RH_TET_Empire_AttendingEffigyCere".Translate();
                if (InstigatorPawn != null)
                {
                    if (InstigatorPawn.CurJob != null)
                    {
                        if (InstigatorPawn.CurJob.def != RH_TET_EmpireDefOf.RH_TET_Empire_EffigyBeginHoldBeginPoweringCere)
                        {
                            this.ReadyForNextToil();
                        }
                    }
                }
            });
            effigyToil.JumpIf(() => InstigatorPawn.CurJob.def == RH_TET_EmpireDefOf.RH_TET_Empire_EffigyBeginHoldBeginPoweringCere, effigyToil);
            yield return effigyToil;

            yield return new Toil
            {
                initAction = delegate
                {
                    //Do something?
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 3 Reflect on worship
            Toil reflectingTime = new Toil();
            reflectingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            reflectingTime.defaultDuration = EmpireUtil.RITUAL_CONSIDER_DURATION;
            reflectingTime.AddPreTickAction(() =>
            {
                report = "RH_TET_Empire_ReflectingOnCeremony".Translate();
            });
            yield return reflectingTime;

            this.AddFinishAction(jobCondition =>
            {
                // Give thought.
                if (!Effigy.CanceledInd)
                    this.pawn.needs.mood.thoughts.memories.TryGainMemory(RH_TET_EmpireDefOf.RH_TET_Empire_PraisedSigmar);

                if (this.TargetC.Cell.GetEdifice(this.pawn.Map) != null)
                {
                    if (this.pawn.Map.reservationManager.ReservedBy(this.TargetC.Cell.GetEdifice(this.pawn.Map), this.pawn))
                        this.pawn.ClearAllReservations();
                }
                else
                {
                    if (this.pawn.Map.reservationManager.ReservedBy(this.TargetC.Cell.GetEdifice(this.pawn.Map), this.pawn))
                        this.pawn.ClearAllReservations();
                }
            });
        }
    }
}
