using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    public class CompProperties_ExpireAndReplace : CompProperties
    {
        public float daysUntilExpire = 2f;
        public ThingDef replaceDef = (ThingDef)null;
        public bool useTempRange = false;
        public IntRange goodTempRange = new IntRange(-9999, 9999);

        public int TicksToRespawn
        {
            get
            {
                return Mathf.RoundToInt(this.daysUntilExpire * 60000f);
            }
        }

        public CompProperties_ExpireAndReplace()
        {
            this.compClass = typeof(CompExpireAndReplace);
        }

        public CompProperties_ExpireAndReplace(float daysToRespawn)
        {
            this.daysUntilExpire = daysToRespawn;
        }
    }
}
