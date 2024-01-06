using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TheEndTimes_Empire
{
    public class WorkGiver_CollectHoney : WorkGiver_Scanner
    {
        protected JobDef JobDef
        {
            get
            {
                return DefDatabase<JobDef>.GetNamed("RH_TET_Empire_CollectHoney", true);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        protected CompHasGatherableBodyResource GetComp(Thing thing)
        {
            return (CompHasGatherableBodyResource)thing.TryGetComp<CompBeeHive>();
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Building> list = pawn.Map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].TryGetComp<CompBeeHive>() != null)
                    yield return (Thing)list[i];
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return t != null && this.GetComp(t) != null && this.GetComp(t).ActiveAndFull && pawn.CanReserve((LocalTargetInfo)t, 1, -1, (ReservationLayerDef)null, false);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(this.JobDef, (LocalTargetInfo)t);
        }
    }
}
