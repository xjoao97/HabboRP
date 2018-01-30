using System;
using Plus.HabboHotel.Items.Crafting;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting
{
    class CraftingRecipeMessageComposer : ServerPacket
    {
        public CraftingRecipeMessageComposer(CraftingRecipe recipe)
            : base(ServerPacketHeader.CraftingRecipeMessageComposer)
        {
            base.WriteInteger(recipe.ItemsNeeded.Count); // Count of different items

            foreach (var item in recipe.ItemsNeeded)
            {
                base.WriteInteger(item.Value); // How many of the item
                base.WriteString(item.Key); // Item name
            }
        }
    }
}
