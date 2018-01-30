using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class CancellInviteGuide : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new OnGuideSessionDetachedComposer(2));
        }
    }
}
