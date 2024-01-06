using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheEndTimes_Empire
{
    public static class RH_TET_Empire_GameVictoryUtility
    {
        private static void ShowCredits(string victoryText)
        {
            Screen_Credits screen_Credits = new Screen_Credits(victoryText);
            screen_Credits.wonGame = true;
            Find.WindowStack.Add(screen_Credits);
            Find.MusicManagerPlay.ForceSilenceFor(999f);
            ScreenFader.StartFade(Color.clear, 3.0f);
        }

        private static void InitiateVictory(Map map)
        {
            DefDatabase<SoundDef>.GetNamed("PsychicPulseGlobal", true).PlayOneShot((SoundInfo)new TargetInfo(map.Center, map, false));
            RH_TET_EmpireDefOf.RH_TET_Empire_EndExplosion.PlayOneShot((SoundInfo)new TargetInfo(map.Center, map, false));
            RH_TET_EmpireDefOf.RH_TET_Empire_Victory.PlayOneShotOnCamera(null);
            ScreenFader.StartFade(Color.yellow, 5.0f);// TODO ADD IN DAWI MOD IF IT WORKS. DOESN'T WORK, HAVE TO SET REMAINING TIME TO 5 SECS for fadeout to happen.
        }

        public static void PlayerVictory(Map map)
        {
            InitiateVictory(map);
            string victoryText = "RH_TET_Empire_Victory".Translate();
            ShowCredits(victoryText);
        }
    }
}
