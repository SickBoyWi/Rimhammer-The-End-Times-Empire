using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TheEndTimes_Empire
{
    public class QuestNode_GetFactionBeastmenOrDefault : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;
        public SlateRef<bool> allowEnemy;
        public SlateRef<bool> allowNeutral;
        public SlateRef<bool> allowAlly;
        public SlateRef<bool> allowAskerFaction;
        public SlateRef<bool?> allowPermanentEnemy;
        public SlateRef<bool> mustBePermanentEnemy;
        public SlateRef<bool> playerCantBeAttackingCurrently;
        public SlateRef<bool> peaceTalksCantExist;
        public SlateRef<bool> leaderMustBeSafe;
        public SlateRef<bool> mustHaveGoodwillRewardsEnabled;
        public SlateRef<Pawn> ofPawn;
        public SlateRef<Thing> mustBeHostileToFactionOf;
        public SlateRef<IEnumerable<Faction>> exclude;
        public SlateRef<IEnumerable<Faction>> allowedHiddenFactions;

        protected override bool TestRunInt(Slate slate)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_Beastmen_GorFaction);

            if (faction is null)
            {
                Log.Warning("Beastmen faction doesn't exist for TestRunInt QuestNode_GetFactionBeastmenOrDefault");

                if (slate.TryGet<Faction>(this.storeAs.GetValue(slate), out faction, false) && this.IsGoodFaction(faction, slate))
                    return true;
                if (!this.TryFindFaction(out faction, slate))
                    return false;
                slate.Set<Faction>(this.storeAs.GetValue(slate), faction, false);
                return true;
            }

            slate.Set<Faction>(this.storeAs.GetValue(slate), faction, false);

            return true;
        }

        protected override void RunInt()
        {
            Slate slate = RimWorld.QuestGen.QuestGen.slate;
            Faction faction = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_Beastmen_GorFaction);

            if (faction is null)
            {
                Log.Warning("Beastmen faction doesn't exist for TestRunInt QuestNode_GetFactionBeastmenOrDefault");

                if (RimWorld.QuestGen.QuestGen.slate.TryGet<Faction>(this.storeAs.GetValue(slate), out faction, false) && this.IsGoodFaction(faction, RimWorld.QuestGen.QuestGen.slate) || !this.TryFindFaction(out faction, RimWorld.QuestGen.QuestGen.slate))
                    return;
                RimWorld.QuestGen.QuestGen.slate.Set<Faction>(this.storeAs.GetValue(slate), faction, false);
                if (faction.Hidden)
                    return;
            }

            RimWorld.QuestGen.QuestGen.quest.AddPart((QuestPart)new QuestPart_InvolvedFactions()
            {
                factions = { faction }
            });
        }

        private bool TryFindFaction(out Faction faction, Slate slate)
        {
            faction = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_Beastmen_GorFaction);

            //if (this.IsGoodFaction(faction, slate))
            if (!(faction is null))
            {
                if (faction.HostileTo(Faction.OfPlayer))
                    return true;

                faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Hostile, false);

                return true;
            }

            return Find.FactionManager.GetFactions(true, false, true, TechLevel.Undefined, false).Where<Faction>((Func<Faction, bool>)(x => this.IsGoodFaction(x, slate))).TryRandomElement<Faction>(out faction);
        }

        private bool IsGoodFaction(Faction faction, Slate slate)
        {
            if (faction.Hidden && (this.allowedHiddenFactions.GetValue(slate) == null || !this.allowedHiddenFactions.GetValue(slate).Contains<Faction>(faction)) || (this.ofPawn.GetValue(slate) != null && faction != this.ofPawn.GetValue(slate).Faction || this.exclude.GetValue(slate) != null && this.exclude.GetValue(slate).Contains<Faction>(faction)) || (this.mustBePermanentEnemy.GetValue(slate) && !faction.def.permanentEnemy || !this.allowEnemy.GetValue(slate) && faction.HostileTo(Faction.OfPlayer) || (!this.allowNeutral.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Neutral || !this.allowAlly.GetValue(slate) && faction.PlayerRelationKind == FactionRelationKind.Ally)))
                return false;
            bool? nullable = this.allowPermanentEnemy.GetValue(slate);
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() ? 1 : 0) : 1) == 0 && faction.def.permanentEnemy || this.playerCantBeAttackingCurrently.GetValue(slate) && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction) || this.mustHaveGoodwillRewardsEnabled.GetValue(slate) && !faction.allowGoodwillRewards)
                return false;
            if (this.peaceTalksCantExist.GetValue(slate))
            {
                if (this.PeaceTalksExist(faction))
                    return false;
                string tag = QuestNode_QuestUnique.GetProcessedTag("PeaceTalks", faction);
                if (Find.QuestManager.questsInDisplayOrder.Any<Quest>((Predicate<Quest>)(q => q.tags.Contains(tag))))
                    return false;
            }
            if (this.leaderMustBeSafe.GetValue(slate) && (faction.leader == null || faction.leader.Spawned || faction.leader.IsPrisoner))
                return false;
            Thing thing = this.mustBeHostileToFactionOf.GetValue(slate);
            return thing == null || thing.Faction == null || thing.Faction != faction && faction.HostileTo(thing.Faction);
        }

        private bool PeaceTalksExist(Faction faction)
        {
            List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
            for (int index = 0; index < peaceTalks.Count; ++index)
            {
                if (peaceTalks[index].Faction == faction)
                    return true;
            }
            return false;
        }
    }
}
