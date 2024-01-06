using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TheEndTimes_Empire
{
    [StaticConstructorOnStartup]
    public class Building_MeatSmoker : Building
    {
        private int stuffCount;
        private float progressInt;
        private Material barFilledCachedMat;
        public const int MaxCapacity = 200;
        private const int DurationTillReady = (60000*3); //600000 is 10 days. So 60000 ticks per day.
        public const float MinIdealTemperature = -5f;
        private static readonly Vector2 BarSize = new Vector2(1.0f, 0.1f);
        private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);
        private static readonly Color BarsmokedColor = new Color(0.9f, 0.85f, 0.2f);
        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
        private CompIngredients compIngredients = new CompIngredients();

        public float Progress
        {
            get
            {
                return this.progressInt;
            }
            set
            {
                if (value == this.progressInt)
                {
                    return;
                }
                this.progressInt = value;
                this.barFilledCachedMat = null;
            }
        }

        private Material BarFilledMat
        {
            get
            {
                if (this.barFilledCachedMat == null)
                {
                    this.barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(Building_MeatSmoker.BarZeroProgressColor, Building_MeatSmoker.BarsmokedColor, this.Progress), false);
                }
                return this.barFilledCachedMat;
            }
        }

        public int SpaceLeftForStuff
        {
            get
            {
                if (this.Ready)
                {
                    return 0;
                }
                return MaxCapacity - this.stuffCount;
            }
        }

        private bool Empty
        {
            get
            {
                return this.stuffCount <= 0;
            }
        }

        public bool Ready
        {
            get
            {
                return !this.Empty && this.Progress >= 1f;
            }
        }

        private float CurrentTempProgressSpeedFactor
        {
            get
            {
                CompProperties_TemperatureRuinable compProperties = this.def.GetCompProperties<CompProperties_TemperatureRuinable>();
                float ambientTemperature = base.AmbientTemperature;
                if (ambientTemperature < compProperties.minSafeTemperature)
                {
                    return 0.1f;
                }
                if (ambientTemperature < MinIdealTemperature)
                {
                    return GenMath.LerpDouble(compProperties.minSafeTemperature, MinIdealTemperature, 0.1f, 1f, ambientTemperature);
                }
                return 1f;
            }
        }

        private float ProgressPerTickAtCurrentTemp
        {
            get
            {
                CompRefuelable refuelableComp = this.GetComp<CompRefuelable>();
                if (refuelableComp != null && !refuelableComp.HasFuel)
                    return 0;
                return 5.555556E-06f * this.CurrentTempProgressSpeedFactor;
            }
        }

        private int EstimatedTicksLeft
        {
            get
            {
                return Mathf.Max(Mathf.RoundToInt((1f - this.Progress) / this.ProgressPerTickAtCurrentTemp), 0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.stuffCount, "stuffCount", 0, false);
            Scribe_Values.Look<float>(ref this.progressInt, "progress", 0f, false);
        }
        
        public void TickSmoking()
        {
            //base.TickRare(); This will cause the heat pusher to run more.
            if (!this.Empty)
            {
                this.Progress = Mathf.Min(this.Progress + 200f * this.ProgressPerTickAtCurrentTemp, 1f);
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % 250 == 0)
                this.TickSmoking();
        }
        
        public void AddStuff(int count)
        {
            base.GetComp<CompTemperatureRuinable>().Reset();
            if (this.Ready)
            {
                Log.Warning("Tried to add meat to a meat smoker that is already fully smoked.");
                return;
            }
            int num = Mathf.Min(count, MaxCapacity - this.stuffCount);
            if (num <= 0)
            {
                return;
            }
            this.Progress = GenMath.WeightedAverage(0f, (float)num, this.Progress, (float)this.stuffCount);
            this.stuffCount += num;
        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
            {
                this.Reset();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        private void Reset()
        {
            this.stuffCount = 0;
            this.Progress = 0f;
            this.compIngredients = new CompIngredients();
        }

        public void AddStuff(Thing stuff)
        {
            int num = Mathf.Min(stuff.stackCount, MaxCapacity - this.stuffCount);
            if (num > 0)
            {
                CompIngredients ingredients = stuff.TryGetComp<CompIngredients>();

                for (int i = 0; i < ingredients.ingredients.Count; i++)
                {
                    compIngredients.RegisterIngredient(ingredients.ingredients[i]);
                }

                this.AddStuff(num);
                stuff.SplitOff(num).Destroy(DestroyMode.Vanish);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();
            if (!this.Empty && !comp.Ruined)
            {
                if (this.Ready)
                {
                    stringBuilder.AppendLine("RH_TET_Empire_ContainsSmokingMeat".Translate(new object[]
                    {
                        this.stuffCount,
                        MaxCapacity
                    }));
                }
                else
                {
                    stringBuilder.AppendLine("RH_TET_Empire_ContainsSmoking".Translate(new object[]
                    {
                        this.stuffCount,
                        MaxCapacity
                    }));
                }
            }
            if (!this.Empty)
            {
                if (this.Ready)
                {
                    stringBuilder.AppendLine("RH_TET_Empire_Smoked".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("RH_TET_Empire_SmokingProgress".Translate(new object[]
                    {
                        this.Progress.ToStringPercent(),
                        this.EstimatedTicksLeft.ToStringTicksToPeriod()
                    }));
                    if (this.CurrentTempProgressSpeedFactor != 1f)
                    {
                        stringBuilder.AppendLine("RH_TET_Empire_SmokerOutOfIdealTemperature".Translate(new object[]
                        {
                            this.CurrentTempProgressSpeedFactor.ToStringPercent()
                        }));
                    }
                }
            }
            stringBuilder.AppendLine("Temperature".Translate() + ": " + base.AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "RH_TET_Empire_IdealSmokingTemperature".Translate(),
                ": ",
                MinIdealTemperature.ToStringTemperature("F0"),
                " ~ ",
                comp.Props.maxSafeTemperature.ToStringTemperature("F0")
            }));
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public Thing TakeOutStuff()
        {
            if (!this.Ready)
            {
                Log.Warning("Tried to get smoked meat, but it's not yet smoked.");
                return null;
            }
            Thing thing = ThingMaker.MakeThing(ThingDef.Named("RH_TET_Jerky"), null);

            // Swap ingredients with those actually used.
            CompIngredients thingIngredients = thing.TryGetComp<CompIngredients>();
            thingIngredients.ingredients = compIngredients.ingredients.ListFullCopy();

            thing.stackCount = this.stuffCount;

            this.Reset();
            return thing;
        }

        public override void Draw()
        {
            base.Draw();
            if (!this.Empty)
            {
                Vector3 drawPos = this.DrawPos;
                drawPos.y += 0.046875f;
                drawPos.z += 0.00f;
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest()
                {
                    center = drawPos,
                    size = Building_MeatSmoker.BarSize,
                    fillPercent = (float)this.stuffCount / (float)MaxCapacity,
                    filledMat = this.BarFilledMat,
                    unfilledMat = Building_MeatSmoker.BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode && !this.Empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate
                    {
                        this.Progress = 1f;
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 0",
                    action = delegate
                    {
                        this.Progress = 0f;
                    }
                };
            }
            yield break;
        }
    }
}
