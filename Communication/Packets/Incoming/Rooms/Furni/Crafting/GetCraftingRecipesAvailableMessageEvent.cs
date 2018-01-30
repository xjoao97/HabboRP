using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Crafting;
using Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.Crafting
{
    class GetCraftingRecipesAvailableMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int CraftingTable = Packet.PopInt();
            int Limit = Packet.PopInt();
            bool isrecipe = false;

            ConcurrentDictionary<string, int> PlacedItems = new ConcurrentDictionary<string, int>();

            for (var i = 0; i < Limit; i++)
            {
                int ItemId = Packet.PopInt();

                Item Item = Session.GetHabbo().GetInventoryComponent().GetItem(ItemId);

                if (Item == null)
                    continue;

                if (!PlacedItems.ContainsKey(Item.GetBaseItem().ItemName))
                    PlacedItems.TryAdd(Item.GetBaseItem().ItemName, 1);
                else
                    PlacedItems.TryUpdate(Item.GetBaseItem().ItemName, PlacedItems[Item.GetBaseItem().ItemName] + 1, PlacedItems[Item.GetBaseItem().ItemName]);
            }

            int RecipeCount = 0;

            foreach (var recipe in CraftingManager.AllCraftingRecipes.Values)
            {
                if (PlacedItems.OrderBy(r => r.Key).SequenceEqual(recipe.ItemsNeeded.OrderBy(r => r.Key)))
                    isrecipe = true;

                if (!DiffDictionary(recipe.ItemsNeeded, PlacedItems).ContainsValue("incorrect"))
                    RecipeCount++;
            }

            Session.SendMessage(new CraftSecretFoundMessageComposer(isrecipe, isrecipe == true ? RecipeCount - 1 : RecipeCount));
        }

        internal Dictionary<string, string> DiffDictionary(Dictionary<string, int> first, ConcurrentDictionary<string, int> second)
        {
            var diff = first.ToDictionary(e => e.Key, e => "Faltando um " + e.Key + " para esta receita");
            foreach (var other in second)
            {
                int firstValue;
                if (first.TryGetValue(other.Key, out firstValue))
                    diff[other.Key] = other.Value <= firstValue ? "correct" : "incorrect";
                else
                    diff[other.Key] = "incorrect";
            }
            return diff;
        }
    }
}
