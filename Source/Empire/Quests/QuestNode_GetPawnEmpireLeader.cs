using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    public class QuestNode_GetPawnEmpireLeader : QuestNode
    {
        public SlateRef<bool> factionMustBePermanent = (SlateRef<bool>)true;
        public SlateRef<int> maxUsablePawnsToGenerate = (SlateRef<int>)10;
        [NoTranslate]
        public SlateRef<string> storeAs;
        public SlateRef<bool> mustBeFactionLeader;
        public SlateRef<bool> mustBeWorldPawn;
        public SlateRef<bool> ifWorldPawnThenMustBeFree;
        public SlateRef<bool> ifWorldPawnThenMustBeFreeOrLeader;
        public SlateRef<bool> mustHaveNoFaction;
        public SlateRef<bool> mustBeFreeColonist;
        public SlateRef<bool> mustBePlayerPrisoner;
        public SlateRef<bool> mustBeNotSuspended;
        public SlateRef<bool> mustHaveRoyalTitleInCurrentFaction;
        public SlateRef<bool> mustBeNonHostileToPlayer;
        public SlateRef<bool?> allowPermanentEnemyFaction;
        public SlateRef<bool> canGeneratePawn;
        public SlateRef<bool> requireResearchedBedroomFurnitureIfRoyal;
        public SlateRef<PawnKindDef> mustBeOfKind;
        public SlateRef<FloatRange> seniorityRange;
        public SlateRef<TechLevel> minTechLevel;
        public SlateRef<List<FactionDef>> excludeFactionDefs;
        public SlateRef<float?> hostileWeight;
        public SlateRef<float?> nonHostileWeight;

        private IEnumerable<Pawn> ExistingUsablePawns(Slate slate)
        {
            return PawnsFinder.AllMapsWorldAndTemporary_Alive.Where<Pawn>((Func<Pawn, bool>)(x => this.IsGoodPawn(x, slate)));
        }

        protected override bool TestRunInt(Slate slate)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_EmpireOfMan);

            FloatRange senRange = this.seniorityRange.GetValue(slate);

            if (faction != null)
            { 
                slate.Set<Pawn>(this.storeAs.GetValue(slate), faction.leader, false);
                return true;
            }

            return false;
        }

        private bool TryFindFactionForPawnGeneration(Slate slate, out Faction faction)
        {
            faction = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_EmpireOfMan);
            if (faction != null)
            {
                slate.Set<Pawn>(this.storeAs.GetValue(slate), faction.leader, false);
                return true;
            }
            return false;
        }

        protected override void RunInt()
        {
            Slate slate = RimWorld.QuestGen.QuestGen.slate;
            Pawn var1;
            if (RimWorld.QuestGen.QuestGen.slate.TryGet<Pawn>(this.storeAs.GetValue(slate), out var1, false) && this.IsGoodPawn(var1, slate))
                return;

            Faction faction = Find.FactionManager.FirstFactionOfDef(RH_TET_EmpireDefOf.RH_TET_EmpireOfMan);
            if (faction != null)
                RimWorld.QuestGen.QuestGen.slate.Set<Pawn>(this.storeAs.GetValue(slate), faction.leader, false);
        }

        private bool IsGoodPawn(Pawn pawn, Slate slate)
        {
            if (this.mustBeFactionLeader.GetValue(slate))
            {
                Faction faction = pawn.Faction;
                if (faction == null || faction.leader != pawn || (!faction.def.humanlikeFaction || faction.defeated) || (faction.Hidden || faction.IsPlayer || pawn.IsPrisoner))
                    return false;
            }
            if (pawn.Faction != null && this.excludeFactionDefs.GetValue(slate) != null && this.excludeFactionDefs.GetValue(slate).Contains(pawn.Faction.def) || (pawn.Faction != null && pawn.Faction.def.techLevel < this.minTechLevel.GetValue(slate) || this.mustBeOfKind.GetValue(slate) != null && pawn.kindDef != this.mustBeOfKind.GetValue(slate)) || this.mustHaveRoyalTitleInCurrentFaction.GetValue(slate) && (pawn.Faction == null || pawn.royalty == null || !pawn.royalty.HasAnyTitleIn(pawn.Faction)) || this.seniorityRange.GetValue(slate) != new FloatRange() && (pawn.royalty?.MostSeniorTitle == null || !this.seniorityRange.GetValue(slate).IncludesEpsilon((float)pawn.royalty.MostSeniorTitle.def.seniority)) || (this.factionMustBePermanent.GetValue(slate) && pawn.Faction != null && pawn.Faction.temporary || this.mustBeWorldPawn.GetValue(slate) && !pawn.IsWorldPawn() || (this.ifWorldPawnThenMustBeFree.GetValue(slate) && pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free || this.ifWorldPawnThenMustBeFreeOrLeader.GetValue(slate) && pawn.IsWorldPawn() && (Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.Free && Find.WorldPawns.GetSituation(pawn) != WorldPawnSituation.FactionLeader)) || (pawn.IsWorldPawn() && Find.WorldPawns.GetSituation(pawn) == WorldPawnSituation.ReservedByQuest || this.mustHaveNoFaction.GetValue(slate) && pawn.Faction != null || (this.mustBeFreeColonist.GetValue(slate) && !pawn.IsFreeColonist || this.mustBePlayerPrisoner.GetValue(slate) && !pawn.IsPrisonerOfColony) || this.mustBeNotSuspended.GetValue(slate) && pawn.Suspended) || this.mustBeNonHostileToPlayer.GetValue(slate) && (pawn.HostileTo(Faction.OfPlayer) || pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.Faction.HostileTo(Faction.OfPlayer))))
                return false;
            bool? nullable = this.allowPermanentEnemyFaction.GetValue(slate);
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() ? 1 : 0) : 1) == 0 && pawn.Faction != null && pawn.Faction.def.permanentEnemy)
                return false;
            return true;
        }
    }
}
