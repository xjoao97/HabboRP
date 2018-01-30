using System;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Crafting
{
    class GetCraftingListMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int count = Packet.PopInt();
        }
    }
}
