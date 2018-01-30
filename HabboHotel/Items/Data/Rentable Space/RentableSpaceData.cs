using System;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Farming;

namespace Plus.HabboHotel.Items.Data.RentableSpace
{
    public class RentableSpaceData
    {
        public int ItemId;
        public int OwnerId;
        public int TimeLeft;
        public bool Enabled;
        public House House;
        public FarmingSpace FarmingSpace;

        public RentableSpaceData(int Item)
        {
            ItemId = Item;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_items_rentable_space` WHERE `item_id` = '" + ItemId + "' LIMIT 1");
                DataRow Row = dbClient.getRow();

                if (Row == null)
                {
                    dbClient.RunQuery("INSERT INTO `room_items_rentable_space` VALUES ('" + ItemId + "','0','0','0')");
                    dbClient.SetQuery("SELECT * FROM `room_items_rentable_space` WHERE `item_id` = '" + ItemId + "' LIMIT 1");
                    Row = dbClient.getRow();
                }

                this.OwnerId = Convert.ToInt32(Row["owner_id"]);
                this.TimeLeft = Convert.ToInt32(Row["time_left"]);
                this.Enabled = PlusEnvironment.EnumToBool(Row["enabled"].ToString());
                this.House = null;
                this.FarmingSpace = null;
            }
        }

        public RentableSpaceData(House House, int ItemId)
        {
            this.OwnerId = House.OwnerId;
            this.Enabled = !House.ForSale;
            this.ItemId = ItemId;
            this.House = House;
            this.FarmingSpace = null;
            this.TimeLeft = (30 * 24 * 60 * 60);
        }

        public RentableSpaceData(FarmingSpace FarmingSpace, int ItemId)
        {
            this.OwnerId = FarmingSpace.OwnerId;
            this.Enabled = FarmingSpace.OwnerId == 0 ? false : true;
            this.ItemId = ItemId;
            this.House = null;
            this.FarmingSpace = FarmingSpace;
            this.TimeLeft = FarmingSpace.Expiration;
        }

        public void UpdateData()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `room_items_rentable_space` SET `owner_id` = @ownerid, `time_left` = @timeleft, `enabled` = @enabled WHERE `item_id` = @itemid");
                dbClient.AddParameter("itemid", this.ItemId);
                dbClient.AddParameter("ownerid", this.OwnerId);
                dbClient.AddParameter("timeleft", this.TimeLeft);
                dbClient.AddParameter("enabled", PlusEnvironment.BoolToEnum(this.Enabled));
                dbClient.RunQuery();
            }
        }
    }
}