using System;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Data.RentableSpace;

namespace Plus.HabboRoleplay.Farming
{
    public class FarmingSpace
    {
        public int Id;
        public int ItemId;
        public int RoomId;
        public int Cost;
        public int X;
        public int Y;
        public double Z;
        public int OwnerId;
        public int Expiration;
        public bool Spawned;
        public Item Item;

        public FarmingSpace(int Id, int ItemId, int RoomId, int Cost, int X, int Y, double Z, int OwnerId, int Expiration)
        {
            this.Id = Id;
            this.ItemId = ItemId;
            this.RoomId = RoomId;
            this.Cost = Cost;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.OwnerId = OwnerId;
            this.Expiration = Expiration;
            this.Spawned = false;
            this.Item = null;
        }

        public void SpawnSign()
        {
            Room Room = RoleplayManager.GenerateRoom(this.RoomId, false);
            if (Room != null && this.Item != null)
            {
                if (Room.GetRoomItemHandler().GetFloor.Where(x => x.Id == this.Item.Id).ToList().Count > 0)
                {
                    foreach (Item Item in Room.GetRoomItemHandler().GetFloor.Where(x => x.Id == this.Item.Id).ToList())
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                    }
                }
            }

            if (Room != null)
            {
                this.Item = RoleplayManager.PlaceItemToRoom(null, this.ItemId, 0, this.X, this.Y, this.Z, 0, false, this.RoomId, false, "0", false, "", null, this);
                this.Spawned = true;
            }
        }

        public void BuySpace(GameClient Session, Item Item)
        {
            if (Session == null || Item == null)
                return;

            if (this.OwnerId > 0)
                return;

            this.OwnerId = Session.GetHabbo().Id;
            this.Expiration = 3600;

            Item.RentableSpaceData.Enabled = true;
            Item.RentableSpaceData.OwnerId = Session.GetHabbo().Id;
            Item.RentableSpaceData.TimeLeft = 3600;

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rp_farming_spaces` SET `expiration` = @expiration, `owner_id` = @owner WHERE `id` = @id");
                dbClient.AddParameter("owner", this.OwnerId);
                dbClient.AddParameter("expiration", this.Expiration);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }
        
        public void ExpireSpace()
        {
            GameClient Owner = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(this.OwnerId);

            if (Owner != null)
                Owner.SendNotification("O espaço agrícola que você comprou acabou de expirar!");

            this.OwnerId = 0;
            this.Expiration = 0;
            
            if (this.Item != null)
                this.Item.RentableSpaceData = new RentableSpaceData(this, this.Item.Id);
        }
    }
}