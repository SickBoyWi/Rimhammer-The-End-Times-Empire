using DoorsExpanded;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    [StaticConstructorOnStartup]
    public partial class HarmonyPatches
    {
        // This will cause all patches in the mod to run.
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.theendtimes.empire");

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(QuestUtility),
                    name: "GenerateQuestAndMakeAvailable",
                    parameters: new Type[] { typeof(QuestScriptDef), typeof(Slate) }),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(QuestUtility_GenerateQuestAndMakeAvailable_PreFix)),
                    postfix: null);

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(Building_DoorExpanded),
                    name: "DrawFrameParams"
                    //,
                    //parameters: new Type[] { typeof(QuestScriptDef), typeof(Slate) }
                    ),
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Building_DoorExpanded_DrawFrameParams_PreFix)),
                    postfix: null);

            harmony.Patch(original: AccessTools.Method(
                    type: typeof(IncidentWorker_GiveQuest),
                    name: "CanFireNowSub",
                    parameters: new Type[] { typeof(IncidentParms) }),
                    prefix: null,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IncidentWorker_GiveQuest_CanFireNowSub_Postfix)));

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(BuildingProperties), "IsMortar", MethodType.Getter)]
        static class Patch_BuildingProperties_IsMortar
        {
            static void Postfix(ref BuildingProperties __instance, ref bool __result)
            {
                if (!__result)
                {
                    if (__instance != null && __instance.turretGunDef != null &&
                        (__instance.turretGunDef.defName.Equals("RH_TET_Empire_Mortar_Turret")
                        //|| __instance.turretGunDef.defName.Equals("RH_TET_Empire_Cannon_Turret")
                        //|| __instance.turretGunDef.defName.Equals("RH_TET_Empire_Hellblaster_Turret")
                        //|| __instance.turretGunDef.defName.Equals("RH_TET_Empire_RocketBattery_Turret")
                        ))
                    {
                        __result = true;
                    }
                }
            }
        }

        //Restrict shallya prisoner joins quest by the number of other faithful pawns the player has. Capped at 3.
        private static void IncidentWorker_GiveQuest_CanFireNowSub_Postfix(ref bool __result, ref IncidentParms parms)
        {
            if (__result && parms != null && parms.questScriptDef != null && parms.questScriptDef.defName.Equals("RH_TET_Empire_OpportunitySite_FaithfulWillingToJoin"))
            {
                Faction ofPlayer = Faction.OfPlayer;

                int faithfulCount = 0;

                // If the player has more than three faith pawns already, don't allow this event.
                List<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;

                foreach (Pawn p in playerPawns)
                {
                    if (p.def != null)
                    {
                        if (p.def.defName.Equals("RH_TET_Empire_FaithfulShallya"))
                            faithfulCount++;
                        else if (p.def.defName.Equals("RH_TET_Empire_FaithfulSigmar"))
                        {
                            faithfulCount++;
                        }
                        else if (p.def.defName.Equals("RH_TET_Empire_FaithfulUlric"))
                        {
                            faithfulCount++;
                        }
                    }

                    if (faithfulCount >= 3)
                        break;
                }

                if (faithfulCount >= 3)
                    __result = false;
            }
        }

        // Restrict Empire quests to Empire only.
        private static void QuestUtility_GenerateQuestAndMakeAvailable_PreFix(ref Quest __result, ref QuestScriptDef root, ref Slate vars)
        {
            Faction ofPlayer = Faction.OfPlayer;
            if (root == null || root.defName == null)
                return;

            if (ofPlayer != null)
            {
                // Should only happen to this mod factions.
                if (root.defName.Equals("RH_TET_Empire_OpportunitySite_FaithfulWillingToJoin"))
                {
                    if (ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerWizard")
                        || ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerColony"))
                    {
                        bool doQuest = false;

                        int faithfulCount = 0;

                        // If the player has more than three faith pawns already, don't allow this event.
                        List<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists;

                        foreach (Pawn p in playerPawns)
                        {
                            if (p.def != null && p.kindDef != null)
                            {
                                if (p.kindDef.defName.Equals("RH_TET_Empire_FaithfulShallya"))
                                    faithfulCount++;
                                else if (p.kindDef.defName.Equals("RH_TET_Empire_FaithfulSigmar"))
                                    faithfulCount++;
                                else if (p.kindDef.defName.Equals("RH_TET_Empire_FaithfulUlric"))
                                    faithfulCount++;
                            }

                            if (faithfulCount >= 3)
                                break;
                        }

                        if (faithfulCount < 3)
                            doQuest = true;

                        // For non this mod factions, swap it out with a different quest.
                        if (!doQuest)
                            root = DefDatabase<QuestScriptDef>.GetNamed("OpportunitySite_PrisonerWillingToJoin");
                    }
                    else
                    {
                        root = DefDatabase<QuestScriptDef>.GetNamed("OpportunitySite_PrisonerWillingToJoin");
                    }
                }
                else if (root.defName.Equals("RH_TET_Empire_SigmarHammer"))
                {
                    if (!(ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerWizard")
                        || ofPlayer.def.defName.Equals("RH_TET_Empire_PlayerColony")))
                    {
                        // For non this mod factions, swap it out with a different quest.
                        root = QuestScriptDefOf.OpportunitySite_ItemStash;
                    }
                }
            }

            __result = null;
            return;
        }

        private static bool Building_DoorExpanded_DrawFrameParams_PreFix(
                  ref ThingDef def,
                  ref CompProperties_DoorExpanded props,
                  ref Vector3 drawPos,
                  ref Rot4 rotation,
                  ref bool split,
                  out Mesh mesh,
                  out Matrix4x4 matrix
            )
        {
            if (def != null && def.defName.Equals("RH_TET_Empire_CastleGate") && rotation == Rot4.South)
            {
                rotation = Rot4.North;

                int numOne = rotation.IsHorizontal ? 1 : 0;
                Vector3 vector3_1 = new Vector3(-1f, 0.0f, 0.0f);
                mesh = MeshPool.plane10;

                Quaternion queue = rotation.AsQuat;

                vector3_1 = queue * vector3_1;
                float num2 = (float)(0.0 + (double)props.doorOpenMultiplier * 1.0) * (float)def.Size.x;
                vector3_1 *= num2;
                Vector2 drawSizeLocal = props.doorFrame.drawSize;
                float numThree = numOne == 0 || !props.fixedPerspective ? 1f : 2f;
                Vector3 sss = new Vector3(drawSizeLocal.x * numThree, 1f, drawSizeLocal.y * numThree);
                Vector3 position = drawPos;
                position.y = AltitudeLayer.Blueprint.AltitudeFor();
                if (rotation == Rot4.North || rotation == Rot4.South)
                    position.y = AltitudeLayer.PawnState.AltitudeFor();
                if (numOne == 0)
                    position.x += num2;
                position += vector3_1;
                Vector3 vector3_2 = props.doorFrameOffset;
                position += vector3_2;
                matrix = Matrix4x4.TRS(position, queue, sss);
            }

            mesh = MeshPool.plane10;
            Vector3 pos = new Vector3(); 
            Quaternion q = Rot4.North.AsQuat;
            Vector2 drawSize = props.doorFrame.drawSize;
            int num1 = rotation.IsHorizontal ? 1 : 0;
            float num3 = num1 == 0 || !props.fixedPerspective ? 1f : 2f;
            Vector3 s = new Vector3(drawSize.x * num3, 1f, drawSize.y * num3);
            matrix = Matrix4x4.TRS(pos, q, s);

            return true;
        }
    }
}