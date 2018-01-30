using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;



namespace Plus.Communication.Packets.Incoming.Inventory.Furni
{
    class RequestFurniInventoryEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            ICollection<Item> FloorItems = Session.GetHabbo().GetInventoryComponent().GetFloorItems();
            ICollection<Item> WallItems = Session.GetHabbo().GetInventoryComponent().GetWallItems();

            if (Session.GetHabbo().InventoryAlert == false)
            {
                Session.GetHabbo().InventoryAlert = true;
                int TotalCount = FloorItems.Count + WallItems.Count;
                if (TotalCount >= 5000)
                {
                    Session.SendNotification("Hey! Nosso sistema detectou que você possui um inventário muito grande!\n\n" +
                        "O máximo que um inventário pode carregar é de 5000 itens, você tem " + TotalCount + " items agora.\n\n" +
                        "Se você tiver 5000 carregados agora, provavelmente você está acima do limite e alguns itens serão escondidos até libertar espaço.\n\n" +
                        "Por favor, note que não somos responsáveis ​​por você falhar por causa de inventários muito grandes!");
                }
            }

            bool CraftingCheck = Session.GetRoleplay().CraftingCheck;
            Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, CraftingCheck));
        }
    }
}
