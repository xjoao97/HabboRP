using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Catalog.Clothing
{
    public class ClothingManager
    {
        private Dictionary<int, ClothingItem> _clothing;

        public ClothingManager()
        {
            this._clothing = new Dictionary<int, ClothingItem>();
           
            this.Init();
        }

        public void Init()
        {
            if (this._clothing.Count > 0)
                this._clothing.Clear();

            DataTable GetClothing = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`clothing_name`,`clothing_parts`,`cost` FROM `catalog_clothing`");
                GetClothing = dbClient.getTable();
            }

            if (GetClothing != null)
            {
                foreach (DataRow Row in GetClothing.Rows)
                {
                    this._clothing.Add(Convert.ToInt32(Row["id"]), new ClothingItem(Convert.ToInt32(Row["id"]), Convert.ToString(Row["clothing_name"]), Convert.ToString(Row["clothing_parts"]), Convert.ToInt32(Row["cost"])));
                }
            }
        }

        public bool TryGetClothing(int ItemId, out ClothingItem Clothing)
        {
            if (this._clothing.TryGetValue(ItemId, out Clothing))
                return true;
            return false;
        }
    }
}
