using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Session;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class VisitRoomGuides : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetRoleplay().GuideOtherUser == null)
                return;

            GameClient requester = Session.GetRoleplay().GuideOtherUser;
            Session.SendMessage(new RoomForwardComposer(requester.GetHabbo().CurrentRoomId));
        }
    }
}
