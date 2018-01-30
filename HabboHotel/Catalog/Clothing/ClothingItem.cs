using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Catalog.Clothing
{
    public class ClothingItem
    {
        public int Id { get; set; }
        public string ClothingName { get; set; }
        public List<int> PartIds { get; private set; }
        public int Cost { get; set; }

        public ClothingItem(int Id, string ClothingName, string PartIds, int Cost)
        {
            this.Id = Id;
            this.ClothingName = ClothingName;

            this.PartIds = new List<int>();
            this.Cost = Cost;
            if (PartIds.Contains(","))
            {
                foreach (string PartId in PartIds.Split(','))
                {
                    this.PartIds.Add(int.Parse(PartId));
                }
            }
            else if (!String.IsNullOrEmpty(PartIds) && (int.Parse(PartIds)) > 0)
                this.PartIds.Add(int.Parse(PartIds));
        }
    }
}
