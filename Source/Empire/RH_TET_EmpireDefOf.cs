using HugsLib.Utils;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    [DefOf]
    public static class RH_TET_EmpireDefOf
    {
        // Core
        public static LifeStageDef HumanlikeAdult;

        // Custom
        //public static HairDef RH_TET_Empire_None;

        // Faction
        public static FactionDef RH_TET_Beastmen_GorFaction;

        // Pawn kinds.
        public static PawnKindDef RH_TET_Empire_FaithfulSigmar;
        public static PawnKindDef RH_TET_Empire_FaithfulShallya;
        public static PawnKindDef RH_TET_Empire_FaithfulUlric;
        public static PawnKindDef RH_TET_Empire_WizardStandard_Beasts;
        public static PawnKindDef RH_TET_Empire_WizardStandard_Bright;
        public static PawnKindDef RH_TET_Empire_WizardStandard_Celestial;
        public static PawnKindDef RH_TET_Empire_WizardStandard_Spirit;
        public static PawnKindDef RH_TET_Empire_WizardStandard_Grey;
        public static PawnKindDef RH_TET_Empire_WizardStandard_Jade;
        public static PawnKindDef RH_TET_Empire_WizardStandard_White;
        public static PawnKindDef RH_TET_Empire_WizardStandard_Gold;
        public static PawnKindDef RH_TET_Empire_WizardGreat_Beasts;
        public static PawnKindDef RH_TET_Empire_WizardGreat_Bright;
        public static PawnKindDef RH_TET_Empire_WizardGreat_Celestial;
        public static PawnKindDef RH_TET_Empire_WizardGreat_Spirit;
        public static PawnKindDef RH_TET_Empire_WizardGreat_Grey;
        public static PawnKindDef RH_TET_Empire_WizardGreat_Jade;
        public static PawnKindDef RH_TET_Empire_WizardGreat_White;
        public static PawnKindDef RH_TET_Empire_WizardGreat_Gold;
        public static PawnKindDef RH_TET_Troll;
        public static PawnKindDef RH_TET_StoneTroll;
        public static PawnKindDef RH_TET_RiverTroll;

        // Races
        public static ThingDef RH_TET_TrollRace;
        public static ThingDef RH_TET_StoneTrollRace;
        public static ThingDef RH_TET_RiverTrollRace;

        // =============== World Stuff =============== 
        public static WorldObjectDef RH_TET_Empire_SigmarHammerWorldObject;
        public static SitePartDef RH_TET_FaithfulPrisonerWillingToJoin;
        public static MapGeneratorDef RH_TET_Empire_EmptyMap;
        public static MapGenDef RH_TET_Empire_SigmarHammerMap;

        // Buildings
        public static ThingDef RH_TET_Empire_Lute;
        public static ThingDef RH_TET_Empire_Brazier;
        public static ThingDef RH_TET_Empire_ColumnSmallA;
        public static ThingDef RH_TET_Empire_DoubleBed;
        public static ThingDef RH_TET_Empire_RoyalBed;
        public static ThingDef RH_TET_Empire_EndTable;
        public static ThingDef RH_TET_Empire_Armoire;

        // Jobs
        public static JobDef RH_TET_Empire_EffigyBeginHoldBeginPoweringCere;
        public static JobDef RH_TET_Empire_EffigyGathering;

        // Recipes
        public static RecipeDef RH_TET_Empire_RepairApparel;
        public static RecipeDef RH_TET_Empire_RepairArmor;
        public static RecipeDef RH_TET_Empire_RepairMeleeWeapon;
        public static RecipeDef RH_TET_Empire_RepairRangedWeapon;

        // Tales
        public static TaleDef RH_TET_Empire_ClaimedHammer;

        // Things
        public static ThingDef RH_TET_MeleeWeapon_SigmarHammer;

        // Sounds
        public static SoundDef RH_TET_Empire_Victory;
        public static SoundDef RH_TET_Empire_Chanting;
        public static SoundDef RH_TET_Empire_EndExplosion;

        // Thoughts
        public static ThoughtDef RH_TET_Empire_PraisedSigmar;
    } 
}