using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    public class Placeworker_ShowBeeHiveRange : PlaceWorker
    {
        public override void DrawGhost(
          ThingDef def,
          IntVec3 center,
          Rot4 rot,
          Color ghostCol,
          Thing thing = null)
        {
            CompProperties_BeeHive propertiesBeeHive = def.comps.Where<CompProperties>((Func<CompProperties, bool>)(c => c is CompProperties_BeeHive)).FirstOrDefault<CompProperties>() as CompProperties_BeeHive;
            if (propertiesBeeHive == null)
                Log.Warning("Placeworker_ShowBeeHiveRange -- No comp, cannot show radius.");
            else
                GenDraw.DrawFieldEdges(new List<IntVec3>(EmpireUtil.CalculateAllCellsInsideRadius(center, Find.CurrentMap, Mathf.RoundToInt(propertiesBeeHive.rangeThings))), ghostCol);
        }
    }
}
