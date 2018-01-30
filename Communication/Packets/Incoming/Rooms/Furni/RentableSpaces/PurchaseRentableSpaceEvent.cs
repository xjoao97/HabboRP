using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.RentableSpaces;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Farming;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    class PurchaseRentableSpaceEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int ItemId = Packet.PopInt();
            var Room = Session.GetHabbo().CurrentRoom;
            Item Item = null;

            if (Room != null)
                Item = Room.GetRoomItemHandler().GetItem(ItemId);

            if (Item != null)
            {
                if (Item.GetBaseItem().Id == 3618)
                    ProcessHousePurchase(Session, Item);
                else if (Item.RentableSpaceData != null && Item.RentableSpaceData.FarmingSpace != null)
                    ProcessFarmingPurchase(Session, Item);
                else if (Item.RentableSpaceData != null)
                {
                    if (CanPurchase(Session, Item))
                    {
                        ProcessPurchase(Session, Item);
                        return;
                    }
                    else
                    {
                        Session.SendNotification("You have already purchased a plot of land in this room!");
                        return;
                    }
                }
                else
                {
                    Session.SendNotification("Something happened, the purchase could not be made right now.");
                    return;
                }
            }
        }

        public void ProcessFarmingPurchase(GameClient Session, Item Item)
        {
            if (Item != null && Item.RentableSpaceData != null && Item.RentableSpaceData.FarmingSpace != null)
            {
                if (Item.RentableSpaceData.FarmingSpace.OwnerId <= 0)
                {
                    lock (FarmingManager.FarmingSpaces)
                    {
                        bool HasSpace = FarmingManager.FarmingSpaces.Values.Where(x => x != null && x.OwnerId == Session.GetHabbo().Id).ToList().Count > 0;

                        if (!HasSpace)
                        {
                            if (!GroupManager.HasJobCommand(Session, "farming"))
                            {
                                Session.SendNotification("Only a farmer can rent a plot of land!");
                                return;
                            }

                            if (!Session.GetRoleplay().IsWorking)
                            {
                                Session.SendNotification("You must be working to buy a plot of land!");
                                return;
                            }

                            if (!Session.GetRoleplay().FarmingStats.HasPlantSatchel && !Session.GetRoleplay().FarmingStats.HasSeedSatchel)
                            {
                                Session.SendNotification("Before you can begin farming, you need to buy a Plant Satchel and Seed Satchel at the Supermarket!");
                                return;
                            }

                            int Cost = Item.RentableSpaceData.FarmingSpace.Cost;

                            Session.GetHabbo().Credits -= Cost;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Item.RentableSpaceData.FarmingSpace.BuySpace(Session, Item);
                            Session.Shout("*Purchases the farming space for $" + Cost + "*", 4);
                            Session.SendMessage(new RentableSpaceComposer(Item, Session));
                            return;
                        }
                        else
                        {
                            Session.SendWhisper("You cannot purchase more than one farming plot at a time!", 1);
                            return;
                        }
                    }
                }
                else
                {
                    Session.SendNotification("This farming space is not for sale!");
                    return;
                }
            }
        }


        public void ProcessHousePurchase(GameClient Session, Item Item)
        {
            var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Item);

            if (House != null)
            {
                if (CanPurchase(Session, House))
                {
                    Item.RentableSpaceData.Enabled = true;
                    Item.RentableSpaceData.OwnerId = Session.GetHabbo().Id;

                    if (House.OwnerId != Session.GetHabbo().Id)
                    {
                        int Cost = GetCost(House);

                        Session.GetHabbo().Credits -= Cost;
                        Session.GetHabbo().UpdateCreditsBalance();
                        House.BuyHouse(Session, Cost);
                        Session.Shout("*Purchases the house for $" + Cost + "*", 4);
                    }
                    else
                    {
                        House.BuyHouse(Session, 0);
                        Session.Shout("*Takes their house off the market*", 4);
                    }

                    UnloadRoom(House);
                    Session.SendMessage(new RentableSpaceComposer(Item, Session));
                }
                else
                {
                    Session.SendNotification("You can only own one house at a time!");
                    return;
                }
            }
        }

        public bool CanPurchase(GameClient Session, Item Item)
        {
            if (Item.RentableSpaceData.Enabled == true)
                return false;

            var Room = Item.GetRoom();
            var SpaceItems = Room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.RENTABLE_SPACE).ToList();

            bool AlreadyOwns = false;
            if (SpaceItems.Count > 1)
            {
                foreach (var item in SpaceItems)
                {
                    if (item.RentableSpaceData == null)
                        continue;

                    if (item.RentableSpaceData.OwnerId == Session.GetHabbo().Id)
                    { 
                        AlreadyOwns = true;
                        break;
                    }
                }
            }

            if (AlreadyOwns)
                return false;
            else
                return true;
        }

        public bool CanPurchase(GameClient Session, House House)
        {
            if (!House.ForSale)
                return false;

            if (House.OwnerId == Session.GetHabbo().Id)
                return true;

            if (PlusEnvironment.GetGame().GetHouseManager().HouseList.Values.Where(x => x.OwnerId == Session.GetHabbo().Id).ToList().Count > 0)
                return false;

            return true;
        }

        public void ProcessPurchase(GameClient Session, Item Item)
        {
            int Cost = GetCost(Item);

            Session.GetHabbo().Credits -= Cost;
            Session.GetHabbo().UpdateCreditsBalance();

            Item.RentableSpaceData.Enabled = true;
            Item.RentableSpaceData.OwnerId = Session.GetHabbo().Id;
            Item.RentableSpaceData.TimeLeft = (30 * 24 * 60 * 60); // 30 Days in Seconds
            Item.RentableSpaceData.UpdateData();

            Session.Shout("*Purchases the plot of land for $" + Cost + "*", 4);
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

        public int GetCost(House House)
        {
            if (House != null)
                return House.Cost;
            else
                return 20000;
        }

        public void UnloadRoom(House House)
        {
            if (House == null)
                return;

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(House.RoomId, out Room))
                return;

            List<RoomUser> UsersToReturn = Room.GetRoomUserManager().GetRoomUsers().ToList();

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room, true);

            foreach (RoomUser User in UsersToReturn)
            {
                if (User == null || User.GetClient() == null)
                    continue;

                RoleplayManager.SendUser(User.GetClient(), House.Sign.RoomId, "The house was just bought, so you were sent back outside!");
            }

        }
    }
}
