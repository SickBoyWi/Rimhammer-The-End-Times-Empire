using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using Verse;

namespace TheEndTimes_Empire
{
    public class GenStep_FaithfulPrisonerWillingToJoin : GenStep_Scatterer
    {
        private const int Size = 8;

        public override int SeedPart
        {
            get
            {
                return 69356100;
            }
        }

        protected override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (!base.CanScatterAt(c, map) || !c.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) || !map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                return false;
            foreach (IntVec3 c1 in CellRect.CenteredOn(c, 8, 8))
            {
                if (!c1.InBounds(map) || c1.GetEdifice(map) != null)
                    return false;
            }
            return true;
        }

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            Faction hostFaction = map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer ? Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined) : map.ParentFaction;
            CellRect var = CellRect.CenteredOn(loc, 8, 8).ClipInsideMap(map);
            Pawn pawn;
            if (parms.sitePart != null && parms.sitePart.things != null && parms.sitePart.things.Any)
            {
                pawn = (Pawn)parms.sitePart.things.Take(parms.sitePart.things[0]);
            }
            else
            {
                PrisonerWillingToJoinComp component = map.Parent.GetComponent<PrisonerWillingToJoinComp>();
                pawn = component == null || !component.pawn.Any ? SitePartWorker_FaithfulPrisonerWillingToJoin.GenerateFaithfulPrisoner(map.Tile, hostFaction) : component.pawn.Take((Thing)component.pawn[0]);
            }
            ResolveParams resolveParams1 = new ResolveParams();
            resolveParams1.rect = var;
            resolveParams1.faction = hostFaction;
            RimWorld.BaseGen.BaseGen.globalSettings.map = map;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("prisonCell", resolveParams1, (string)null);
            RimWorld.BaseGen.BaseGen.Generate();
            ResolveParams resolveParams2 = new ResolveParams();
            resolveParams2.rect = var;
            resolveParams2.faction = hostFaction;
            resolveParams2.singlePawnToSpawn = pawn;
            resolveParams2.singlePawnSpawnCellExtraPredicate = (Predicate<IntVec3>)(x => x.GetDoor(map) == null);
            resolveParams2.postThingSpawn = (Action<Thing>)(x =>
            {
                MapGenerator.rootsToUnfog.Add(x.Position);
                ((Pawn)x).mindState.WillJoinColonyIfRescued = true;
            });
            RimWorld.BaseGen.BaseGen.globalSettings.map = map;
            RimWorld.BaseGen.BaseGen.symbolStack.Push("pawn", resolveParams2, (string)null);
            RimWorld.BaseGen.BaseGen.Generate();
            MapGenerator.SetVar<CellRect>("RectOfInterest", var);
        }
    }
}
