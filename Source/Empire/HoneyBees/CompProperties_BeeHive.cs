using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Empire
{
    public class CompProperties_BeeHive : CompProperties
    {
        public int updateTicks = 598;
        public FloatRange activeTempRange = new FloatRange(0.0f, 40f);
        public float rangeThings = 10f;
        public int thingsCountMin = 3;
        public int resourceIntervalDays = 3;
        public List<HoneySeasonProductionMultiplicator> seasonalHoneyProductionMults = new List<HoneySeasonProductionMultiplicator>()
    {
          new HoneySeasonProductionMultiplicator()
          {
            season = Season.PermanentSummer,
            multi = 1f
          },
          new HoneySeasonProductionMultiplicator()
          {
            season = Season.PermanentWinter,
            multi = 0.7f
          },
          new HoneySeasonProductionMultiplicator()
          {
            season = Season.Spring,
            multi = 1f
          },
          new HoneySeasonProductionMultiplicator()
          {
            season = Season.Summer,
            multi = 1f
          },
          new HoneySeasonProductionMultiplicator()
          {
            season = Season.Fall,
            multi = 0.5f
          },
          new HoneySeasonProductionMultiplicator()
          {
            season = Season.Winter,
            multi = -0.1f
          }
        };
        public GatherableResource resources;
    }
}
