using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace TheEndTimes_Empire
{
    public class QuestNode_GetSiteTileHammer : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;
        public SlateRef<bool> preferCloserTiles;
        public SlateRef<bool> allowCaravans;

        protected override bool TestRunInt(Slate slate)
        {
            if (SigmarHammerSite.playerWon)
            {
                return false;
            }

            PlanetTile tile;
            if (!this.TryFindTile(slate, out tile))
                return false;
            slate.Set<PlanetTile>(this.storeAs.GetValue(slate), tile, false);
            return true;
        }

        protected override void RunInt()
        {
            if (SigmarHammerSite.playerWon)
            {
                return;
            }

            Slate slate = RimWorld.QuestGen.QuestGen.slate;
            PlanetTile tile;
            if (!this.TryFindTile(RimWorld.QuestGen.QuestGen.slate, out tile))
                return;
            RimWorld.QuestGen.QuestGen.slate.Set<PlanetTile>(this.storeAs.GetValue(slate), tile, false);
        }

        private bool TryFindTile(Slate slate, out PlanetTile tile)
        {
            if (SigmarHammerSite.playerWon)
            {
                tile = PlanetTile.Invalid;
                return false;
            }

            Map map = slate.Get<Map>("map", (Map)null, false) ?? Find.RandomPlayerHomeMap;
            PlanetTile nearThisTile1 = map != null ? map.Tile : PlanetTile.Invalid;
            IntRange var;
            if (slate.TryGet<IntRange>("siteDistRange", out var, false))
                return QuestNode_GetSiteTileHammer.TryFindNewSiteTile(out tile, var.min, var.max, this.allowCaravans.GetValue(slate), this.preferCloserTiles.GetValue(slate), nearThisTile1);

            bool flag = this.preferCloserTiles.GetValue(slate);
            int num1 = this.allowCaravans.GetValue(slate) ? 1 : 0;
            int num2 = flag ? 1 : 0;
            PlanetTile nearThisTile2 = nearThisTile1;
            return QuestNode_GetSiteTileHammer.TryFindNewSiteTile(out tile, 12, 30, num1 != 0, num2 != 0, nearThisTile2);
        }

        public static bool TryFindNewSiteTile(out PlanetTile tile, int minDist = 8, int maxDist = 30,
            bool allowCaravans = false, bool preferCloserTiles = false, int nearThisTile = -1)
        {
            if (SigmarHammerSite.playerWon)
            {
                tile = PlanetTile.Invalid;
                return false;
            }

            Func<PlanetTile, PlanetTile> findTile = delegate (PlanetTile root)
            {
                int minDist2 = minDist;
                int maxDist2 = maxDist;
                Predicate<PlanetTile> validator = (PlanetTile x) =>
                    !Find.WorldObjects.AnyWorldObjectAt(x)
                    && Find.WorldGrid[x].hilliness == Hilliness.Flat
                    && IsValidTileForNewSettlement(x, null);
                bool preferCloserTiles2 = preferCloserTiles;
                PlanetTile result;
                if (TileFinder.TryFindPassableTileWithTraversalDistance(root, minDist2, maxDist2, out result, validator,
                    false, TileFinderMode.Random, false, false))
                {
                    return result;
                }
                return PlanetTile.Invalid;
            };

            PlanetTile arg;
            if (nearThisTile != PlanetTile.Invalid)
            {
                arg = nearThisTile;
            }
            else if (!TileFinder.TryFindRandomPlayerTile(out arg, allowCaravans, (PlanetTile x) => findTile(x) != PlanetTile.Invalid))
            {
                tile = PlanetTile.Invalid;
                return false;
            }
            tile = findTile(arg);
            return tile != PlanetTile.Invalid;
        }









        //public static bool TryFindNewSiteTile(
        //  out PlanetTile tile,
        //  PlanetTile nearTile,
        //  int minDist = 8, int maxDist = 30,
        //  bool allowCaravans = false,
        //  List<LandmarkDef> allowedLandmarks = null,
        //  float selectLandmarkChance = 0.5f,
        //  bool canSelectComboLandmarks = true,
        //  TileFinderMode tileFinderMode = TileFinderMode.Near,
        //  bool exitOnFirstTileFound = false,
        //  bool canBeSpace = false,
        //  PlanetLayer layer = null,
        //  Predicate<PlanetTile> validator = null)
        //{
        //    bool pickLandmark = ModsConfig.OdysseyActive && Rand.ChanceSeeded(selectLandmarkChance, Gen.HashCombineInt(Find.TickManager.TicksGame, 18271));
        //    if (!nearTile.Valid && !TileFinder.TryFindRandomPlayerTile(out nearTile, allowCaravans, (Predicate<PlanetTile>)null, true, (PlanetLayer)null))
        //    {
        //        tile = PlanetTile.Invalid;
        //        return false;
        //    }
        //    if (layer == null)
        //        layer = nearTile.Layer;
        //    if (!canBeSpace && layer.Def.isSpace && !Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(nearTile, PlanetLayerDefOf.Surface, out layer))
        //    {
        //        int num;
        //        PlanetLayer planetLayer;
        //        ((IEnumerable<KeyValuePair<int, PlanetLayer>>)Find.WorldGrid.PlanetLayers).Where<KeyValuePair<int, PlanetLayer>>((Func<KeyValuePair<int, PlanetLayer>, bool>)(t => !t.Value.Def.isSpace)).RandomElement<KeyValuePair<int, PlanetLayer>>().Deconstruct(ref num, ref planetLayer);
        //        layer = planetLayer;
        //    }
        //    FastTileFinder.LandmarkMode landmarkMode = pickLandmark ? FastTileFinder.LandmarkMode.Required : FastTileFinder.LandmarkMode.Any;
        //    FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(nearTile, (float)minDist, (float)maxDist, landmarkMode, true, Hilliness.Undefined, Hilliness.Undefined, true, true, canSelectComboLandmarks);
        //    List<PlanetTile> planetTileList = layer.FastTileFinder.Query(query, (List<BiomeDef>)null, allowedLandmarks, new FastTileFinder.TileQueryParams());
        //    if (validator != null)
        //    {
        //        for (int index = planetTileList.Count - 1; index >= 0; --index)
        //        {
        //            if (!validator(planetTileList[index]))
        //                planetTileList.RemoveAt(index);
        //        }
        //    }
        //    if (planetTileList.Empty<PlanetTile>())
        //    {
        //        if (TileFinder.TryFillFindTile(layer.GetClosestTile_NewTemp(nearTile, false), out tile, minDist, maxDist, allowedLandmarks, canSelectComboLandmarks, tileFinderMode, exitOnFirstTileFound, validator, pickLandmark))
        //            return true;
        //        tile = PlanetTile.Invalid;
        //        return false;
        //    }
        //    tile = planetTileList.RandomElement<PlanetTile>();
        //    return true;
        //}








        public static bool IsValidTileForNewSettlement(PlanetTile tile, StringBuilder reason = null)
        {
            Tile tile1 = Find.WorldGrid[tile];
            if (!tile1.PrimaryBiome.canBuildBase)
            {
                reason?.Append("CannotLandBiome".Translate((NamedArgument)tile1.PrimaryBiome.LabelCap));
                return false;
            }
            if (!tile1.PrimaryBiome.implemented)
            {
                reason?.Append("BiomeNotImplemented".Translate() + ": " + tile1.PrimaryBiome.LabelCap);
                return false;
            }
            Settlement settlementBase = Find.WorldObjects.SettlementBaseAt(tile);
            if (settlementBase != null)
            {
                if (reason != null)
                {
                    if (settlementBase.Faction == null)
                        reason.Append("TileOccupied".Translate());
                    else if (settlementBase.Faction == Faction.OfPlayer)
                        reason.Append("YourBaseAlreadyThere".Translate());
                    else
                        reason.Append("BaseAlreadyThere".Translate((NamedArgument)settlementBase.Faction.Name));
                }
                return false;
            }
            if (Find.WorldObjects.AnySettlementBaseAtOrAdjacent(tile))
            {
                reason?.Append("FactionBaseAdjacent".Translate());
                return false;
            }
            if (!Find.WorldObjects.AnyMapParentAt(tile) && Current.Game.FindMap(tile) == null && !Find.WorldObjects.AnyWorldObjectOfDefAt(WorldObjectDefOf.AbandonedSettlement, tile))
                return true;
            reason?.Append("TileOccupied".Translate());
            return false;
        }
    }
}
