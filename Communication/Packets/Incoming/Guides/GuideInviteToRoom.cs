using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class GuideInviteToRoom : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            GameClient requester = Session.GetRoleplay().GuideOtherUser;

            requester.SendMessage(new OnGuideSessionInvitedToGuideRoomComposer(Session));
            Session.SendMessage(new OnGuideSessionInvitedToGuideRoomComposer(Session));
        }
    }
}
