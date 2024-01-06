using RimWorld;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    /* Usage. In comps for thingdef.
			<li Class = "TheEndTimes_Empire.CompProperties_ExpireAndReplace" >
                < compClass > TheEndTimes_Empire.CompExpireAndReplace </ compClass >
                < replaceDef > RH_TET_Empire_Honey </ replaceDef >
                < daysUntilExpire > 1 </ daysUntilRespawn >
                < useTempRange > true </ useTempRange >
                < goodTempRange >
                    < min > -5 </ min >
                    < max > 40 </ max >
                </ goodTempRange >
            </ li >
        */

    public class CompExpireAndReplace : ThingComp
    {
        private CompExpireAndReplace.RespawnReasons reason = CompExpireAndReplace.RespawnReasons.none;
        private bool _active = true;
        private float activeTimeInt;

        public bool Active
        {
            get
            {
                int num;
                if (this.parent.Map != null)
                {
                    IntVec3 position = this.parent.Position;
                    num = 0;
                }
                else
                    num = 1;
                if (num != 0)
                    return false;
                return this._active;
            }
            set
            {
                this._active = value;
            }
        }

        public float ActiveTime
        {
            get
            {
                return this.activeTimeInt;
            }
            set
            {
                this.activeTimeInt = value;
            }
        }

        private CompProperties_ExpireAndReplace PropsTimedRespawn
        {
            get
            {
                return (CompProperties_ExpireAndReplace)this.props;
            }
        }

        public int TicksUntilRespawnAtCurrentTemp
        {
            get
            {
                IntVec3 positionHeld = this.parent.PositionHeld;
                if (this.parent.MapHeld == null)
                    return int.MaxValue;
                if (!this.IsTemperatureValid((float)Mathf.RoundToInt(GenTemperature.GetTemperatureForCell(this.parent.PositionHeld, this.parent.MapHeld))))
                {
                    this.reason = CompExpireAndReplace.RespawnReasons.temperature;
                    return 0;
                }
                int num = Mathf.RoundToInt((float)this.PropsTimedRespawn.TicksToRespawn - this.ActiveTime);
                if (num <= 0)
                    return 0;
                return num;
            }
        }

        private bool IsTemperatureValid(float temperature)
        {
            return !this.PropsTimedRespawn.useTempRange || (double)temperature > (double)this.PropsTimedRespawn.goodTempRange.min && (double)temperature < (double)this.PropsTimedRespawn.goodTempRange.max;
        }

        public void SpawnDefAndVanishSelf()
        {
            if (this.PropsTimedRespawn.replaceDef != null)
                GenSpawn.Spawn(this.PropsTimedRespawn.replaceDef, this.parent.PositionHeld, this.parent.MapHeld, WipeMode.Vanish).SetForbiddenIfOutsideHomeArea();
            if (this.parent.MapHeld.areaManager.Home.ActiveCells.Contains<IntVec3>(this.parent.PositionHeld))
            {
                switch (this.reason)
                {
                    case CompExpireAndReplace.RespawnReasons.temperature:
                        Messages.Message((string)"RH_TET_Empire_QueenBeeDeadTemp".Translate(), (LookTargets)new TargetInfo(this.parent.PositionHeld, this.parent.MapHeld, false), MessageTypeDefOf.NegativeEvent, true);
                        break;
                    case CompExpireAndReplace.RespawnReasons.time:
                        Messages.Message((string)"RH_TET_Empire_QueenBeeDeadTime".Translate(), (LookTargets)new TargetInfo(this.parent.PositionHeld, this.parent.MapHeld, false), MessageTypeDefOf.NeutralEvent, true);
                        break;
                }
            }
            this.parent.Destroy(DestroyMode.Vanish);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this._active, "active", true, false);
            Scribe_Values.Look<float>(ref this.activeTimeInt, "activeTime", 0.0f, false);
        }

        public override void CompTickRare()
        {
            if (!this.Active)
                return;
            this.ActiveTime = (this.ActiveTime += 250f);
            if (this.TicksUntilRespawnAtCurrentTemp > 0)
                return;
            if (this.reason == CompExpireAndReplace.RespawnReasons.none)
                this.reason = CompExpireAndReplace.RespawnReasons.time;
            this.SpawnDefAndVanishSelf();
        }

        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            float t = (float)count / (float)(this.parent.stackCount + count);
            this.ActiveTime = Mathf.Lerp(this.ActiveTime, ((ThingWithComps)otherStack).GetComp<CompExpireAndReplace>().ActiveTime, t);
        }

        public override void PostSplitOff(Thing piece)
        {
            ((ThingWithComps)piece).GetComp<CompExpireAndReplace>().ActiveTime = this.ActiveTime;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if ((double)((float)this.PropsTimedRespawn.TicksToRespawn - this.ActiveTime) > 0.0)
            {
                int respawnAtCurrentTemp = this.TicksUntilRespawnAtCurrentTemp;
                stringBuilder.AppendLine((string)"RH_TET_Empire_TimeRemaining".Translate((NamedArgument)respawnAtCurrentTemp.ToStringTicksToPeriod(true, false, true, true)));
            }
            return stringBuilder.ToString().Trim();
        }

        private enum RespawnReasons
        {
            none,
            temperature,
            time,
        }
    }
}
