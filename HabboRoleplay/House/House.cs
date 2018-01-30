using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Houses
{
    public class House
    {
        public int RoomId;
        public int OwnerId;
        public int Cost;
        public bool ForSale;
        public string[] Upgrades;
        public bool IsLocked;
        public HouseSign Sign;

        public House(int RoomId, int OwnerId, int Cost, bool ForSale, string[] Upgrades, bool IsLocked, HouseSign Sign)
        {
            this.RoomId = RoomId;
            this.OwnerId = OwnerId;
            this.Cost = Cost;
            this.ForSale = ForSale;
            this.Upgrades = Upgrades;
            this.IsLocked = IsLocked;
            this.Sign = Sign;
        }

        public void SpawnSign()
        {
            Room Room = RoleplayManager.GenerateRoom(this.Sign.RoomId, false);
            if (Room != null && Sign.Item != null)
            {
                if (Room.GetRoomItemHandler().GetFloor.Where(x => x.Id == this.Sign.Item.Id).ToList().Count > 0)
                {
                    foreach (Item Item in Room.GetRoomItemHandler().GetFloor.Where(x => x.Id == this.Sign.Item.Id).ToList())
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                    }
                }
            }

            if (Room != null)
            {
                this.Sign.Item = RoleplayManager.PlaceItemToRoom(null, 5224, 0, this.Sign.X, this.Sign.Y, this.Sign.Z, 0, false, this.Sign.RoomId, false, "0", false, "", this);
                this.Sign.Spawned = true;
            }
        }

        public void UpdateCost(int Cost)
        {
            this.Cost = Cost;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rp_houses` SET `cost` = @cost WHERE `owner_id` = @owner");
                dbClient.AddParameter("owner", this.OwnerId);
                dbClient.AddParameter("cost", this.Cost);
                dbClient.RunQuery();
            }
        }

        public void BuyHouse(GameClient Session, int Cost)
        {
            if (!this.ForSale)
                return;

            if (this.OwnerId != 0 && Session.GetHabbo().Id != this.OwnerId)
            {
                GameClient Owner = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(this.OwnerId);

                if (Owner != null && Owner.GetHabbo() != null)
                {
                    Owner.GetHabbo().Credits += Cost;
                    Owner.GetHabbo().UpdateCreditsBalance();
                    Owner.SendNotification("Sua casa foi vendida para  " + Session.GetHabbo().Username + " por R$" + Cost + "!");
                }
                else
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `users` SET `credits` = (credits + @prize) WHERE `id` = @winner LIMIT 1");
                        dbClient.AddParameter("prize", Cost);
                        dbClient.AddParameter("winner", this.OwnerId);
                        dbClient.RunQuery();
                    }
                }
            }

            this.OwnerId = Session.GetHabbo().Id;
            this.ForSale = false;
            this.Cost = 5000;
            
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rp_houses` SET `owner_id` = @owner, `for_sale` = @forsale, `cost` = @cost WHERE `room_id` = @room");
                dbClient.AddParameter("owner", this.OwnerId);
                dbClient.AddParameter("forsale", PlusEnvironment.BoolToEnum(this.ForSale));
                dbClient.AddParameter("cost", this.Cost);
                dbClient.AddParameter("room", this.RoomId);
                dbClient.RunQuery();
            } 
        }

        public void SellHouse(GameClient Session)
        {
            if (this.ForSale)
                return;

            if (this.OwnerId != Session.GetHabbo().Id)
                return;

            this.ForSale = true;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rp_houses` SET `for_sale` = @forsale WHERE `owner_id` = @owner");
                dbClient.AddParameter("forsale", PlusEnvironment.BoolToEnum(this.ForSale));
                dbClient.AddParameter("owner", this.OwnerId);
                dbClient.RunQuery();
            }
        }
    }
}
