using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class OnGuideSessionTyping : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            bool Typing = Packet.PopBoolean();

            if (Session != null && Session.GetRoleplay() != null && Session.GetRoleplay().GuideOtherUser != null)
                Session.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionPartnerIsTypingComposer(Typing));
        }
    }
}
