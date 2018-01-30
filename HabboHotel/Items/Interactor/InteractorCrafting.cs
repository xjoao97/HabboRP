using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorCrafting : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "";
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session.GetRoomUser() == null)
                return;

            var User = Session.GetRoomUser();

            if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                ICollection<Item> FloorItems = Session.GetHabbo().GetInventoryComponent().GetFloorItems();
                ICollection<Item> WallItems = Session.GetHabbo().GetInventoryComponent().GetWallItems();

                Session.GetRoleplay().CraftingCheck = false;
                Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, Session.GetRoleplay().CraftingCheck));

                Session.SendMessage(new CraftableProductsMessageComposer(Session));

                Session.GetRoleplay().CraftingCheck = true;
                Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, Session.GetRoleplay().CraftingCheck));
            }
            else
                User.MoveTo(Item.SquareInFront);
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}