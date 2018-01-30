using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Rooms.Engine
{
    class ObjectsComposer : ServerPacket
    {
        public ObjectsComposer(GameClient Session, Item[] Objects, Room Room)
            : base(ServerPacketHeader.ObjectsMessageComposer)
        {
            base.WriteInteger(1);

            base.WriteInteger(Room.OwnerId);
            base.WriteString(Room.OwnerName);

            base.WriteInteger(Objects.Length);
            foreach (Item Item in Objects)
            {
                if (Item.GetBaseItem().InteractionType == InteractionType.PURCHASABLE_CLOTHING || Item.GetBaseItem().InteractionType == InteractionType.CRAFTING || Item.GetBaseItem().ItemName.ToLower() == "fxbox_fx192")
                    WriteFloorItem(Item, Session.GetHabbo().Id);
                else
                {
                    WriteFloorItem(Item, Convert.ToInt32(Item.UserID));
                }
            }
        }

        private void WriteFloorItem(Item Item, int UserID)
        {
            base.WriteInteger(Item.Id);
            base.WriteInteger(Item.GetBaseItem().SpriteId);
            base.WriteInteger(Item.GetX);
            base.WriteInteger(Item.GetY);
            base.WriteInteger(Item.Rotation);
            base.WriteString(String.Format("{0:0.00}", TextHandling.GetString(Item.GetZ)));
            base.WriteString(String.Empty);

            if (Item.LimitedNo > 0)
            {
                base.WriteInteger(1);
                base.WriteInteger(256);
                base.WriteString(Item.ExtraData);
                base.WriteInteger(Item.LimitedNo);
                base.WriteInteger(Item.LimitedTot);
            }
            else
            {
                ItemBehaviourUtility.GenerateExtradata(Item, this);
            }

            base.WriteInteger(-1); // to-do: check
            base.WriteInteger((Item.GetBaseItem().Modes > 1) ? 2 : 0);
            base.WriteInteger(UserID);
        }
    }
}