using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Empire
{
    public class SigmarHammerSite : MapParent
    {
        public static SigmarHammerSite ActiveSite;
        public CaravanArrivalAction_SigmarHammerSite caravanAction;
        public SigmarHammerSiteComp siteComp;
        public static bool playerWon = false;
        public bool destroyAndCleanUp = false;
        public bool aiAssault = false;
        public bool aiFlee = false;
        public int startingEnemyCount = -1;
        public float enemyAttackThreshold = .9F;
        public float enemyFleeThreshold = .49F;
        private bool destroyed;

        public override void SpawnSetup()
        {
            if (this.Faction == null)
            {
                this.SetFaction(this.GetEnemyFaction());
            }

            base.SpawnSetup();
            this.caravanAction = new CaravanArrivalAction_SigmarHammerSite((MapParent)this);
            this.siteComp = this.GetComponent<SigmarHammerSiteComp>();

            Faction f = this.Faction;

            if (!f.HostileTo(Faction.OfPlayer))
            {
                int currentGoodwill = f.GoodwillWith(Faction.OfPlayer);

                if (currentGoodwill > 0)
                {
                    currentGoodwill *= -1;
                    currentGoodwill += -100;
                }
                else if (currentGoodwill < 0)
                {
                    currentGoodwill *= -1;
                    currentGoodwill = -100 + currentGoodwill;
                }
                else
                    currentGoodwill = -100;

                f.TryAffectGoodwillWith(Faction.OfPlayer, currentGoodwill, true, true, null);
            }
        }

        private Faction GetEnemyFaction()
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_Beastmen_GorFaction);
            return f;
        }

        public override void Tick()
        {
            base.Tick();

            this.CheckCleanUpSiteMapNow();

            if (!this.HasMap || playerWon)
                return;

            this.CheckPlayerWon();
            this.CheckColonistsNow();

        }

        public void CheckCleanUpSiteMapNow()
        {
            // Clean everything up if the player won, and the hammer is gone, and there's no player pawns capable of moving on the map.
            // Also, account for player hasn't won, and was defeated. Site should stay in that case.
            if (this.destroyed || !destroyAndCleanUp)
                return;

            if (destroyAndCleanUp && this.Map != null)
            {
                IEnumerable<Pawn> pawnList = this.Map.mapPawns.FreeHumanlikesSpawnedOfFaction(Faction.OfPlayer);
                if (pawnList.Count() == 0)
                {
                    List<Thing> things = this.Map.listerThings.ThingsOfDef(RH_TET_EmpireDefOf.RH_TET_MeleeWeapon_SigmarHammer);

                    if (things != null && things.Count() > 0)
                    {
                        return;
                    }
                }
                else
                    return;
            }

            if (this.Map == null)
            {
                return;
            }
            // JEH 1.5
            Current.Game.DeinitAndRemoveMap(this.Map, false);
            this.Destroy();
        }

        public override void Destroy()
        {
            if (this.destroyed)
            {
                Log.Error("Tried to destroy already-destroyed world object " + (object)this);
            }
            else
            {
                if (this.Spawned)
                    Find.WorldObjects.Remove(this);
                this.destroyed = true;
            }
        }

        private void CheckPlayerWon()
        {
            if (playerWon || this.Faction == Faction.OfPlayer)
                return;

            Map map = this.Map;

            if (map == null || !this.IsDefeated(map, this.Faction))
                return;

            playerWon = true;

            StringBuilder text = new StringBuilder();
            text.Append("RH_TET_Empire_ReclaimedSigmarHammerDesc".Translate(this.Label));
            foreach (Faction allFaction in Find.FactionManager.AllFactions)
            {
                if (!allFaction.def.hidden && !allFaction.IsPlayer && (allFaction != this.Faction && allFaction.HostileTo(this.Faction)))
                {
                    FactionRelationKind playerRelationKind = allFaction.PlayerRelationKind;
                    if (allFaction.TryAffectGoodwillWith(Faction.OfPlayer, 20, false, false, null, new GlobalTargetInfo?()))
                    {
                        text.AppendLine();
                        text.AppendLine();
                        text.Append("RelationsWith".Translate((NamedArgument)allFaction.Name) + ": " + 20.ToStringWithSign());
                        allFaction.TryAppendRelationKindChangedInfo(text, playerRelationKind, allFaction.PlayerRelationKind, (string)null);
                    }
                }
            }

            Find.LetterStack.ReceiveLetter("RH_TET_Empire_ReclaimedSigmarHammerLabel".Translate(),
                    text.ToString(),
                    LetterDefOf.PositiveEvent,
                    (LookTargets)new GlobalTargetInfo(this.Tile),
                    this.Faction,
                    null,
                    null,
                    null);

            TaleRecorder.RecordTale(RH_TET_EmpireDefOf.RH_TET_Empire_ClaimedHammer, (object)map.mapPawns.FreeColonists.RandomElement<Pawn>());

            this.destroyAndCleanUp = true;
        }

        private bool IsDefeated(Map map, Faction faction)
        {
            IEnumerable<Pawn> pawnList = map.mapPawns.FreeHumanlikesSpawnedOfFaction(faction);

            if (this.startingEnemyCount == -1)
                startingEnemyCount = pawnList.Count();

            int threatCount = 0;
            foreach (Pawn pawn in pawnList)
            {
                if (pawn.Map.fogGrid.IsFogged(pawn.Position))
                    threatCount++;
                else if (pawn.RaceProps.Humanlike && GenHostility.IsActiveThreatToPlayer((IAttackTarget)pawn))
                    threatCount++;
            }

            if (!aiFlee && (threatCount <= (int)(this.startingEnemyCount * enemyFleeThreshold)))
            {
                aiFlee = true;
                // Victory! Make remaining enemies flee.
                List<Lord> lords = new List<Lord>();
                List<LordToil> lordToils = new List<LordToil>();
                foreach (Lord lord in this.Map.lordManager.lords)
                {
                    if (lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer) && lord.faction.def.autoFlee)
                    {
                        LordToil newLordToil = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_PanicFlee));
                        if (newLordToil != null)
                        {
                            lordToils.Add(newLordToil);
                            //lord.GotoToil(newLordToil);
                            lords.Add(lord);
                        }
                    }
                }

                int i = 0;
                foreach (Lord lord in lords)
                {
                    LordToil newLordToil = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_PanicFlee));
                    lord.GotoToil(lordToils.ElementAt(i));
                    i++;
                }

                return true;
            }
            else if (!aiFlee && !aiAssault && (threatCount <= (int)(this.startingEnemyCount * enemyAttackThreshold)))
            {
                aiAssault = true;
                // Now they're angry!
                foreach (Lord lord in this.Map.lordManager.lords)
                {
                    if (lord.faction != null && lord.faction.HostileTo(Faction.OfPlayer))
                    {
                        LordToil_AssaultColony toilAssaultColony = new LordToil_AssaultColony(true);
                        LordToil lordToilDefendBase1 = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_DefendBase));

                        toilAssaultColony.useAvoidGrid = true;
                        toilAssaultColony.lord = lord;
                        lord.Graph.AddToil((LordToil)toilAssaultColony);

                        Transition transition3 = new Transition((LordToil)lordToilDefendBase1, (LordToil)toilAssaultColony, false, true);
                        transition3.AddTrigger((Trigger)new Trigger_FractionPawnsLost(0.2f));
                        transition3.AddTrigger((Trigger)new Trigger_PawnHarmed(0.4f, false, (Faction)null));
                        transition3.AddTrigger((Trigger)new Trigger_ChanceOnTickInterval(2500, 0.03f));
                        transition3.AddTrigger((Trigger)new Trigger_TicksPassed(251999));
                        transition3.AddTrigger((Trigger)new Trigger_UrgentlyHungry());
                        transition3.AddTrigger((Trigger)new Trigger_ChanceOnPlayerHarmNPCBuilding(0.4f));
                        transition3.AddPostAction((TransitionAction)new TransitionAction_WakeAll());
                        TaggedString taggedString = "MessageDefendersAttacking".Translate((NamedArgument)lord.faction.def.pawnsPlural, (NamedArgument)lord.faction.Name, (NamedArgument)Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
                        Messages.Message(taggedString, (MessageTypeDef)MessageTypeDefOf.ThreatBig, true);
                        lord.Graph.AddTransition(transition3, false);

                        LordToil newLordToil = lord.Graph.lordToils.FirstOrDefault<LordToil>((Func<LordToil, bool>)(st => st is LordToil_AssaultColony));
                        if (newLordToil != null)
                            lord.GotoToil(newLordToil);
                    }
                }
            }

            return false;
        }

        public void CheckColonistsNow()
        {
            if (playerWon)
                return;

            List<Pawn> list = this.Map.mapPawns.FreeColonists.ToList<Pawn>();

            int downedPawns = 0;
            list.ForEach((Action<Pawn>)(p =>
            {
                if (!p.Downed && !p.Dead && p.Spawned)
                    return;
                ++downedPawns;
            }));

            if (downedPawns != list.Count)
                return;

            Find.LetterStack.ReceiveLetter((Letter)LetterMaker.MakeLetter("RH_TET_Empire_FailedToClaimHammerLabel".Translate(), "RH_TET_Empire_FailedToClaimHammerDesc".Translate(), LetterDefOf.NegativeEvent), (string)null);
            Current.Game.DeinitAndRemoveMap(this.Map, true);
        }

        public override void PostMapGenerate()
        {
            base.PostMapGenerate();

            List<Pawn> pawnList;

            // TODO JEH 1.3
            MapGenHandler.GenerateMap(RH_TET_EmpireDefOf.RH_TET_Empire_SigmarHammerMap, this.Map, out pawnList, true, true, true, true, false, true, true, false, Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_Beastmen_GorFaction));

            this.ShowHelp();
        }

        private void ShowHelp()
        {
            DiaNode nodeRoot = new DiaNode("RH_TET_Empire_SigmarHammerHelp".Translate());

            nodeRoot.options.Add(new DiaOption("OK".Translate())
            {
                resolveTree = true
            });
            nodeRoot.options.Add(new DiaOption("JumpToLocation".Translate())
            {
                action = (Action)(() => CameraJumper.TryJumpAndSelect((GlobalTargetInfo)this.Map.PlayerPawnsForStoryteller.First<Pawn>())),
                resolveTree = true
            });
            Find.WindowStack.Add((Window)new Dialog_NodeTree(nodeRoot, false, false, (string)null));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref SigmarHammerSite.playerWon, "playerWon", false, false);
            Scribe_References.Look<SigmarHammerSite>(ref SigmarHammerSite.ActiveSite, "ActiveSite", false);
        }

        private bool AnyHostileOnMap(Map map, Faction enemyFaction)
        {
            List<Pawn> list = map.mapPawns.AllPawnsSpawned.Where<Pawn>((Func<Pawn, bool>)(p =>
            {
                if (p.Faction == enemyFaction && !p.Dead && !p.Downed)
                    return p.RaceProps.Humanlike;
                return false;
            })).ToList<Pawn>();
            return list != null && list.Count != 0;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(
          Caravan caravan)
        {
            foreach (FloatMenuOption floatMenuOption in CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_SigmarHammerSite>((Func<FloatMenuAcceptanceReport>)(() => this.caravanAction.CanVisit(caravan, this)), (Func<CaravanArrivalAction_SigmarHammerSite>)(() => this.caravanAction), "EnterMap".Translate((NamedArgument)this.Label), caravan, this.Tile, (WorldObject)this))
                yield return floatMenuOption;
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());

            sb.AppendLine();
            sb.Append("RH_TET_Empire_SigmarHammerSiteInfo".Translate());

            sb.AppendLine();
            sb.Append("RH_TET_Empire_SigmarHammerSiteBeastsInfo".Translate());

            return sb.ToString();
        }
    }
}
