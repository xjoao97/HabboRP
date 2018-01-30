using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class HelperToolConfigurationComposer : ServerPacket
    {
        public HelperToolConfigurationComposer(GameClient Session)
            : base(ServerPacketHeader.HelperToolConfigurationComposer)
        {
            GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();

            if (Session == null || Session.GetRoleplay() == null)
                base.WriteBoolean(false);
            else
                base.WriteBoolean(Session.GetRoleplay().IsWorking);
            base.WriteInteger(guideManager.GuidesCount);
            base.WriteInteger(guideManager.HelpersCount);
            base.WriteInteger(guideManager.GuardiansCount);
        }
    }
}