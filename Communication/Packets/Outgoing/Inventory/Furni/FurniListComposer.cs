using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items.Crafting;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Catalog.Utilities;

namespace Plus.Communication.Packets.Outgoing.Inventory.Furni
{
    class FurniListComposer : ServerPacket
    {
        public FurniListComposer(List<Item> Items, ICollection<Item> Walls, bool CraftingCheck)
            : base(ServerPacketHeader.FurniListMessageComposer)
        {
            int CraftableItemCount = 0;

            base.WriteInteger(1);
            base.WriteInteger(1);

            if (CraftingCheck == false)
                base.WriteInteger(Items.Count + Walls.Count);
            else
            {
                foreach (var Item in Items.ToList())
                {
                    if (CraftingManager.isCraftingItem(Item.GetBaseItem().ItemName))
                        CraftableItemCount++;
                }

                foreach (var Item in Walls.ToList())
                {
                    if (CraftingManager.isCraftingItem(Item.GetBaseItem().ItemName))
                        CraftableItemCount++;
                }
                base.WriteInteger(Items.Count + Walls.Count - CraftableItemCount);
            }

            foreach (Item Item in Items.ToList())
            {
                if (CraftingCheck)
                {
                    if (!CraftingManager.isCraftingItem(Item.GetBaseItem().ItemName))
                        WriteItem(Item);
                }
                else
                    WriteItem(Item);
            }

            foreach (Item Item in Walls.ToList())
            {
                if (CraftingCheck)
                {
                    if (!CraftingManager.isCraftingItem(Item.GetBaseItem().ItemName))
                        WriteItem(Item);
                }
                else
                    WriteItem(Item);
            }
        }

        private void WriteItem(Item Item)
        {
            base.WriteInteger(Item.Id);
            base.WriteString(Item.GetBaseItem().Type.ToString().ToUpper());
            base.WriteInteger(Item.Id);
            base.WriteInteger(Item.GetBaseItem().SpriteId);

            if (Item.LimitedNo > 0)
            {
                base.WriteInteger(1);
                base.WriteInteger(256);
                base.WriteString(Item.ExtraData);
                base.WriteInteger(Item.LimitedNo);
                base.WriteInteger(Item.LimitedTot);
            }
            else
                ItemBehaviourUtility.GenerateExtradata(Item, this);

            base.WriteBoolean(Item.GetBaseItem().AllowEcotronRecycle);
            base.WriteBoolean(Item.GetBaseItem().AllowTrade);
            base.WriteBoolean(Item.LimitedNo == 0 ? Item.GetBaseItem().AllowInventoryStack : false);
            base.WriteBoolean(ItemUtility.IsRare(Item));
            base.WriteInteger(-1);//Seconds to expiration.
            base.WriteBoolean(true);
            base.WriteInteger(-1);//Item RoomId

            if (!Item.IsWallItem)
            {
                base.WriteString(string.Empty);
                base.WriteInteger(0);
            }
        }
    }
}