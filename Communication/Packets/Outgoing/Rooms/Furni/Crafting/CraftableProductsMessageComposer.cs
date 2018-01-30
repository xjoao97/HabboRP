using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items.Crafting;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting
{
    class CraftableProductsMessageComposer : ServerPacket
    {
        public CraftableProductsMessageComposer(HabboHotel.GameClients.GameClient Session)
            : base(ServerPacketHeader.CraftableProductsMessageComposer)
        {
            base.WriteInteger(CraftingManager.NotSecretCraftingRecipes.Count + Session.GetHabbo().UnlockedRecipes.Count);

            foreach (var recipe in CraftingManager.NotSecretCraftingRecipes.Values)
            {
                base.WriteString(recipe.Result);
                base.WriteString(recipe.Result);
            }

            foreach (var recipe in Session.GetHabbo().UnlockedRecipes)
            {
                base.WriteString(recipe.Result);
                base.WriteString(recipe.Result);
            }

            base.WriteInteger(CraftingManager.CraftableItems.Count);

            foreach (var itemName in CraftingManager.CraftableItems)
            {
                base.WriteString(itemName);
            }
        }
    }
}
