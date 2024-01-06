using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    public class IncidentWorker_HuntedWizard : IncidentWorker
    {
        private static readonly IntRange RaidDelay = new IntRange(90000, 125000);
        private static readonly FloatRange RaidPointsFactorRange = new FloatRange(1.8f, 2.0f);
        private const float RelationWithColonistWeight = .05f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            IntVec3 spawnSpot;
            if (!base.CanFireNowSub(parms) || !this.TryFindSpawnSpot((Map)parms.target, out spawnSpot))
                return false;

            Faction ofPlayer = Faction.OfPlayer;

            // Make sure this only happens to Empire.
            if (ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerWizard")
                || ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerColony"))
            {
                Faction enemyFac;
                bool factionFound = this.TryFindEnemyFaction(out enemyFac);
                int wizardCount = 0;

                if (factionFound)
                {
                    // If the player has more than three faith pawns already, don't allow this event.
                    List<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;

                    foreach (Pawn p in playerPawns)
                    {
                        if (p.def != null)
                        {
                            if (p.def.defName.Contains("Wizard"))
                                wizardCount++;
                        }
                        
                        if (wizardCount > 5)
                            break;
                    }

                    if (wizardCount > 5)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Faction enemyFac;
            IntVec3 spawnSpot;
            if (!this.TryFindSpawnSpot(map, out spawnSpot) || !this.TryFindEnemyFaction(out enemyFac))
                return false;
            int num = Rand.Int;
            IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, (IIncidentTarget)map);
            raidParms.forced = true;
            raidParms.faction = enemyFac;
            raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            raidParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            raidParms.spawnCenter = spawnSpot;
            raidParms.points = Mathf.Max(raidParms.points * IncidentWorker_HuntedWizard.RaidPointsFactorRange.RandomInRange, enemyFac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            raidParms.pawnGroupMakerSeed = new int?(num);
            PawnGroupMakerParms pawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms, false);
            pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(pawnGroupMakerParms.points, raidParms.raidArrivalMode, raidParms.raidStrategy, pawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
            IEnumerable<PawnKindDef> pawnKindsExample = PawnGroupMakerUtility.GeneratePawnKindsExample(pawnGroupMakerParms);

            //int randoPawnsCount = RH_TET_EmpireMod.random.Next(2, 5);
            List<Pawn> wizard = new List<Pawn>();

            Pawn refugee = PawnGenerator.GeneratePawn(new PawnGenerationRequest(GetRandomPawnKindDef(), (Faction)null, PawnGenerationContext.NonPlayer, -1,
                false, false,
                false, true, true,
                20f, true, true, true,
                true, false, false,
                false, false, false,
                0.0f, 0.0f, null, 5.0f,
                null, null, null,
                null, null, null,
                null, null, null,
                null, null, null));
            refugee.relations.everSeenByPlayer = true;

            try
            {
                refugee.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            catch
            {
                // Ignore if no ideos present.
            }
            TaggedString text = "RH_TET_Empire_RefugeeChasedMagicInitial".Translate((NamedArgument)refugee.Name.ToStringFull, (NamedArgument)refugee.story.Title, (NamedArgument)enemyFac.def.pawnsPlural, (NamedArgument)enemyFac.Name, (NamedArgument)PawnUtility.PawnKindsToCommaList(pawnKindsExample, true), refugee.Named("PAWN")).AdjustedFor(refugee, "PAWN");
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, refugee);

            wizard.Add(refugee);
            
            DiaNode nodeRoot = new DiaNode(text);
            nodeRoot.options.Add(new DiaOption("RH_TET_Empire_RefugeeChasedInitial_Accept".Translate())
            {
                action = (Action)(() =>
                {
                    foreach (Pawn p in wizard)
                    {
                        GenSpawn.Spawn((Thing)p, spawnSpot, map, WipeMode.Vanish);
                        p.SetFaction(Faction.OfPlayer, (Pawn)null);
                    }

                    CameraJumper.TryJump((GlobalTargetInfo)((Thing)refugee));
                    Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, (StorytellerComp)null, raidParms), Find.TickManager.TicksGame + IncidentWorker_HuntedWizard.RaidDelay.RandomInRange, 0));
                }),
                resolveTree = true
            });
            nodeRoot.options.Add(new DiaOption("RH_TET_Empire_RefugeeChasedInitial_Reject".Translate())
            {
                action = (Action)(() => Find.WorldPawns.PassToWorld(refugee, PawnDiscardDecideMode.Decide)),
                link = new DiaNode("RH_TET_Empire_RefugeeChasedRejected".Translate((NamedArgument)refugee.LabelShort, (NamedArgument)((Thing)refugee)))
                {
                    options = {
                    new DiaOption("OK".Translate()) { resolveTree = true }
                    }
                }
            });

            string title = "RH_TET_Empire_RefugeeChasedMagicTitle".Translate((NamedArgument)map.Parent.Label);
            Find.WindowStack.Add((Window)new Dialog_NodeTreeWithFactionInfo(nodeRoot, enemyFac, true, true, title));
            Find.Archive.Add((IArchivable)new ArchivedDialog(nodeRoot.text, title, enemyFac));
            return true;
        }

        private PawnKindDef GetRandomPawnKindDef()
        {
            if (RH_TET_EmpireMod.random.Next(0, 10) != 0)
                return RH_TET_EmpireMod.STANDARD_WIZARD_PAWN_KINDS.RandomElement();
            else 
                return RH_TET_EmpireMod.GREAT_WIZARD_PAWN_KINDS.RandomElement();
        }

        private bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            return CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)(c =>
            {
                if (map.reachability.CanReachColony(c))
                    return !c.Fogged(map);
                return false;
            }), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        private bool TryFindEnemyFaction(out Faction enemyFac)
        {
            return Find.FactionManager.AllFactions.Where<Faction>((Func<Faction, bool>)(f =>
            {
                if (!f.def.hidden && !f.defeated)
                    return f.HostileTo(Faction.OfPlayer);
                return false;
            })).TryRandomElement<Faction>(out enemyFac);
        }
    }
}
