using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Empire
{
    public class CaravanArrivalAction_SigmarHammerSite : CaravanArrivalAction_EnterMap
    {
        public static IntVec3 PLAYER_ENTER_LOC = new IntVec3(118, 0, 246);

        public override IntVec3 MapSize
        {
            get
            {
                return new IntVec3(250, 1, 250);
            }
        }

        public CaravanArrivalAction_SigmarHammerSite()
        {
        }

        public CaravanArrivalAction_SigmarHammerSite(MapParent mapParent)
          : base(mapParent)
        {
        }

        public override FloatMenuAcceptanceReport CanVisit(
          Caravan caravan,
          MapParent mapParent)
        {
            if (mapParent == null || !mapParent.Spawned)
                return (FloatMenuAcceptanceReport)false;
            return (FloatMenuAcceptanceReport)true;
        }

        public override Map GetOrGenerateMap(
          int tile,
          IntVec3 mapSize,
          WorldObjectDef suggestedMapParentDef)
        {
            return Current.Game.FindMap(tile) ?? Verse.MapGenerator.GenerateMap(mapSize, this.MapParent, RH_TET_EmpireDefOf.RH_TET_Empire_EmptyMap, (IEnumerable<GenStepWithParams>)null, (Action<Map>)null);
        }

        public override void DoEnter(Caravan caravan)
        {
            Pawn pawn = caravan.PawnsListForReading[0];
            int num = !this.MapParent.HasMap ? 1 : 0;
            Map orGenerateMap = this.GetOrGenerateMap(this.MapParent.Tile, this.MapSize, (WorldObjectDef)null);
            Map map = orGenerateMap;
            IntVec3 enterPos = CaravanArrivalAction_SigmarHammerSite.PLAYER_ENTER_LOC;
            CaravanEnterMapUtility.Enter(caravan, map, (Func<Pawn, IntVec3>)(p => enterPos), CaravanDropInventoryMode.DoNotDrop, true);
            SigmarHammerSiteComp component = this.MapParent.GetComponent<SigmarHammerSiteComp>();

            if (!SigmarHammerSite.playerWon)
            {
                Log.Clear();
                Faction f = GetEnemyFaction();
                this.SetMapSpecificJobs(map.mapPawns.SpawnedPawnsInFaction(GetEnemyFaction()), f, map);
            }
        }

        private Faction GetEnemyFaction()
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_Beastmen_GorFaction);
            return f;
        }

        private void SetMapSpecificJobs(List<Pawn> pawnList, Faction f, Map map)
        {
            // No turrets. Set all to default which is defend base.
        }
    }
}
