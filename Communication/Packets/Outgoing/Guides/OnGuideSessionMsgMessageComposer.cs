using System;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class OnGuideSessionMsgComposer : ServerPacket
    {
        public OnGuideSessionMsgComposer(GameClient Session, string message)
            : base(ServerPacketHeader.OnGuideSessionMsgComposer)
        {
            base.WriteString(message);
            base.WriteInteger(Session.GetHabbo().Id);
        }
    }
}