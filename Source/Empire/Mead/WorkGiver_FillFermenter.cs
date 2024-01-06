using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class WorkGiver_FillFermenter : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("RH_TET_Empire_FermenterMead"));
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public static void Reset()
        {
            TemperatureTrans = "BadTemperature".Translate().ToLower();
            NoMustTrans = "RH_TET_Empire_NoMustMead".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_FermenterMead Building_Fermenter) ||
                Building_Fermenter.Fermented || Building_Fermenter.SpaceLeftForMust <= 0)
                return false;

            var ambientTemperature = Building_Fermenter.AmbientTemperature;
            var compProperties =
                Building_Fermenter.def.GetCompProperties<CompProperties_TemperatureRuinable>();
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

            if (FindMust(pawn, Building_Fermenter) != null) return !t.IsBurning();
            JobFailReason.Is(NoMustTrans);
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var barrel = (Building_FermenterMead)t;
            var t2 = FindMust(pawn, barrel);
            return new Job(DefDatabase<JobDef>.GetNamed("RH_TET_Empire_FillFermenter"), t, t2);
        }

        private Thing FindMust(Pawn pawn, Building_FermenterMead fermenter)
        {
            bool Predicate(Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            var position = pawn.Position;
            var map = pawn.Map;
            var thingReq = ThingRequest.ForDef(ThingDef.Named("RH_TET_Empire_MeadMust"));
            const PathEndMode peMode = PathEndMode.ClosestTouch;
            var traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = Predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator,
                null, 0, -1, false, RegionType.Set_Passable, false);
        }

        private static string TemperatureTrans;
        private static string NoMustTrans;
    }
}
