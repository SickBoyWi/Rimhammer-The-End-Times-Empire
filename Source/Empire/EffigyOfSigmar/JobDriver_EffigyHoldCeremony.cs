using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TheEndTimes_Magic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Empire
{

    [StaticConstructorOnStartup]
    public class JobDriver_EffigyHoldCeremony : JobDriver
    {
        public static readonly Texture2D SIGMAR_MOTE_IMAGE = ContentFinder<Texture2D>.Get("Mote/RH_TET_Sigmar", true);

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private const TargetIndex EffigyIndex = TargetIndex.A;

        protected Building_EffigyOfSigmar Effigy
        {
            get { return (Building_EffigyOfSigmar)base.job.GetTarget(TargetIndex.A).Thing; }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_Reserve.Reserve(EffigyIndex);

            yield return new Toil
            {
                initAction = delegate
                {
                    Effigy.ChangeState(Building_EffigyOfSigmar.State.initiating);
                }
            };

            //Toil 1: Go to effigy.
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            //Toil 3: Chant.
            Toil chantingTime = new Toil();
            chantingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            chantingTime.defaultDuration = EmpireUtil.RITUAL_DURATION;
            chantingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            chantingTime.PlaySustainerOrSound(RH_TET_EmpireDefOf.RH_TET_Empire_Chanting);
            Texture2D godSymbol = JobDriver_EffigyHoldCeremony.SIGMAR_MOTE_IMAGE;
            //chantingTime.AddPreTickAction((Action)(() => this.WatchTickAction()));
            chantingTime.initAction = delegate
            {
                // JEH 1.5
                this.pawn.equipment.Primary.DrawNowAt(pawn.Position.ToVector3(), true);

                if (godSymbol != null)
                    MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, godSymbol);
            };
            chantingTime.tickAction = delegate
            {
                // Do anything
            };
            yield return chantingTime;

            // JEH 1.5
            this.AddFinishAction((Action<JobCondition>) (jobCondition =>
            {
                // Give thought.
                if (!Effigy.CanceledInd)
                    this.pawn.needs.mood.thoughts.memories.TryGainMemory(RH_TET_EmpireDefOf.RH_TET_Empire_PraisedSigmar);

                TaleDef taleToAdd = TaleDef.Named("RH_TET_Empire_EffigyGathering");
                if ((this.pawn.IsColonist || this.pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
                {
                    TaleRecorder.RecordTale(taleToAdd, new object[]
                    {
                        this.pawn,
                    });
                }

                Effigy.ChangeState(Building_EffigyOfSigmar.State.powering);
            }));
        }
    }
}