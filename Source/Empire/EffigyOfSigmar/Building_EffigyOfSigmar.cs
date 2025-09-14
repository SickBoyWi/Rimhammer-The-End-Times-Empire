using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using RimWorld;
using TheEndTimes_Magic;

namespace TheEndTimes_Empire
{
    public partial class Building_EffigyOfSigmar : Building //, IBillGiver, IThingHolder
    {
        // Main building variables.
        public IEnumerable<IntVec3> CellsAround => GenRadial.RadialCellsAround(Position, 5, true);
        public enum State { notinuse = 0, initiating, powering, powered };
        private State currentState = State.notinuse;
        private bool isDestroyed = false;
        private HashSet<Pawn> availableAttendees;
        private Pawn instigator = null;
        Effecter empEffecter = null;
        public bool CanceledInd = false;

        // Events / end game variables.
        private int nextRaidWeakTick = -1;
        private int nextRaidStrongTick = -1;
        private int chargeTicksCompleted = 0;
        private int nextLightningTick = -1;
        private int WEAK_RAID_TICKS_MIN = (int)(60000f * 2.5f); // 1 day is 60000 ticks.
        private int STRONG_RAID_TICKS_MIN = (int)(65000f * 4.5f);
        private int TICKS_TILL_FULLY_CHARGED = 60000 * 15;

        private bool IsPowering() => currentState == State.initiating || currentState == State.powering;
        private bool IsPowered() => currentState == State.initiating || currentState == State.powered;

        public Building_EffigyOfSigmar()
        {

        }

        public Pawn Instigator
        {
            get
            {
                return instigator;
            }
        }

        private bool IsCongregating() => IsInitiating();
        private bool IsInitiating() => currentState == State.initiating;

        private static bool RejectMessage(string s, Pawn pawn = null)
        {
            Messages.Message(s, TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
            if (pawn != null) pawn = null;
            return false;
        }

        public void ChangeState(State type)
        {
            currentState = type;
            ReportState();
        }

        private void ReportState()
        {
            var s = new StringBuilder();
            s.Append("===================");
            s.AppendLine("Effigy of Sigmar States Changed");
            s.AppendLine("===================");
            s.AppendLine("State: " + currentState);
            s.AppendLine("===================");
            EmpireUtil.DebugReport(s.ToString());
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
        }

        public bool IsInstigator (Pawn p)
        {
            if (instigator != null && p != null && p.Equals(instigator))
                return true;

            return false;
        }


        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<State>(ref currentState, "currentState");
            Scribe_References.Look<Pawn>(ref instigator, "instigator");
            Scribe_Values.Look<int>(ref this.chargeTicksCompleted, "chargeTicksCompleted");
            Scribe_Values.Look<int>(ref this.nextLightningTick, "nextLightningTick");
            Scribe_Values.Look<int>(ref this.nextRaidWeakTick, "nextRaidWeakTick");
            Scribe_Values.Look<int>(ref this.nextRaidStrongTick, "nextRaidStrongTick");
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // block further ticker work
            isDestroyed = true;
            base.Destroy(mode);
        }

        protected override void Tick()
        {
            if (isDestroyed)
                return;

            if (!Spawned) return;

            // Don't forget the base work
            base.Tick();

            if (Faction.OfPlayerSilentFail == null || this.Faction != Faction.OfPlayer)//!(Faction.OfPlayerSilentFail.def.defName.Equals("RH_TET_Empire_PlayerWizard") || Faction.OfPlayerSilentFail.def.defName.Equals("RH_TET_Empire_PlayerColony")))
                return;

            SigmarEffigyTick();
        }

        private void SigmarEffigyTick()
        {
            if (currentState == State.initiating)
            {
                InitiatingTick();
            }
            else if (IsPowering() || IsPowered())
            { 
                PoweringTick();
            }

            if (chargeTicksCompleted >= TICKS_TILL_FULLY_CHARGED)
            {
                currentState = State.powered;
            }
        }

        private void WinTheGame()
        {
            SmiteAllFoes();
            DoVictory();
        }

        private void InitiatingTick()
        {
            if (nextLightningTick < Find.TickManager.TicksGame && InteractionCell.GetThingList(this.Map).Contains(instigator))
            {
                nextLightningTick = Find.TickManager.TicksGame;
                this.Map.weatherManager.eventHandler.AddEvent((WeatherEvent)new WeatherEvent_LightningStrikeNoDamage(this.Map, this.Position));

                if (RH_TET_EmpireMod.random.Next(0, 2) == 0)
                this.Map.weatherManager.eventHandler.AddEvent((WeatherEvent)new WeatherEvent_LightningStrikeNoDamage(this.Map, this.Position));

                if (RH_TET_EmpireMod.random.Next(0, 3) == 0)
                    this.Map.weatherManager.eventHandler.AddEvent((WeatherEvent)new WeatherEvent_LightningStrikeNoDamage(this.Map, this.Position));

                if (RH_TET_EmpireMod.random.Next(0, 4) == 0)
                    this.Map.weatherManager.eventHandler.AddEvent((WeatherEvent)new WeatherEvent_LightningStrikeNoDamage(this.Map, this.Position));

                nextLightningTick = (Find.TickManager.TicksGame + RH_TET_EmpireMod.random.Next(100, 250));
            }
        }

        private void PoweringTick()
        {
            if (nextRaidWeakTick <= 0)
            {
                float threatScale = Find.Storyteller.difficulty.threatScale;
                nextRaidWeakTick = Find.TickManager.TicksGame + (int)(((float)WEAK_RAID_TICKS_MIN / threatScale)); 
                nextRaidStrongTick = Find.TickManager.TicksGame + (int)(((float)STRONG_RAID_TICKS_MIN / threatScale));
            }

            if (chargeTicksCompleted < TICKS_TILL_FULLY_CHARGED)
                chargeTicksCompleted++;

            if (this.empEffecter == null)
            { 
                this.empEffecter = EffecterDefOf.DisabledByEMP.Spawn(this, this.Map);
            }
            this.empEffecter.EffectTick((TargetInfo)this, (TargetInfo)this);

            if (nextRaidWeakTick < Find.TickManager.TicksGame)
            {
                float threatScale = Find.Storyteller.difficulty.threatScale;
                nextRaidWeakTick = Find.TickManager.TicksGame + (int)(((float)WEAK_RAID_TICKS_MIN / threatScale));

                EmpireUtil.SpawnRaidEvent(this, new FloatRange(.7f, .8f));
            }
            else if (nextRaidStrongTick < Find.TickManager.TicksGame)
            {
                float threatScale = Find.Storyteller.difficulty.threatScale;
                nextRaidStrongTick = Find.TickManager.TicksGame + (int)(((float)STRONG_RAID_TICKS_MIN / threatScale));

                EmpireUtil.SpawnRaidEvent(this, new FloatRange(.7f, 1.2f));
            }
        }

        private void DoVictory()
        {
            if (currentState != State.powered)
                return;

            RH_TET_Empire_GameVictoryUtility.PlayerVictory(this.Map);

            currentState = State.notinuse;
            chargeTicksCompleted = 0;
            nextRaidStrongTick = -1;
            nextRaidWeakTick = -1;
        }

        private void SmiteAllFoes()
        {
            if (currentState != State.powered)
                return; 

            FleckMaker.Static(this.Position, this.Map, FleckDefOf.PsycastAreaEffect, 6f);
            FleckMaker.Static(this.Position, this.Map, FleckDefOf.LightningGlow, 6f);

            // Effigy map.
            IEnumerable<Pawn> mapEnemyPawns = this.Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(p => (p.Faction != null && this.Faction.HostileTo(p.Faction))));//p.RaceProps.Humanlike && 
            List<Pawn> pawns = new List<Pawn>(mapEnemyPawns);

            foreach (Pawn p in pawns)
            {
                BodyPartRecord bodyPartRecord = ExecuteCutPart(p);
                int num2 = Mathf.Clamp((int)p.health.hediffSet.GetPartHealth(bodyPartRecord) - 1, 1, 20);
                DamageInfo dinfo = new DamageInfo(DamageDefOf.ExecutionCut, (float)num2, 999f, -1f, instigator, bodyPartRecord, (ThingDef)null, DamageInfo.SourceCategory.ThingOrUnknown, (Thing)null);

                p.Kill(dinfo);
            }

            FleckMaker.Static(this.Position, this.Map, FleckDefOf.PsycastAreaEffect, 10f);
            FleckMaker.Static(this.Position, this.Map, FleckDefOf.LightningGlow, 10f);

            // World map.
            int maxDist = 20;
            bool canTraverseImpassable = true;
            bool ignoreFirstTilePassability = true;

            WorldFloodFiller filler = this.Map.Tile.Layer.Filler;

            filler.FloodFill(
                this.Map.Tile,
                (Predicate<PlanetTile>)(x => canTraverseImpassable || !Find.World.Impassable(x) || x == this.Map.Tile & ignoreFirstTilePassability),
                (Predicate<PlanetTile, int>)((tile, traversalDistance) =>
                {
                    if (traversalDistance > maxDist)
                        return true;

                    Settlement settlement = Find.WorldObjects.SettlementBaseAt(tile);
                    if (settlement != null)
                    {
                        if (settlement.Faction != null && settlement.Faction.HostileTo(this.Faction))
                        {
                            settlement.Destroy();
                        }
                    }
                    return false;
                }),
                int.MaxValue,
                (IEnumerable<PlanetTile>)null);

            if (this.empEffecter != null)
            { 
                this.empEffecter.Cleanup();
                this.empEffecter = null;
            }
        }

        private static BodyPartRecord ExecuteCutPart(Pawn pawn)
        {
            BodyPartRecord bodyPartRecord1 = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null).FirstOrDefault<BodyPartRecord>((Func<BodyPartRecord, bool>)(x => x.def == DefDatabase<BodyPartDef>.GetNamed("Neck")));
            if (bodyPartRecord1 != null)
                return bodyPartRecord1;
            BodyPartRecord bodyPartRecord2 = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null).FirstOrDefault<BodyPartRecord>((Func<BodyPartRecord, bool>)(x => x.def == DefDatabase<BodyPartDef>.GetNamed("Head")));
            if (bodyPartRecord2 != null)
                return bodyPartRecord2;
            BodyPartRecord bodyPartRecord3 = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null).FirstOrDefault<BodyPartRecord>((Func<BodyPartRecord, bool>)(x => x.def == DefDatabase<BodyPartDef>.GetNamed("InsectHead")));
            if (bodyPartRecord3 != null)
                return bodyPartRecord3;
            BodyPartRecord bodyPartRecord4 = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null).FirstOrDefault<BodyPartRecord>((Func<BodyPartRecord, bool>)(x => x.def == DefDatabase<BodyPartDef>.GetNamed("Body")));
            if (bodyPartRecord4 != null)
                return bodyPartRecord4;
            BodyPartRecord bodyPartRecord5 = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null).FirstOrDefault<BodyPartRecord>((Func<BodyPartRecord, bool>)(x => x.def == DefDatabase<BodyPartDef>.GetNamed("SnakeBody")));
            if (bodyPartRecord5 != null)
                return bodyPartRecord5;
            BodyPartRecord bodyPartRecord6 = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null).FirstOrDefault<BodyPartRecord>((Func<BodyPartRecord, bool>)(x => x.def == DefDatabase<BodyPartDef>.GetNamed("Torso")));
            if (bodyPartRecord6 != null)
                return bodyPartRecord6;
            Log.Error("No good killing cut part found for " + (object)pawn);
            return pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, (BodyPartTagDef)null, (BodyPartRecord)null).RandomElementByWeight<BodyPartRecord>((Func<BodyPartRecord, float>)(x => x.coverageAbsWithChildren));
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            stringBuilder.Append(base.GetInspectString());

            stringBuilder.AppendLine();

            if (currentState == State.notinuse)
                stringBuilder.Append("RH_TET_Empire_NotInUse".Translate());
            else if (currentState == State.initiating)
                stringBuilder.Append("RH_TET_Empire_Initiating".Translate());
            else if (currentState == State.powering)
                stringBuilder.Append(string.Format("{0}: {1}", (object)"RH_TET_Empire_PoweringUp".Translate(), (object)(this.TICKS_TILL_FULLY_CHARGED - this.chargeTicksCompleted).ToStringTicksToPeriod(true, false, true, true)));
            else if (currentState == State.powered)
                stringBuilder.Append("RH_TET_Empire_SmitingFoes".Translate());

            // return the complete string
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;

            if (!IsPowering())
            {
                if (CanPowerNow())
                { 
                    var command_Action = new Command_Action
                    {
                        action = TryToBeginPowering,
                        defaultLabel = "RH_TET_Empire_CommandPower".Translate(),
                        defaultDesc = "RH_TET_Empire_CommandPowerDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc3,
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/RH_TET_Empire_Power")
                    };
                    command_Action.Disabled = false;
                    yield return command_Action;
                }
                else
                {
                    string descr = "RH_TET_Empire_CommandPowerDescUnavailable".Translate();
                    if (IsPowered())
                        descr = "RH_TET_Empire_CommandPowerDescUnavailablePowered".Translate();

                    var command_Action = new Command_Action
                    {
                        action = null,
                        defaultLabel = "RH_TET_Empire_CommandPower".Translate(),
                        defaultDesc = descr,
                        hotKey = KeyBindingDefOf.Misc3,
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/RH_TET_Empire_PowerDisabled")
                    };
                    command_Action.Disabled = false;
                    yield return command_Action;
                }
            }
            else
            {
                var command_Cancel = new Command_Action
                {
                    action = CancelPowering,
                    defaultLabel = "RH_TET_Empire_Cancel".Translate(),
                    defaultDesc = "RH_TET_Empire_CommandCancelPowering".Translate(),
                    hotKey = KeyBindingDefOf.Designator_Cancel,
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel")
                };
                yield return command_Cancel;
            }

            if(IsPowered())
            {
                var command_Action = new Command_Action
                {
                    action = WinTheGame,
                    defaultLabel = "RH_TET_Empire_CommandSmite".Translate(),
                    defaultDesc = "RH_TET_Empire_CommandSmiteDesc".Translate(),
                    hotKey = KeyBindingDefOf.Misc3,
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/RH_TET_Empire_Smite")
                };
                command_Action.Disabled = false;
                yield return command_Action;
            }
            else
            {
                var command_Action = new Command_Action
                {
                    action = null,
                    defaultLabel = "RH_TET_Empire_CommandSmite".Translate(),
                    defaultDesc = "RH_TET_Empire_CommandSmiteDescUnavailable".Translate(),
                    hotKey = KeyBindingDefOf.Misc3,
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/RH_TET_Empire_SmiteDisabled")
                };
                command_Action.Disabled = false;
                yield return command_Action;
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Start Charging Now",
                    action = delegate
                    {
                        this.currentState = State.powering;
                    }
                };
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Nearly Finish Charging",
                    action = delegate
                    {
                        this.chargeTicksCompleted = 60000 * 15 - 1000;
                    }
                };
            }

            yield break;
        }

        private bool CanPowerNow()
        {
            if (IsPowered())
                return false;

            List<Pawn> pawns = this.Map.mapPawns.FreeColonistsSpawned.FindAll(x => x.equipment.Primary != null && x.equipment.Primary.def != null
                    && (x.equipment.Primary.def.defName.Equals("RH_TET_MeleeWeapon_SigmarHammer") 
                        || x.equipment.Primary.def.defName.Equals("RH_TET_MeleeWeapon_UlricHammer")));

            if (pawns.Count > 0 && (this.Faction != null && pawns.First() != null && pawns.First().Faction != null && this.Faction == pawns.First().Faction))
                return true;

            return false;
        }

        private void CancelPowering()
        {
            Pawn pawn = null;
            // JEH 1.4
            //var listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            // JEH 1.5
            var listeners = Map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(x => x.RaceProps.intelligence == Intelligence.Humanlike)).ToList<Pawn>();
            CanceledInd = true;
            if (!listeners.NullOrEmpty())
            {
                foreach (var t in listeners)
                {
                    pawn = t;
                    if (pawn.Faction != Faction.OfPlayer) continue;
                    if (pawn.CurJob.def == RH_TET_EmpireDefOf.RH_TET_Empire_EffigyBeginHoldBeginPoweringCere ||
                        pawn.CurJob.def == RH_TET_EmpireDefOf.RH_TET_Empire_EffigyGathering)
                    {
                        pawn.jobs.StopAll();
                    }

                    try
                    { 
                        pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(RH_TET_EmpireDefOf.RH_TET_Empire_PraisedSigmar);
                    }
                    catch
                    {
                        // Ignore.
                    }
                }
            }

            currentState = State.notinuse;
            chargeTicksCompleted = 0;
            if (empEffecter != null)
            { 
                empEffecter.Cleanup();
                empEffecter = null;
            }

            Messages.Message("RH_TET_Empire_PoweringCancelled".Translate(), MessageTypeDefOf.NegativeEvent);
        }

        private void TryToBeginPowering()
        {
            if (IsPowering())
            {
                Messages.Message("RH_TET_Empire_PoweringCeremonyAlready".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            if (!CanGatherForPoweringNow()) return;

            switch (currentState)
            {
                case State.notinuse:
                    StartPowering();
                    return;

                case State.initiating:
                case State.powering:
                case State.powered:
                    Messages.Message("RH_TET_Empire_EffigyPoweringAlready".Translate(), TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
                    return;
            }
        }

        private bool CanGatherForPoweringNow()
        {
            List<Pawn> pawns = this.Map.mapPawns.FreeColonistsSpawned.FindAll(x => x.equipment != null
                && x.equipment.Primary != null && x.equipment.Primary.def != null
                && x.equipment.Primary.def.defName.Equals("RH_TET_MeleeWeapon_SigmarHammer"));

            Pawn initiator = null;
            foreach (Pawn p in pawns)
            {
                if (p != null && !p.Drafted && !p.Dead && !p.Downed)
                {
                    initiator = p;
                    break;
                }
            }

            if (initiator == null)
            { 
                List < Pawn > pawns2 = this.Map.mapPawns.FreeColonistsSpawned.FindAll(x => x.equipment.Primary != null && x.equipment.Primary.def != null
                       && x.equipment.Primary.def.defName.Equals("RH_TET_MeleeWeapon_UlricHammer"));

                foreach (Pawn p in pawns2)
                {
                    if (p != null && !p.Drafted && !p.Dead && !p.Downed)
                    {
                        initiator = p;
                        break;
                    }
                }
            }

            if (initiator == null)
                return RejectMessage("RH_TET_Empire_EffigyNoInstigator".Translate());

            if (!initiator.CanReserve(this)) return RejectMessage("RH_TET_Empire_EffigyNoEffigy".Translate());

            this.instigator = initiator;
            return true;
        }

        private void StartPowering()
        {
            //Check for missing things.
            if (!PreStartPoweringReady()) return;

            //Send a message about the gathering
            if (Map?.info?.parent is Settlement factionBase)
            {
                Messages.Message("RH_TET_Empire_EffigyGathering".Translate(), TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);
            }

            CanceledInd = false;

            //Give the effigy gathering job
            var job = new Job(RH_TET_EmpireDefOf.RH_TET_Empire_EffigyBeginHoldBeginPoweringCere, this) { count = 1 };
            instigator.jobs.TryTakeOrderedJob(job);

            //Set the congregation
            GetCongregationGroup();

            EmpireUtil.DebugReport("Effigy gathering started.");
        }

        private bool PreStartPoweringReady()
        {
            if (Destroyed || !Spawned)
            {
                AbortPowering("The Effigy of Sigmar is unavailable.");
                return false;
            }
            if (!EmpireUtil.IsActorAvailable(instigator))
            {
                AbortPowering("The properly equipped pawn, " + instigator.LabelShort + " is unavaialable.");
                instigator = null;
                return false;
            }

            return true;
        }

        private void AbortPowering(String reason)
        {
            currentState = State.notinuse;
            Messages.Message(reason + " " + "RH_TET_Empire_EffigyAbortPowering".Translate(), MessageTypeDefOf.NegativeEvent);
        }

        public static List<Pawn> GetCongregationGroup(Building_EffigyOfSigmar effigy) => effigy.GetCongregationGroup();

        public List<Pawn> GetCongregationGroup()
        {
            var room = this.GetRoom();

            var pawns = new List<Pawn>();
            if (room.Role == RoomRoleDefOf.PrisonBarracks || room.Role == RoomRoleDefOf.PrisonCell) return pawns;
            if (AvailableAttendees == null || AvailableAttendees.Count <= 0) return pawns;
            foreach (var p in AvailableAttendees)
            {
                EmpireUtil.GiveAttendEffigyGatheringJob(this, p);
                pawns.Add(p);
            }
            return pawns;
        }
        public HashSet<Pawn> AvailableAttendees
        {
            get
            {
                if (availableAttendees == null || availableAttendees.Count == 0)
                {
                    availableAttendees =
                        new HashSet<Pawn>(Map.mapPawns.FreeColonistsSpawned.FindAll(y => y is Pawn x &&
                           x.RaceProps.Humanlike &&
                           !x.IsPrisoner &&
                           x.Faction == Faction &&
                           x.RaceProps.intelligence == Intelligence.Humanlike &&
                           !x.Downed && !x.Dead &&
                           !x.InMentalState && !x.InAggroMentalState &&
                           x.CurJob.def != RH_TET_EmpireDefOf.RH_TET_Empire_EffigyGathering &&
                           x.CurJob.def != JobDefOf.Capture &&
                           x.CurJob.def != JobDefOf.ExtinguishSelf &&
                           x.CurJob.def != JobDefOf.Rescue &&
                           x.CurJob.def != JobDefOf.TendPatient &&
                           x.CurJob.def != JobDefOf.BeatFire &&
                           x.CurJob.def != JobDefOf.Lovin &&
                           x.CurJob.def != JobDefOf.LayDown &&
                           x.CurJob.def != JobDefOf.FleeAndCower
                    ).ChangeType<List<Pawn>>());
                }
                return availableAttendees;
            }
        }
    }
}