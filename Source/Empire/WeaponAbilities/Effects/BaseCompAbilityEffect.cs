//using HugsLib.Utils;
//using RimWorld;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;
//using Verse;

//namespace TheEndTimes_Empire
//{
//    public class BaseCompAbilityEffect : CompAbilityEffect
//    {
//        public override bool GizmoDisabled(out string reason)
//        {
//            if (parent.pawn.equipment.Primary == null)
//            {
//                reason = "RH_TET_Empire_RequireWeapon".Translate();
//                return true;
//            }
//            else if (!parent.pawn.equipment.Primary.def.IsMeleeWeapon)
//            {
//                reason = "RH_TET_Empire_RequireWeapon".Translate();
//                return true;
//            }

//            reason = "RH_TET_Empire_HasWeapon".Translate();
//            return false;
//        }
//    }
//}