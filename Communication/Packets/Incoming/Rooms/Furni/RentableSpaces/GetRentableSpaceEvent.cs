using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.RentableSpaces;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    class GetRentableSpaceEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int ItemId = Packet.PopInt();
            var Room = Session.GetHabbo().CurrentRoom;
            Item Item = null;

            if (Room != null)
                Item = Room.GetRoomItemHandler().GetItem(ItemId);

            Session.SendMessage(new RentableSpaceComposer(Item, Session));
        }
    }
}
