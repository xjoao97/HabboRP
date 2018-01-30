using System;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Items;

namespace Plus.HabboRoleplay.Farming
{
    public class FarmingItem
    {
        public int Id;
        public string BaseItem;
        public int LevelRequired;
        public int MinExp;
        public int MaxExp;
        public int SellPrice;
        public int BuyPrice;

        public FarmingItem(int Id, string BaseItem, int LevelRequired, int MinExp, int MaxExp, int SellPrice, int BuyPrice)
        {
            this.Id = Id;
            this.BaseItem = BaseItem;
            this.LevelRequired = LevelRequired;
            this.MinExp = MinExp;
            this.MaxExp = MaxExp;
            this.SellPrice = SellPrice;
            this.BuyPrice = BuyPrice;
        }
    }
}