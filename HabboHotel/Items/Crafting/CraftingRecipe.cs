using System;
using System.Collections.Generic;

namespace Plus.HabboHotel.Items.Crafting
{
    class CraftingRecipe
    {
        public Dictionary<string, int> ItemsNeeded;
        public string Result;
        public bool Secret;

        public CraftingRecipe(string itemsNeeded, string result, bool secret)
        {
            ItemsNeeded = new Dictionary<string, int>();
            var splitted = itemsNeeded.Split(',');

            foreach (var split in splitted)
            {
                var item = split.Split(':');
                if (item.Length != 2) continue;
                ItemsNeeded.Add(item[0], Convert.ToInt32(item[1]));
            }

            Result = result;
            Secret = secret;
        }
    }
}
