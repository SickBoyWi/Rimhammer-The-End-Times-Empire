using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    public class CompBeeHive : CompHasGatherableBodyResource
    {
        private Season lastSearchedSeason = Season.Undefined;
        private float lastFoundMulti = 1f;
        private const int doUpdateAfterXTicks = 50;
        private List<Thing> foundThingsInt;

        protected override bool Active
        {
            get
            {
                return this.IsTemperatureGoodOrFull();
            }
        }

        private bool IsTemperatureGoodOrFull()
        {
            if (!this.parent.Spawned || this.parent.Map == null)
                return false;
            if (this.fullness >= 1.0)
                return true;
            float num = (float)Mathf.RoundToInt(GenTemperature.GetTemperatureForCell(this.parent.PositionHeld, this.parent.Map));
            return (double)num > (double)this.Props.activeTempRange.min && (double)num < (double)this.Props.activeTempRange.max;
        }

        private float SeasonGrowthMultiplicator
        {
            get
            {
                if (!this.parent.Spawned || this.parent.Map == null)
                    return 0.0f;
                Season season = GenLocalDate.Season(this.parent.Map);
                if (season == this.lastSearchedSeason)
                    return this.lastFoundMulti;
                float num = 1f;
                foreach (HoneySeasonProductionMultiplicator seasonMultiplicator in this.Props.seasonalHoneyProductionMults)
                {
                    if (seasonMultiplicator.season == season)
                    {
                        num = seasonMultiplicator.multi;
                        break;
                    }
                }
                this.lastFoundMulti = num;
                this.lastSearchedSeason = season;
                return num;
            }
        }

        public CompProperties_BeeHive Props
        {
            get
            {
                return (CompProperties_BeeHive)this.props;
            }
        }

        protected override int GatherResourcesIntervalDays
        {
            get
            {
                return this.Props.resourceIntervalDays;
            }
        }

        protected override string SaveKey
        {
            get
            {
                return "honeyProdGrowth";
            }
        }

        protected override int ResourceAmount
        {
            get
            {
                if (this.Props == null || this.Props.resources == null)
                    return 0;
                return this.Props.resources.resourceCount;
            }
        }

        protected override ThingDef ResourceDef
        {
            get
            {
                if (this.Props == null || this.Props.resources == null)
                    return (ThingDef)null;
                return this.Props.resources.resourceDef;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override void CompTick()
        {
            if (this.parent.Map == null || !this.parent.Spawned)
                return;
            if (this.parent.IsHashIntervalTick(50) && this.Active)
            {
                float num = 0.25f;
                if (this.foundThingsInt != null && this.foundThingsInt.Count > this.Props.thingsCountMin)
                    num = 1f;
                this.fullness = this.Fullness + (float)(1.0 / ((double)this.GatherResourcesIntervalDays * 60000.0)) * num * this.SeasonGrowthMultiplicator * 50f;
                if ((double)this.Fullness > 1.0)
                    this.fullness = 1f;
                if ((double)this.Fullness < 0.00999999977648258)
                    this.fullness = 0.01f;
            }
            this.SearchForFlowers(this.parent.Map, false);
        }

        public override string CompInspectStringExtra()
        {
            string str1 = "";
            if (!this.IsTemperatureGoodOrFull())
                str1 = "\n" + (string)"OutOfIdealTemperatureRangeNotGrowing".Translate() + " ( " + this.Props.activeTempRange.min.ToStringTemperature("F0") + "~" + this.Props.activeTempRange.max.ToStringTemperature("F0") + " )";
            string str2 = "";
            if (this.ActiveAndFull)
                str2 = "\n" + (string)"ReadyToHarvest".Translate();
            return (string)"RH_TET_Empire_FlowersAvailable".Translate() + ": " + (this.foundThingsInt == null ? 0.ToString() : this.foundThingsInt.Count.ToString()) + " / " + (this.Props == null ? "999" : this.Props.thingsCountMin.ToString()) + "\n" + (string)"RH_TET_Empire_HoneyReady".Translate() + ": " + ((double)this.Fullness < 0.1 ? "< 10%" : this.Fullness.ToStringPercent()) + str1 + str2;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                Gizmo c = gizmo;
                yield return c;
                c = (Gizmo)null;
            }
            int baseGroupKey = 3137645;
            if (DebugSettings.godMode)
            {
                Command_Action cmd1 = new Command_Action();
                cmd1.icon = (Texture2D)null;
                cmd1.defaultLabel = "Debug: Honey 99%";
                cmd1.defaultDesc = "Debug: Honey 99%";
                cmd1.hotKey = KeyBindingDefOf.Misc2;
                cmd1.activateSound = SoundDef.Named("Click");
                cmd1.action = (Action)(() => this.fullness = 0.99f);
                cmd1.Disabled = false;
                cmd1.disabledReason = "";
                cmd1.groupKey = baseGroupKey + 1;
                yield return (Gizmo)cmd1;
                Command_Action cmd2 = new Command_Action();
                cmd2.icon = (Texture2D)null;
                cmd2.defaultLabel = "Debug: Find flowers";
                cmd2.defaultDesc = "Debug: Find flowers";
                cmd2.hotKey = KeyBindingDefOf.Misc3;
                cmd2.activateSound = SoundDef.Named("Click");
                cmd2.action = (Action)(() => this.SearchForFlowers(this.parent.Map, true));
                cmd2.Disabled = false;
                cmd2.disabledReason = "";
                cmd2.groupKey = baseGroupKey + 2;
                yield return (Gizmo)cmd2;
                Command_Action cmd3 = new Command_Action();
                cmd3.icon = (Texture2D)null;
                cmd3.defaultLabel = "Debug: Highlight flowers";
                cmd3.defaultDesc = "Debug: Highlight flowers";
                cmd3.hotKey = KeyBindingDefOf.Misc4;
                cmd3.activateSound = SoundDef.Named("Click");
                cmd3.action = (Action)(() => this.ShowAllValidFlowers(this.parent.Map));
                cmd3.Disabled = false;
                cmd3.disabledReason = "";
                cmd3.groupKey = baseGroupKey + 3;
                yield return (Gizmo)cmd3;
                cmd1 = (Command_Action)null;
                cmd2 = (Command_Action)null;
                cmd3 = (Command_Action)null;
            }
        }

        private void ShowAllValidFlowers(Map map)
        {
            if (this.foundThingsInt == null)
                return;
            foreach (Thing thing in this.foundThingsInt)
                FleckMaker.Static(thing.Position, map, FleckDefOf.FeedbackGoto, 1f);
        }

        private void SearchForFlowers(Map map, bool forced = false)
        {
            int updateTicks = this.Props.updateTicks;
            if (Find.TickManager.CurTimeSpeed >= TimeSpeed.Superfast && (GenLocalDate.Season(this.parent.Map) == Season.Winter || this.foundThingsInt != null && this.foundThingsInt.Count > 1))
                updateTicks += 5000;
            if (!forced && !this.parent.IsHashIntervalTick(updateTicks))
                return;
            this.foundThingsInt = this.FindValidThingsInRange(map).ToList<Thing>();
        }

        private IEnumerable<Thing> FindValidThingsInRange(Map map)
        {
            Room room = this.parent.GetRoom(RegionType.Set_Passable);
            IEnumerable<Thing> foundThings = map.listerThings.AllThings.Where<Thing>((Func<Thing, bool>)(t =>
            {
                if (!t.def.blockWind && t.def.plant != null && (!t.def.plant.IsTree && !t.def.defName.ToLower().Contains("grass")) && ((t.def.plant.Harvestable || (double)t.GetStatValue(StatDefOf.Beauty, true) >= 2.0) && (t.GetRoom(RegionType.Set_Passable) == room && t is Plant)))
                    return (double)(t as Plant).Growth > 0.0700000002980232;
                return false;
            }));
            if (foundThings != null)
            {
                foreach (Thing thing in foundThings)
                {
                    Thing t = thing;
                    if (EmpireUtil.IsCellInRadius(t.Position, this.parent.PositionHeld, this.Props.rangeThings))
                        yield return t;
                    t = (Thing)null;
                }
            }
        }
    }
}
