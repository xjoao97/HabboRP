using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Camera;

namespace Plus.Communication.Packets.Incoming.Camera
{
    class GetCameraPriceMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new SetCameraPriceMessageComposer());
        }
    }
}
