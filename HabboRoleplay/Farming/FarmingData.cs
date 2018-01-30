using System;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Houses;

namespace Plus.HabboRoleplay.Farming
{
    public class FarmingData
    {
        public int ItemId;
        public int OwnerId;
        public bool BeingFarmed;

        public FarmingData(int Item)
        {
            this.ItemId = Item;
            this.OwnerId = 0;
            this.BeingFarmed = false;
        }
    }
}