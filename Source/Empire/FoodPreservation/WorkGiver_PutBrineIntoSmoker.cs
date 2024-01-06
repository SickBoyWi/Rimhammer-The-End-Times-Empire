using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class WorkGiver_PutBrineIntoSmoker : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("RH_TET_Empire_MeatSmoker"));
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        private const String BRINED_MEAT = "RH_TET_Empire_BrinedMeat";

        private static string TemperatureTrans;
        private static string NoBrineTrans;

        public static void Reset()
        {
            TemperatureTrans = "BadTemperature".Translate().ToLower();
            NoBrineTrans = "RH_TET_Empire_NoBrinedMeat".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_MeatSmoker BuildingCurrent) ||
                BuildingCurrent.Ready || BuildingCurrent.SpaceLeftForStuff <= 0)
                return false;

            var ambientTemperature = BuildingCurrent.AmbientTemperature;
            var compProperties =
                BuildingCurrent.def.GetCompProperties<CompProperties_TemperatureRuinable>();
            if (ambientTemperature < compProperties.minSafeTemperature + 2f ||
                ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is(TemperatureTrans);
                return false;
            }
            if (t.IsForbidden(pawn)) return false;
            LocalTargetInfo target = t;
            if (!pawn.CanReserve(target, 1, -1, null, forced)) return false;
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                return false;

            if (FindStuff(pawn, BuildingCurrent) != null) return !t.IsBurning();
            JobFailReason.Is(NoBrineTrans);
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var thingCurrent = (Building_MeatSmoker)t;
            var t2 = FindStuff(pawn, thingCurrent);
            return new Job(DefDatabase<JobDef>.GetNamed("RH_TET_Empire_PutBrineIntoSmoker"), t, t2);
        }

        private Thing FindStuff(Pawn pawn, Building_MeatSmoker buildingCurrent)
        {
            bool Predicate(Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            var position = pawn.Position;
            var map = pawn.Map;
            var thingReq = ThingRequest.ForDef(ThingDef.Named(BRINED_MEAT));
            const PathEndMode peMode = PathEndMode.ClosestTouch;
            var traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = Predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator,
                null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
