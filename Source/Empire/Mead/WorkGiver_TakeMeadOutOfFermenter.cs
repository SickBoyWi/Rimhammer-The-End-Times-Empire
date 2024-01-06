using RimWorld;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class WorkGiver_TakeMeadOutOfFermenter : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDef.Named("RH_TET_Empire_FermenterMead"));
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_FermenterMead building_Fermenter) ||
                !building_Fermenter.Fermented)
                return false;
            if (t.IsBurning())
                return false;
            if (t.IsForbidden(pawn)) return false;
            LocalTargetInfo target = t;
            return pawn.CanReserve(target, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("RH_TET_Empire_TakeMeadOutOfFermenter"), t);
        }
    }
}
