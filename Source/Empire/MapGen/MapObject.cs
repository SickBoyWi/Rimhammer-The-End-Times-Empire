using System.Collections.Generic;
using Verse;

namespace TheEndTimes_Empire
{
    public class MapObject
    {
        public ThingData key;
        public List<IntVec3> value;

        public override string ToString()
        {
            return $"key:{key}, value:{value}";
        }
    }
}
