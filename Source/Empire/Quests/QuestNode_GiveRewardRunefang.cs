using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace TheEndTimes_Empire
{
    public class QuestNode_GiveRewardRunefang : QuestNode
    {
        private List<List<Reward>> generatedRewards = new List<List<Reward>>();
        [NoTranslate]
        public SlateRef<string> inSignal;
        public SlateRef<RewardsGeneratorParams> parms;
        public SlateRef<string> customLetterLabel;
        public SlateRef<string> customLetterText;
        public SlateRef<RulePack> customLetterLabelRules;
        public SlateRef<RulePack> customLetterTextRules;
        public SlateRef<bool?> useDifficultyFactor;
        public QuestNode nodeIfChosenPawnSignalUsed;
        public SlateRef<int?> variants;
        public SlateRef<bool> addCampLootReward;
        private const float MinRewardValue = 250f;
        private const int DefaultVariants = 3;

        protected override bool TestRunInt(Slate slate)
        {
            return this.nodeIfChosenPawnSignalUsed == null || this.nodeIfChosenPawnSignalUsed.TestRun(slate);
        }

        protected override void RunInt()
        {
            this.GiveRewards(this.parms.GetValue(RimWorld.QuestGen.QuestGen.slate), this.inSignal.GetValue(RimWorld.QuestGen.QuestGen.slate), this.customLetterLabel.GetValue(RimWorld.QuestGen.QuestGen.slate), this.customLetterText.GetValue(RimWorld.QuestGen.QuestGen.slate), this.customLetterLabelRules.GetValue(RimWorld.QuestGen.QuestGen.slate), this.customLetterTextRules.GetValue(RimWorld.QuestGen.QuestGen.slate), this.useDifficultyFactor.GetValue(RimWorld.QuestGen.QuestGen.slate), (Action)(() => this.nodeIfChosenPawnSignalUsed?.Run()), this.variants.GetValue(RimWorld.QuestGen.QuestGen.slate), this.addCampLootReward.GetValue(RimWorld.QuestGen.QuestGen.slate), RimWorld.QuestGen.QuestGen.slate.Get<Pawn>("asker", (Pawn)null, false), false, false, new float?());
        }

        private QuestPart_Choice GiveRewards(
              RewardsGeneratorParams parms,
              string inSignal = null,
              string customLetterLabel = null,
              string customLetterText = null,
              RulePack customLetterLabelRules = null,
              RulePack customLetterTextRules = null,
              bool? useDifficultyFactor = null,
              Action runIfChosenPawnSignalUsed = null,
              int? variants = null,
              bool addCampLootReward = false,
              Pawn asker = null,
              bool addShuttleLootReward = false,
              bool addPossibleFutureReward = false,
              float? overridePopulationIntent = null)
        {
            try
            {
                Slate slate = RimWorld.QuestGen.QuestGen.slate;
                RewardsGeneratorParams parmsResolved = parms;

                parmsResolved.rewardValue = (double)parmsResolved.rewardValue == 0.0 ? slate.Get<float>("rewardValue", 0.0f, false) : parmsResolved.rewardValue;
                parmsResolved.giverFaction = parmsResolved.giverFaction ?? asker?.Faction;

                Slate.VarRestoreInfo restoreInfo = slate.GetRestoreInfo(nameof(inSignal));
                if (inSignal.NullOrEmpty())
                    inSignal = slate.Get<string>(nameof(inSignal), (string)null, false);
                else
                    slate.Set<string>(nameof(inSignal), QuestGenUtility.HardcodedSignalWithQuestID(inSignal), false);

                try
                {
                    QuestPart_Choice questPartChoice = new QuestPart_Choice();
                    questPartChoice.inSignalChoiceUsed = slate.Get<string>(nameof(inSignal), (string)null, false);
                    bool chosenPawnSignalUsed = false;
                    int variants1 = 1;
                    this.generatedRewards.Clear();

                    for (int index = 0; index < variants1; ++index)
                    {
                        QuestPart_Choice.Choice choice = new QuestPart_Choice.Choice();
                        List<Reward> rewards = this.GenerateRewards(parmsResolved, slate, index == 0, ref chosenPawnSignalUsed, choice, variants1, customLetterLabel, customLetterText, customLetterLabelRules, customLetterTextRules);
                        if (rewards != null)
                        {
                            questPartChoice.choices.Add(choice);
                            this.generatedRewards.Add(rewards);
                        }
                    }

                    this.generatedRewards.Clear();

                    RimWorld.QuestGen.QuestGen.quest.AddPart((QuestPart)questPartChoice);
                    
                    // This is for a chosen pawn, which this quest doesn't have.
                    //if (chosenPawnSignalUsed && runIfChosenPawnSignalUsed != null)
                    //{
                    //    QuestGen_Rewards.tmpPrevQuestParts.Clear();
                    //    QuestGen_Rewards.tmpPrevQuestParts.AddRange((IEnumerable<QuestPart>)RimWorld.QuestGen.QuestGen.quest.PartsListForReading);
                    //    runIfChosenPawnSignalUsed();
                    //    List<QuestPart> partsListForReading = RimWorld.QuestGen.QuestGen.quest.PartsListForReading;
                    //    for (int index1 = 0; index1 < partsListForReading.Count; ++index1)
                    //    {
                    //        if (!QuestGen_Rewards.tmpPrevQuestParts.Contains(partsListForReading[index1]))
                    //        {
                    //            for (int index2 = 0; index2 < questPartChoice.choices.Count; ++index2)
                    //            {
                    //                bool flag = false;
                    //                for (int index3 = 0; index3 < questPartChoice.choices[index2].rewards.Count; ++index3)
                    //                {
                    //                    if (questPartChoice.choices[index2].rewards[index3].MakesUseOfChosenPawnSignal)
                    //                    {
                    //                        flag = true;
                    //                        break;
                    //                    }
                    //                }
                    //                if (flag)
                    //                    questPartChoice.choices[index2].questParts.Add(partsListForReading[index1]);
                    //            }
                    //        }
                    //    }
                    //    QuestGen_Rewards.tmpPrevQuestParts.Clear();
                    //}

                    return questPartChoice;
                }
                finally
                {
                    slate.Restore(restoreInfo);
                }
            }
            finally
            {
                this.generatedRewards.Clear();
            }
        }

        private List<Reward> GenerateRewards(
          RewardsGeneratorParams parmsResolved,
          Slate slate,
          bool addDescriptionRules,
          ref bool chosenPawnSignalUsed,
          QuestPart_Choice.Choice choice,
          int variants,
          string customLetterLabel,
          string customLetterText,
          RulePack customLetterLabelRules,
          RulePack customLetterTextRules)
        {
            List<string> source;
            List<string> stringList;

            if (addDescriptionRules)
            {
                source = new List<string>();
                stringList = new List<string>();
            }
            else
            {
                source = (List<string>)null;
                stringList = (List<string>)null;
            }

            bool flag1 = false;
            bool genRewardsHasPawn = false;

            for (int index1 = 0; index1 < this.generatedRewards.Count; ++index1)
            {
                for (int index2 = 0; index2 < this.generatedRewards[index1].Count; ++index2)
                {
                    if (this.generatedRewards[index1][index2] is Reward_Pawn)
                    {
                        genRewardsHasPawn = true;
                        break;
                    }
                }
                if (genRewardsHasPawn)
                    break;
            }

            if (genRewardsHasPawn)
                parmsResolved.thingRewardItemsOnly = true;

            List<Reward> rewardList = this.DoGenerate(parmsResolved, out float _);

            if (rewardList.NullOrEmpty<Reward>())
                return (List<Reward>)null;

            Reward_Items rewardItems1 = (Reward_Items)rewardList.Find((Predicate<Reward>)(x => x is Reward_Items));

            if (rewardItems1 == null)
            {
                List<System.Type> list = rewardList.Select<Reward, System.Type>((Func<Reward, System.Type>)(x => x.GetType())).ToList<System.Type>();
                for (int index = 0; index < this.generatedRewards.Count; ++index)
                {
                    if (this.generatedRewards[index].Select<Reward, System.Type>((Func<Reward, System.Type>)(x => x.GetType())).ToList<System.Type>().SetsEqual<System.Type>(list))
                        return (List<Reward>)null;
                }
            }
            else if (rewardList.Count == 1)
            {
                List<ThingDef> list = rewardItems1.ItemsListForReading.Select<Thing, ThingDef>((Func<Thing, ThingDef>)(x => x.def)).ToList<ThingDef>();
                for (int index = 0; index < this.generatedRewards.Count; ++index)
                {
                    Reward_Items rewardItems2 = (Reward_Items)this.generatedRewards[index].Find((Predicate<Reward>)(x => x is Reward_Items));
                    if (rewardItems2 != null && rewardItems2.ItemsListForReading.Select<Thing, ThingDef>((Func<Thing, ThingDef>)(x => x.def)).ToList<ThingDef>().SetsEqual<ThingDef>(list))
                        return (List<Reward>)null;
                }
            }

            rewardList.SortBy<Reward, bool>((Func<Reward, bool>)(x => x is Reward_Items));
            choice.rewards.AddRange((IEnumerable<Reward>)rewardList);

            for (int index = 0; index < rewardList.Count; ++index)
            {
                if (addDescriptionRules)
                {
                    source.Add(rewardList[index].GetDescription(parmsResolved));
                    if (!(rewardList[index] is Reward_Items))
                        stringList.Add(rewardList[index].GetDescription(parmsResolved));
                    else if (index == rewardList.Count - 1)
                        flag1 = true;
                }
                foreach (QuestPart questPart in rewardList[index].GenerateQuestParts(index, parmsResolved, customLetterLabel, customLetterText, customLetterLabelRules, customLetterTextRules))
                {
                    RimWorld.QuestGen.QuestGen.quest.AddPart(questPart);
                    choice.questParts.Add(questPart);
                }
                if (!parmsResolved.chosenPawnSignal.NullOrEmpty() && rewardList[index].MakesUseOfChosenPawnSignal)
                    chosenPawnSignalUsed = true;
            }

            if (addDescriptionRules)
            {
                TaggedString clauseSequence = source.AsEnumerable<string>().ToList<string>().ToClauseSequence();
                string str = clauseSequence.Resolve().UncapitalizeFirst();
                if (flag1)
                    str = str.TrimEnd('.');
                List<Rule> rules = new List<Rule>();
                rules.Add((Rule)new Rule_String("allRewardsDescriptions", str.UncapitalizeFirst()));
                string output;
                if (!stringList.Any<string>())
                {
                    output = "";
                }
                else
                {
                    clauseSequence = stringList.AsEnumerable<string>().ToList<string>().ToClauseSequence();
                    output = clauseSequence.Resolve().UncapitalizeFirst();
                }
                rules.Add((Rule)new Rule_String("allRewardsDescriptionsExceptItems", output));
                RimWorld.QuestGen.QuestGen.AddQuestDescriptionRules(rules);
            }

            return rewardList;
        }

        private List<Reward> DoGenerate(RewardsGeneratorParams parms, out float generatedRewardValue)
        {
            List<Reward> rewardList = new List<Reward>();

            Reward_Items newReward = new Reward_Items();
            
            Thing runefang = ThingMaker.MakeThing(RH_TET_EmpireDefOf.RH_TET_Empire_MagicWeapon_Runefang, (ThingDef)null);
            generatedRewardValue = runefang.MarketValue;

            newReward.items.Add(runefang);
            rewardList.Add(newReward);

            return rewardList;
        }
    }
}
