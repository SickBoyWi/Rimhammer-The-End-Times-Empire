using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

using Verse;

namespace TheEndTimes_Empire
{
    public class ThoughtWorker_ListeningToLute : ThoughtWorker_MusicalInstrumentListeningBase
    {
        protected override ThingDef InstrumentDef
        {
            get
            {
                return RH_TET_EmpireDefOf.RH_TET_Empire_Lute;
            }
        }
    }
}