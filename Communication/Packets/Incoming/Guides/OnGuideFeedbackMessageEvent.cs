using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class OnGuideFeedbackMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            bool Feedback = Packet.PopBoolean();

            Session.SendMessage(new OnGuideSessionDetachedComposer(0));

            if (Session != null && Session.GetRoleplay() != null)
            {
                Session.GetRoleplay().Sent911Call = false;
                Session.GetRoleplay().GuideOtherUser = null;
            }
        }
    }
}
