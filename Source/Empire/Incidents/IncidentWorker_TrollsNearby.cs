using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    public class IncidentWorker_TrollsNearby : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            return !target.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && (target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(RH_TET_EmpireDefOf.RH_TET_TrollRace) || target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(RH_TET_EmpireDefOf.RH_TET_StoneTrollRace) || target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(RH_TET_EmpireDefOf.RH_TET_RiverTrollRace)) && this.TryFindEntryCell(target, out IntVec3 _);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            IntVec3 cell;
            if (!this.TryFindEntryCell(target, out cell))
                return false;

            PawnKindDef animal = RH_TET_EmpireDefOf.RH_TET_Troll;
            if (target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(RH_TET_EmpireDefOf.RH_TET_RiverTrollRace) && RH_TET_EmpireMod.random.Next(0, 3) == 0)
            {
                animal = RH_TET_EmpireDefOf.RH_TET_RiverTroll;
            }
            else if (target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(RH_TET_EmpireDefOf.RH_TET_StoneTrollRace) && RH_TET_EmpireMod.random.Next(0, 3) == 0)
            {
                animal = RH_TET_EmpireDefOf.RH_TET_StoneTroll;
            }
            int num1 = Mathf.Clamp(GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow((IIncidentTarget)target) / animal.combatPower), 2, Rand.RangeInclusive(3, 6));
            IntVec3 result = cell;
            Pawn pawn = (Pawn)null;
            int minAge = 50;
            int maxAge = 80;
            for (int index = 0; index < num1; ++index)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(cell, target, 10, (Predicate<IntVec3>)null);

                float biologicalAge = (float)RH_TET_EmpireMod.random.Next(minAge, maxAge);

                PawnGenerationRequest pawnRequest = new PawnGenerationRequest(animal, null, PawnGenerationContext.NonPlayer, -1,
                    true, false,
                    false, false, true,
                    0f, false, true, false,
                    false, false, false,
                    false, false, false,
                    0.0f, 0.0f, null, 0.0f,
                    null, null, null,
                    null, null, biologicalAge,
                    biologicalAge, null, null,
                    null, null, null);

                pawn = PawnGenerator.GeneratePawn(pawnRequest);
                GenSpawn.Spawn((Thing)pawn, loc, target, Rot4.Random, WipeMode.Vanish, false);
                if (result.IsValid)
                    pawn.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(result, target, 10, (Predicate<IntVec3>)null);
            }
            this.SendStandardLetter("RH_TET_LabelTrollsWandersIn".Translate((NamedArgument)animal.label).CapitalizeFirst(), "RH_TET_TrollsWandersIn".Translate((NamedArgument)animal.label), LetterDefOf.PositiveEvent, parms, (LookTargets)(Thing)pawn, (NamedArgument[])Array.Empty<NamedArgument>());
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f, false, (Predicate<IntVec3>)null);
        }
    }
}
