//using HugsLib.Utils;
//using RimWorld;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using UnityEngine;
//using Verse;

//namespace TheEndTimes_Empire
//{
//    public class CompAbilityEffect_Murderstroke : BaseCompAbilityEffect
//    {
//        CompProperties_AbilityMurderstroke _Props;
//        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
//        {
//            _Props = (CompProperties_AbilityMurderstroke)Props;

//            if (target.Pawn == null || _Props == null)
//                return;

//            BodyPartRecord torso = target.Pawn?.health.hediffSet.GetNotMissingParts(0, 0, null, null).FirstOrDefault(x => x.def == _Props.torsoDef);

//            if (torso == null)
//                return;

//            target.Pawn.TakeDamage(new DamageInfo(_Props.damageDef, _Props.damage, _Props.armourPen, -1, parent.pawn, torso, parent.pawn.equipment.Primary.def));
//            MoteMaker.ThrowText(target.Pawn.PositionHeld.ToVector3(), target.Pawn.MapHeld, "RH_TET_Empire_Murderstroke".Translate(), 6f);
//        }
//    }
//}