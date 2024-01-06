using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace TheEndTimes_Empire
{
    public class SitePartWorker_FaithfulPrisonerWillingToJoin : SitePartWorker
    {
        public override void Notify_GeneratedByQuestGen(
          SitePart part,
          Slate slate,
          List<Rule> outExtraDescriptionRules,
          Dictionary<string, string> outExtraDescriptionConstants)
        {
            base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
            Pawn prisoner = SitePartWorker_FaithfulPrisonerWillingToJoin.GenerateFaithfulPrisoner(part.site.Tile, part.site.Faction);
            part.things = (ThingOwner)new ThingOwner<Pawn>((IThingHolder)part, true, LookMode.Deep);
            part.things.TryAdd((Thing)prisoner, true);
            string pawnRelationsInfo;
            PawnRelationUtility.Notify_PawnsSeenByPlayer(Gen.YieldSingle<Pawn>(prisoner), out pawnRelationsInfo, true, false);
            string output = pawnRelationsInfo.NullOrEmpty() ? "" : (string)("\n\n" + "PawnHasTheseRelationshipsWithColonists".Translate((NamedArgument)prisoner.LabelShort, (NamedArgument)((Thing)prisoner)) + "\n\n" + pawnRelationsInfo);
            slate.Set<Pawn>("prisoner", prisoner, false);
            outExtraDescriptionRules.Add((Rule)new Rule_String("prisonerFullRelationInfo", output));
        }

        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            string str = base.GetPostProcessedThreatLabel(site, sitePart);
            if (sitePart.things != null && sitePart.things.Any)
                str = str + ": " + sitePart.things[0].LabelShortCap;
            if (site.HasWorldObjectTimeout)
                str = (string)(str + (" (" + "DurationLeft".Translate((NamedArgument)site.WorldObjectTimeoutTicksLeft.ToStringTicksToPeriod(true, false, true, true)) + ")"));
            return str;
        }

        public static Pawn GenerateFaithfulPrisoner(int tile, Faction hostFaction)
        {
            float? age = RH_TET_EmpireMod.random.Next(25, 40);
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(RH_TET_EmpireDefOf.RH_TET_Empire_FaithfulShallya, (Faction)null, PawnGenerationContext.NonPlayer, -1,
                false, false,
                false, true, false,
                5f, false, true, true,
                true, false, false,
                false, false, false,
                0.0f, 0.0f, null, 5.0f,
                null, null, null,
                null, age, age,
                null, null, null,
                null, null, null));

            try
            {
                pawn.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            catch
            {
                // Ignore if no ideos present.
            }

            pawn.guest.SetGuestStatus(hostFaction, GuestStatus.Guest);
            return pawn;
        }
    }
}
