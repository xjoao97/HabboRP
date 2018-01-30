using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Navigator;

namespace Plus.Communication.Packets.Incoming.Navigator
{
    class CanCreateRoomEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new CanCreateRoomComposer(false, 150));
        }
    }
}
