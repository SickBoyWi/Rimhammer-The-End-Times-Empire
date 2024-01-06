using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace TheEndTimes_Empire
{
    partial class HarmonyPatchesRoyaltyIdeology
    {
        // Alter Ideos here, if they're present.
        [HarmonyPatch(typeof(Alert_IdeoBuildingDisrespected), "GetReport")]
        static class Patch_Alert_IdeoBuildingDisrespected_GetReport
        {
            static bool Prefix()
            {
                if (RH_TET_EmpireMod.ideologyAlertPatched)
                    return true;

                Faction ofPlayer = Faction.OfPlayerSilentFail;
                if (ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerWizard"))
                {
                    if (ModsConfig.IdeologyActive)
                    {
                        PreceptDef ideoBuilding = DefDatabase<PreceptDef>.GetNamed("IdeoBuilding");
                        if (ideoBuilding != null)
                        {
                            foreach (RoomRequirement rr in ideoBuilding.buildingRoomRequirements)
                            {
                                if (rr != null && rr is RoomRequirement_ThingCount)
                                {
                                    RoomRequirement_ThingCount rrtc = rr as RoomRequirement_ThingCount;
                                    if (rrtc.thingDef == ThingDefOf.Column)
                                        rrtc.thingDef = RH_TET_EmpireDefOf.RH_TET_Empire_ColumnSmallA;
                                }
                            }
                        }
                        else
                            Log.Warning("Rimhammer Empire could not find IdeoBuilding in order to update required buildings.");
                    }

                    RH_TET_EmpireMod.ideologyAlertPatched = true;
                }

                return true;
            }
        }

        // Alter Royalty Titles here, if they're present.
        [HarmonyPatch(typeof(Alert_UndignifiedThroneroom), "GetReport")]
        static class Patch_Alert_UndignifiedThroneroom_GetReport
        {
            static bool Prefix()
            {
                if (RH_TET_EmpireMod.royaltyAlertPatched)
                    return true;

                Faction ofPlayer = Faction.OfPlayerSilentFail;
                if (ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerColony") || ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerWizard"))
                {
                    if (ModsConfig.RoyaltyActive)
                    {
                        RoyalTitleDef acolyte = DefDatabase<RoyalTitleDef>.GetNamed("Acolyte");
                        RoyalTitleDef knight = DefDatabase<RoyalTitleDef>.GetNamed("Knight");
                        RoyalTitleDef praetor = DefDatabase<RoyalTitleDef>.GetNamed("Praetor");
                        RoyalTitleDef baron = DefDatabase<RoyalTitleDef>.GetNamed("Baron");
                        RoyalTitleDef count = DefDatabase<RoyalTitleDef>.GetNamed("Count");
                        List<RoyalTitleDef> titles = new List<RoyalTitleDef>();
                        titles.Add(acolyte);
                        titles.Add(knight);
                        titles.Add(praetor);
                        titles.Add(baron);
                        titles.Add(count);

                        ThingDef endTable = DefDatabase<ThingDef>.GetNamed("EndTable");
                        ThingDef dresser = DefDatabase<ThingDef>.GetNamed("Dresser");
                        ThingDef doubleBed = DefDatabase<ThingDef>.GetNamed("DoubleBed");

                        foreach (RoyalTitleDef rtd in titles)
                        {
                            if (rtd != null && rtd.throneRoomRequirements != null)
                            {
                                foreach (RoomRequirement rr in rtd.throneRoomRequirements)
                                {
                                    if (rr is RoomRequirement_ThingCount)
                                    {
                                        RoomRequirement_ThingCount rrtc = rr as RoomRequirement_ThingCount;

                                        if (rrtc.thingDef == ThingDefOf.Brazier)
                                            rrtc.thingDef = RH_TET_EmpireDefOf.RH_TET_Empire_Brazier;
                                        else if (rrtc.thingDef == ThingDefOf.Column)
                                            rrtc.thingDef = RH_TET_EmpireDefOf.RH_TET_Empire_ColumnSmallA;
                                        //else if (rrtc.thingDef == ThingDefOf.Drape)  TODO
                                        //    rrtc.thingDef = RH_TET_EmpireDefOf.RH_TET_Dwarfs_Banner;
                                    }
                                    else if (rr is RoomRequirement_ThingAnyOfCount)
                                    {
                                        RoomRequirement_ThingAnyOfCount rrtaoc = rr as RoomRequirement_ThingAnyOfCount;

                                        List<ThingDef> replace = new List<ThingDef>();

                                        foreach (ThingDef thing in rrtaoc.things)
                                        {
                                            if (thing == ThingDefOf.Brazier)
                                                replace.Add(thing);
                                            else if (thing == ThingDefOf.Column)
                                                replace.Add(thing);
                                            else if (thing == ThingDefOf.Drape)
                                                replace.Add(thing);
                                        }

                                        foreach (ThingDef thing in replace)
                                        {
                                            if (thing == ThingDefOf.Brazier)
                                                rrtaoc.things.Replace<ThingDef>(thing, RH_TET_EmpireDefOf.RH_TET_Empire_Brazier);
                                            else if (thing == ThingDefOf.Column)
                                                rrtaoc.things.Replace<ThingDef>(thing, RH_TET_EmpireDefOf.RH_TET_Empire_ColumnSmallA);
                                            //else if (thing == ThingDefOf.Drape)  TODO
                                            //    rrtaoc.things.Replace<ThingDef>(thing, RH_TET_DwarfDefOf.RH_TET_Dwarfs_Banner);
                                        }
                                    }
                                    else if (rr is RoomRequirement_AllThingsAreGlowing)
                                    {
                                        RoomRequirement_AllThingsAreGlowing rrag = rr as RoomRequirement_AllThingsAreGlowing;

                                        if (rrag.thingDef == ThingDefOf.Brazier)
                                            rrag.thingDef = RH_TET_EmpireDefOf.RH_TET_Empire_Brazier;
                                    }
                                    else if (rr is RoomRequirement_ThingAnyOf)
                                    {
                                        if (rtd.defName.Equals("Knight"))
                                        {
                                            RoomRequirement_ThingAnyOf rrtao = rr as RoomRequirement_ThingAnyOf;
                                            rrtao.things.Add(RH_TET_EmpireDefOf.RH_TET_Empire_Lute);
                                        }
                                    }
                                }
                            }
                            if (rtd != null && rtd.bedroomRequirements != null)
                            {
                                foreach (RoomRequirement rr in rtd.bedroomRequirements)
                                {
                                    if (rr is RoomRequirement_ThingAnyOf)
                                    {
                                        RoomRequirement_ThingAnyOf rrtao = rr as RoomRequirement_ThingAnyOf;

                                        if (rrtao.things.Contains(doubleBed))
                                        {
                                            rrtao.things.Add(RH_TET_EmpireDefOf.RH_TET_Empire_DoubleBed);
                                        }
                                        if (rrtao.things.Contains(ThingDefOf.RoyalBed))
                                        {
                                            rrtao.things.Add(RH_TET_EmpireDefOf.RH_TET_Empire_RoyalBed);
                                        }
                                    }
                                    else if (rr is RoomRequirement_Thing)
                                    {
                                        RoomRequirement_Thing rrt = rr as RoomRequirement_Thing;

                                        if (rrt.thingDef.Equals(endTable))
                                        {
                                            rrt.thingDef = RH_TET_EmpireDefOf.RH_TET_Empire_EndTable;
                                        }
                                        else if (rrt.thingDef.Equals(dresser))
                                        {
                                            rrt.thingDef = RH_TET_EmpireDefOf.RH_TET_Empire_Armoire;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                RH_TET_EmpireMod.royaltyAlertPatched = true;

                return true;
            }
        }
    }
}
