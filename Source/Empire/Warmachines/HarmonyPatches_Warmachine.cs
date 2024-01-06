using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    partial class HarmonyPatches_Warmachine
    {
        private static FloatRange smokeSize = new FloatRange(0.25f, 0.5f);
        private static int stackLimitSaveHellblaster = 0;
        private static int stackLimitSaveRocketBattery = 0;

        [HarmonyPatch(typeof(Building_TurretGun), "BeginBurst")]
        static class Patch_Building_TurretGun_BeginBurst
        {
            static void Prefix(Building_TurretGun __instance)
            {
                if (__instance.def.defName.Equals("RH_TET_Empire_Hellblaster"))
                {
                    stackLimitSaveHellblaster = __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit;

                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = 5;
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadShell(ThingDef.Named("RH_TET_Empire_Hellblaster_Round"), 5);
                }
                else if (__instance.def.defName.Equals("RH_TET_Empire_RocketBattery"))
                {
                    stackLimitSaveRocketBattery = __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit;

                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = 3; 
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadShell(ThingDef.Named("RH_TET_Empire_RocketBattery_Rocket"), 3);
                }
            }

            static void Postfix(Building_TurretGun __instance)
            {
                if (__instance.def.defName.Equals("RH_TET_Empire_Mortar")
                    || __instance.def.defName.Equals("RH_TET_Empire_Cannon"))
                {
                    ResolveSmoke(__instance);
                }
                else if (__instance.def.defName.Equals("RH_TET_Empire_RocketBattery"))
                {
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = stackLimitSaveRocketBattery;
                    ResolveSmoke(__instance);
                }
                else if (__instance.def.defName.Equals("RH_TET_Empire_Hellblaster"))
                {
                    __instance.gun.TryGetComp<CompChangeableProjectile>().LoadedShell.stackLimit = stackLimitSaveHellblaster;
                    ResolveSmoke(__instance);
                }
            }

            private static void ResolveSmoke(Building_TurretGun __instance)
            {
                IntVec3 position = __instance.Position;
                Map map = __instance.Map;
                float statValue = 2f;
                DamageDef smoke = DamageDefOf.Smoke;
                GenExplosion.DoExplosion(position, map, statValue, smoke, null, -1, -1f, null, null, null, null, null, 1f, 1, new GasType?(GasType.BlindSmoke), false, null, 0f, 1, 0f, false);
            }
        }
    }
}
