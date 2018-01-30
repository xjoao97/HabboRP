using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Misc;

using Plus.Communication.Packets.Outgoing.Avatar;

namespace Plus.Communication.Packets.Incoming.Avatar
{
    class GetWardrobeEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int ClothingRoom = Convert.ToInt32(RoleplayData.GetData("clothing", "roomid"));

            if (Session.GetRoomUser() == null || !Session.GetHabbo().InRoom)
                Session.SendNotification("Você so pode mudar seu visual na Loja de Roupas! [Quarto ID: " + ClothingRoom + "]");

            if (Session.GetRoomUser().RoomId != ClothingRoom)
                Session.SendNotification("Você so pode mudar seu visual na Loja de Roupas! [Quarto ID: " + ClothingRoom + "]");

            Session.SendMessage(new WardrobeComposer(Session));
        }
    }
}
