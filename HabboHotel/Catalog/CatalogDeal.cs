using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Catalog
{
    public class CatalogDeal
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public List<CatalogItem> ItemDataList { get; private set; }
        public string DisplayName { get; set; }
        public int CostCredits { get; set; }
        public int CostPixels { get; set; }

        public CatalogDeal(int Id, int PageId, string Items, string DisplayName, int Credits, int Pixels, ItemDataManager ItemDataManager)
        {
            this.Id = Id;
            this.PageId = PageId;
            this.DisplayName = DisplayName;
            this.ItemDataList = new List<CatalogItem>();

            string[] SplitItems = Items.Split(';');
            foreach (string Split in SplitItems)
            {
                string[] Item = Split.Split('*');
                int ItemId = 0;
                int Amount = 0;
                if (!int.TryParse(Item[0], out ItemId) || !int.TryParse(Item[1], out Amount))
                    continue;

                ItemData Data = null;
                if (!ItemDataManager.GetItem(ItemId, out Data))
                    continue;

                ItemDataList.Add(new CatalogItem(0, ItemId, Data, string.Empty, PageId, 0, 0, 0, 0, 0, 0, false, "", "", 0));
            }

            this.CostCredits = Credits;
            this.CostPixels = Pixels;
        }
    }
}
