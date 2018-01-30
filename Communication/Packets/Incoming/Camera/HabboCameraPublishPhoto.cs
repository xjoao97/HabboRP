using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Avatar;

namespace Plus.Communication.Packets.Incoming.Camera
{
    class HabboCameraPublishPhoto : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            // Uncoded?
        }
    }
}
