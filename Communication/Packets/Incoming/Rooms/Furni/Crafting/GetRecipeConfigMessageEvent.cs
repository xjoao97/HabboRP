using System;
using System.Linq;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Crafting;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Crafting
{
    class GetRecipeConfigMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            string item = Packet.PopString();

            var CraftingItem = CraftingManager.getRecipe(item);

            if (CraftingItem == null)
            {
                Session.SendWhisper("Esta receita não pôde ser encontrada! Notificar um desenvolvedor!", 1);
                return;
            }

            ICollection<Item> FloorItems = Session.GetHabbo().GetInventoryComponent().GetFloorItems();
            ICollection<Item> WallItems = Session.GetHabbo().GetInventoryComponent().GetWallItems();

            Session.GetRoleplay().CraftingCheck = false;
            Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, Session.GetRoleplay().CraftingCheck));

            Session.SendMessage(new CraftingRecipeMessageComposer(CraftingItem));

            Session.GetRoleplay().CraftingCheck = true;
            Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, Session.GetRoleplay().CraftingCheck));
        }
    }
}
