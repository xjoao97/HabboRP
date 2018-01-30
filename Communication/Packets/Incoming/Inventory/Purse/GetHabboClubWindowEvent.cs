using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Inventory.Purse
{
    class GetHabboClubWindowEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int WindowId = Packet.PopInt();

            Session.SendMessage(new HabboClubOffersMessageComposer(Session, WindowId));
        }
    }
}
