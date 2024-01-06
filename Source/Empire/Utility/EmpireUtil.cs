using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public static class ModProps
    {
        public static string main = "TheEndTimes";
        public static string mod = "Empire";
    }

    public static class EmpireUtil
    {
        public static readonly int RITUAL_DURATION = 1000; // ~15 secs
        public static readonly int RITUAL_CONSIDER_DURATION = 480; // 8 secs

        public static bool IsEmpirePawnKind(PawnKindDef kindDef)
        {
            // TODO Add all Empire pawn kinds here.
            if (kindDef.defName.Equals("RH_TET_EmpireVillager")
                || kindDef.defName.Equals("RH_TET_Empire_WizardScenario")

                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_Beasts")
                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_Bright")
                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_Celestial")
                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_Spirit")
                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_Grey")
                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_Jade")
                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_White")
                || kindDef.defName.Equals("RH_TET_Empire_WizardStandard_Gold")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_Beasts")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_Bright")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_Celestial")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_Spirit")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_Grey")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_Jade")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_White")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreat_Gold")

                || kindDef.defName.Equals("RH_TET_Empire_WizardChaos")
                || kindDef.defName.Equals("RH_TET_Empire_WizardGreatChaos")
                || kindDef.defName.Equals("RH_TET_Empire_Civilian")
                || kindDef.defName.Equals("RH_TET_Empire_Trader")
                || kindDef.defName.Equals("RH_TET_Empire_Flagellant")
                || kindDef.defName.Equals("RH_TET_Empire_FreeCompany_Melee")
                || kindDef.defName.Equals("RH_TET_Empire_FreeCompanyCaptain_Melee")
                || kindDef.defName.Equals("RH_TET_Empire_FreeCompany_Ranged")
                || kindDef.defName.Equals("RH_TET_Empire_FreeCompanyCaptain_Ranged")
                || kindDef.defName.Equals("RH_TET_Empire_Spearman")
                || kindDef.defName.Equals("RH_TET_Empire_Halberdier")
                || kindDef.defName.Equals("RH_TET_Empire_Swordsman")
                || kindDef.defName.Equals("RH_TET_Empire_Greatsword")
                || kindDef.defName.Equals("RH_TET_Empire_Crossbowman")
                || kindDef.defName.Equals("RH_TET_Empire_Handgunner")
                || kindDef.defName.Equals("RH_TET_Empire_Pistolier")
                || kindDef.defName.Equals("RH_TET_Empire_Captain_Melee")
                || kindDef.defName.Equals("RH_TET_Empire_Captain_Ranged")
                || kindDef.defName.Equals("RH_TET_Empire_Witchhunter")
                || kindDef.defName.Equals("RH_TET_Empire_Cook")
                || kindDef.defName.Equals("RH_TET_Empire_FaithfulSigmar")
                || kindDef.defName.Equals("RH_TET_Empire_FaithfulUlric")
                || kindDef.defName.Equals("RH_TET_Empire_FaithfulShallya"))
            {
                return true;
            }
            return false;
        }

        public static bool IsCellInRadius(IntVec3 checkCell, IntVec3 centerOfRadius, float radius)
        {
            return (double)Mathf.Pow((float)(checkCell.x - centerOfRadius.x), 2f) + (double)Mathf.Pow((float)(checkCell.z - centerOfRadius.z), 2f) <= (double)Mathf.Pow(radius, 2f);
        }

        public static IEnumerable<IntVec3> CalculateAllCellsInsideRadius(
          IntVec3 center,
          Map map,
          int radius)
        {
            for (int z = -radius; z <= radius; ++z)
            {
                for (int x = -radius; x <= radius; ++x)
                {
                    IntVec3 cell = new IntVec3(center.x + x, center.y, center.z + z);
                    if (x * x + z * z <= radius * radius && cell.InBounds(map))
                        yield return cell;
                    cell = new IntVec3();
                }
            }
        }

        public static bool IsWallRockDoorOrSolid(Building b)
        {
            if (b == null)
                return false;
            if (b.def.defName.Contains("Wall"))
                return true;
            else if (b.def.building.isNaturalRock)
                return true;
            else if (b.def.fillPercent > .75f)
                return true;
            if (b.def.defName.Contains("Door") || b.def.defName.Contains("Gate"))
                return true;
            return false;
        }

        public static string Prefix => ModProps.main + " :: " + ModProps.mod + " :: ";

        public static void DebugReport(string x)
        {
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                Log.Message(Prefix + x);
            }
        }
        public static bool IsActorAvailable(Pawn actor, bool downedAllowed = false)
        {
            StringBuilder s = new StringBuilder();
            s.Append("ActorAvailble Checks Initiated");
            s.AppendLine();
            if (actor == null)
                return ResultFalseWithReport(s);

            s.Append("ActorAvailble: Passed null Check");
            s.AppendLine();
            if (actor.Dead)
                return ResultFalseWithReport(s);

            s.Append("ActorAvailble: Passed not-dead");
            s.AppendLine();
            if (actor.Downed && !downedAllowed)
                return ResultFalseWithReport(s);

            s.Append("ActorAvailble: Passed downed check & downedAllowed = " + downedAllowed.ToString());
            s.AppendLine();
            if (actor.Drafted)
                return ResultFalseWithReport(s);

            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (actor.InAggroMentalState)
                return ResultFalseWithReport(s);

            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (actor.InMentalState)
                return ResultFalseWithReport(s);

            s.Append("ActorAvailble: Passed InMentalState check");
            s.AppendLine();
            s.Append("ActorAvailble Checks Passed");
            DebugReport(s.ToString());

            return true;
        }

        public static bool ResultFalseWithReport(StringBuilder s)
        {
            s.Append("ActorAvailble: Result = Unavailable");
            DebugReport(s.ToString());
            return false;
        }

        public static void GiveAttendEffigyGatheringJob(Building_EffigyOfSigmar effigy, Pawn attendee)
        {
            if (effigy.IsInstigator(attendee)) return;
            if (!EmpireUtil.IsActorAvailable(attendee)) return;
            if (attendee.jobs.curJob.def == RH_TET_EmpireDefOf.RH_TET_Empire_EffigyGathering) return;
            if (attendee.Drafted) return;
            if (attendee.IsPrisoner) return;

            IntVec3 result;
            Building chair;

            if (!WatchBuildingUtility.TryFindBestWatchCell(effigy, attendee, true, out result, out chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(effigy, attendee, false, out result, out chair))
                {
                    return;
                }
            }

            int dir = effigy.Rotation.Opposite.AsInt;

            if (chair != null)
            {
                IntVec3 newPos = chair.Position + GenAdj.CardinalDirections[dir];

                Job J = new Job(RH_TET_EmpireDefOf.RH_TET_Empire_EffigyGathering, effigy, newPos, chair);
                J.playerForced = true;
                J.ignoreJoyTimeAssignment = true;
                J.expiryInterval = 9999;
                J.ignoreDesignations = true;
                J.ignoreForbidden = true;
                attendee.jobs.TryTakeOrderedJob(J);
            }
            else
            {
                IntVec3 newPos = result + GenAdj.CardinalDirections[dir];

                Job J = new Job(RH_TET_EmpireDefOf.RH_TET_Empire_EffigyGathering, effigy, newPos, result);
                J.playerForced = true;
                J.ignoreJoyTimeAssignment = true;
                J.expiryInterval = 9999;
                J.ignoreDesignations = true;
                J.ignoreForbidden = true;
                attendee.jobs.TryTakeOrderedJob(J);
            }
        }
        public static void SpawnRaidEvent(Building_EffigyOfSigmar effigy, FloatRange strengthRange)
        {
            Map map = effigy.Map;
            Faction enemyFac;
            IntVec3 spawnSpot;

            if (!TryFindSpawnSpot(map, out spawnSpot) || !TryFindEnemyFaction(out enemyFac))
                return;

            int num = Rand.Int;
            float raidStrengthFactor = strengthRange.RandomInRange;
            IncidentCategoryDef cat = IncidentCategoryDefOf.ThreatSmall;
            if (raidStrengthFactor >= 1)
            {
                cat = IncidentCategoryDefOf.ThreatBig;
            }

            IncidentParms incidentParams = StorytellerUtility.DefaultParmsNow(cat, (IIncidentTarget)map);
            incidentParams.forced = true;
            incidentParams.faction = enemyFac;
            incidentParams.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            incidentParams.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            incidentParams.spawnCenter = spawnSpot;
            incidentParams.points = Mathf.Max(StorytellerUtility.DefaultThreatPointsNow(effigy.Map ?? (IIncidentTarget)Find.World) * raidStrengthFactor, enemyFac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            incidentParams.pawnGroupMakerSeed = new int?(num);

            PawnGroupMakerParms pawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParams, false);
            pawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(pawnGroupMakerParms.points, incidentParams.raidArrivalMode, incidentParams.raidStrategy, pawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);

            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, (StorytellerComp)null, incidentParams), Find.TickManager.TicksGame + 1000, 0));
        }

        private static bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot)
        {
            return CellFinder.TryFindRandomEdgeCellWith((Predicate<IntVec3>)(c =>
            {
                if (map.reachability.CanReachColony(c))
                    return !c.Fogged(map);
                return false;
            }), map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        private static bool TryFindEnemyFaction(out Faction enemyFac)
        {
            Faction returnFaction = null;
            Faction beastmenFaction = null;
            Faction chaosMonsterFaction = null;

            foreach (Faction f in Find.FactionManager.AllFactions)
            {
                if (f.def.defName.Equals("RH_TET_Beastmen_GorFaction"))
                {
                    beastmenFaction = f;
                }
                else if (f.def.defName.Equals("RH_TET_ChaosMonsterFaction"))
                {
                    chaosMonsterFaction = f;
                }
            }

            if (RH_TET_EmpireMod.random.Next(0, 4) == 0)
            {
                returnFaction = chaosMonsterFaction;
            }
            else
            {
                returnFaction = beastmenFaction;
            }

            if (returnFaction == null && beastmenFaction != null)
                returnFaction = beastmenFaction;
            else if (returnFaction == null && chaosMonsterFaction != null)
                returnFaction = chaosMonsterFaction;

            if (returnFaction != null)
            {
                enemyFac = returnFaction;
                return true;
            }
            else
                return TryFindEnemyFactionOther(out enemyFac);
        }

        private static bool TryFindEnemyFactionOther(out Faction enemyFac)
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