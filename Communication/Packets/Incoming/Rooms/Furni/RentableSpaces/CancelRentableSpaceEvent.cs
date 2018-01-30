using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.RentableSpaces;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    class CancelRentableSpaceEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int ItemId = Packet.PopInt();
            var Room = Session.GetHabbo().CurrentRoom;
            Item Item = null;

            if (Room != null)
                Item = Room.GetRoomItemHandler().GetItem(ItemId);

            if (Item != null)
            {
                if (Item.GetBaseItem().Id == 3618)
                    ProcessHouseSale(Session, Item);
                else if (Item.RentableSpaceData != null && Item.RentableSpaceData.FarmingSpace != null)
                {
                    Session.SendNotification("Sorry, but you cannot sell back a farming space! This space will run out after an hour passes from your purchase.");
                    return;
                }
                else if (Item.RentableSpaceData != null)
                {
                    if (Item.RentableSpaceData.OwnerId == Session.GetHabbo().Id || Session.GetHabbo().GetPermissions().HasRight("sell_any_plot"))
                        ProcessSale(Session, Item);
                    else
                        Session.SendNotification("You cannot sell back a plot you do not own!");
                    return;
                }
                else
                {
                    Session.SendNotification("Something happened, the sale could not be made right now.");
                    return;
                }
            }
        }

        public void ProcessHouseSale(GameClient Session, Item Item)
        {
            var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Item);
            int Cost = House.Cost;

            Item.RentableSpaceData.Enabled = false;
            House.SellHouse(Session);

            Session.Shout("*Puts their house on sale for $" + Cost + "*", 4);
            Session.SendMessage(new RentableSpaceComposer(Item, Session));
        }

        public void ProcessSale(GameClient Session, Item Item)
        {
            int Cost = (GetCost(Item) / 2);

            Session.GetHabbo().Credits += Cost;
            Session.GetHabbo().UpdateCreditsBalance();

            Item.RentableSpaceData.Enabled = false;
            Item.RentableSpaceData.OwnerId = 0;
            Item.RentableSpaceData.TimeLeft = 0;
            Item.RentableSpaceData.UpdateData();

            Session.Shout("*Sells back their plot of land for $" + Cost + "*", 4);
            Session.SendMessage(new RentableSpaceComposer(Item, Session));
        }

        public int GetCost(Item Item)
        {
            if (Item == null)
                return 2000;

            int Cost;
            string ItemName = Item.GetBaseItem().ItemName;

            switch (ItemName.ToLower())
            {
                // 3x4 Space
                case "hblooza_spacerent3x4":
                    {
                        Cost = 250;
                        break;
                    }
                // 5x5 Space
                case "hblooza_spacerent5x5":
                    {
                        Cost = 500;
                        break;
                    }
                // 6x6 Space
                case "hblooza_spacerent6x6":
                    {
                        Cost = 1000;
                        break;
                    }
                // 7x7 Space
                case "hblooza_spacerent7x7":
                    {
                        Cost = 2000;
                        break;
                    }
                // Any Other Size
                default:
                    {
                        Cost = 2000;
                        break;
                    }
            }
            return Cost;
        }
    }
}
