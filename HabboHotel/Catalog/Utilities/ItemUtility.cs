using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Utilities;

namespace Plus.HabboHotel.Catalog.Utilities
{
    public static class ItemUtility
    {
        public static bool CanGiftItem(CatalogItem Item)
        {
            if (!Item.Data.AllowGift || Item.IsLimited || Item.Amount > 1 || Item.Data.ItemName.ToLower().StartsWith("cf_") || Item.Data.ItemName.ToLower().StartsWith("cfc_") ||
                Item.Data.InteractionType == InteractionType.BADGE || (Item.Data.Type != 's' && Item.Data.Type != 'i') || Item.CostDiamonds > 0 ||
                Item.Data.InteractionType == InteractionType.TELEPORT || Item.Data.InteractionType == InteractionType.DEAL)
                return false;

            if (Item.Data.IsRare)
                return false;

            if (PetUtility.IsPet(Item.Data.InteractionType))
                return false;
            return true;
        }

        public static bool CanSelectAmount(CatalogItem Item)
        {
            if (Item.IsLimited || Item.Amount > 1 || Item.Data.ItemName.ToLower().StartsWith("cf_") || Item.Data.ItemName.ToLower().StartsWith("cfc_") || !Item.HaveOffer || Item.Data.InteractionType == InteractionType.BADGE || Item.Data.InteractionType == InteractionType.DEAL)
                return false;
            return true;
        }

        public static int GetSaddleId(int Saddle)
        {
            switch (Saddle)
            {
                default:
                case 9:
                    return 2623; //4221 Changed to the right BaseItem, so it can be saved.
                case 10:
                    return 2844;
            }
        }

        public static bool IsRare(Item Item)
        {
            if (Item.LimitedNo > 0)
                return true;

            if (Item.Data.IsRare)
                return true;

            return false;
        }
    }
}
