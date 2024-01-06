using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace TheEndTimes_Empire
{
    public class SigmarHammerSiteComp : WorldObjectComp
    {
        public SigmarHammerSite Parent;

        public SigmarHammerSiteComp()
        {
            this.Parent = (SigmarHammerSite)this.parent;
        }

        public override void CompTick()
        {
            base.CompTick();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            return stringBuilder.ToString();
        }
    }
}
