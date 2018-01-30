using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class GuideToolMessageNew : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string message = Packet.PopString();
            GameClient requester = Session.GetRoleplay().GuideOtherUser;

            requester.SendMessage(new OnGuideSessionMsgComposer(Session, message));
            Session.SendMessage(new OnGuideSessionMsgComposer(Session, message));
        }
    }
}
