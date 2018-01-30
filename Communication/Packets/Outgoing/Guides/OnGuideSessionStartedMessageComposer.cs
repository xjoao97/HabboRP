using System;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class OnGuideSessionStartedMessageComposer : ServerPacket
    {
        public OnGuideSessionStartedMessageComposer(GameClient Session, GameClient requester)
            : base(ServerPacketHeader.OnGuideSessionStartedComposer)
        {
            base.WriteInteger(requester.GetHabbo().Id);
            base.WriteString(requester.GetHabbo().Username);
            base.WriteString(requester.GetHabbo().Look);
            base.WriteInteger(Session.GetHabbo().Id);
            base.WriteString(Session.GetHabbo().Username);
            base.WriteString(Session.GetHabbo().Look);
        }
    }
}