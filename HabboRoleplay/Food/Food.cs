using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Food
{
    public class Food
    {
        public string Name;
        public string Type;
        public int ItemId;
        public string ExtraData;
        public int Cost;
        public int Health;
        public int Energy;
        public int Hunger;
        public string ServeText;
        public string EatText;
        public bool Servable;

        public Food(string Name, string Type, int ItemId, string ExtraData, int Cost, int Health, int Energy, int Hunger, string ServeText, string EatText, bool Servable)
        {
            this.Name = Name;
            this.Type = Type;
            this.ItemId = ItemId;
            this.ExtraData = ExtraData;
            this.Cost = Cost;
            this.Health = Health;
            this.Energy = Energy;
            this.Hunger = Hunger;
            this.ServeText = ServeText;
            this.EatText = EatText;
            this.Servable = Servable;
        }
    }
}
