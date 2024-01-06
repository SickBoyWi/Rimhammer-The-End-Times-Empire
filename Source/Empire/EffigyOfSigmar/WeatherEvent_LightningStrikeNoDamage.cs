using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Empire
{
    [StaticConstructorOnStartup]
    public class WeatherEvent_LightningStrikeNoDamage : WeatherEvent_LightningFlash
    {
        private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt", -1);
        private IntVec3 strikeLoc = IntVec3.Invalid;
        private Mesh boltMesh;

        public WeatherEvent_LightningStrikeNoDamage(Map map)
          : base(map)
        {
        }

        public WeatherEvent_LightningStrikeNoDamage(Map map, IntVec3 forcedStrikeLoc)
          : base(map)
        {
            this.strikeLoc = forcedStrikeLoc;
        }

        public override void FireEvent()
        {
            base.FireEvent();
            if (!this.strikeLoc.IsValid)
                this.strikeLoc = CellFinderLoose.RandomCellWith((Predicate<IntVec3>)(sq => sq.Standable(this.map) && !this.map.roofGrid.Roofed(sq)), this.map, 1000);
            this.boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (!this.strikeLoc.Fogged(this.map))
            {
                GenExplosion.DoExplosion(this.strikeLoc, this.map, 1.9f, DamageDefOf.Extinguish, (Thing)null, -1, -1f, (SoundDef)null, (ThingDef)null, (ThingDef)null, (Thing)null, (ThingDef)null, 0.0f, 1, new GasType?(), false, (ThingDef)null, 0.0f, 1, 0.0f, false, new float?(), (List<Thing>)null);
                Vector3 vector3Shifted = this.strikeLoc.ToVector3Shifted();
                for (int index = 0; index < 4; ++index)
                {
                    FleckMaker.ThrowSmoke(vector3Shifted, this.map, 1.5f);
                    FleckMaker.ThrowMicroSparks(vector3Shifted, this.map);
                    FleckMaker.ThrowLightningGlow(vector3Shifted, this.map, 1.5f);
                }
            }
            SoundInfo info = SoundInfo.InMap(new TargetInfo(this.strikeLoc, this.map, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        public override void WeatherEventDraw()
        {
            Graphics.DrawMesh(this.boltMesh, this.strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(WeatherEvent_LightningStrikeNoDamage.LightningMat, this.LightningBrightness), 0);
        }
    }
}
