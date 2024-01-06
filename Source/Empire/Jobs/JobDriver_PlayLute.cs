using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class JobDriver_PlayLute : JobDriver_SitFacingBuilding
    {
        public Building_Lute Lute
        {
            get
            {
                return (Building_Lute)(Thing)this.job.GetTarget(TargetIndex.A);
            }
        }

        protected override void ModifyPlayToil(Toil toil)
        {
            if (!this.pawn.CanReserve(this.Lute))
                return;
            base.ModifyPlayToil(toil);
            toil.AddPreInitAction((Action)(() => this.Lute.StartPlaying(this.pawn)));
            toil.AddFinishAction((Action)(() => this.Lute.StopPlaying()));
        }
    }
}