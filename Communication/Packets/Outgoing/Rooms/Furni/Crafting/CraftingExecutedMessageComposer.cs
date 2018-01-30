using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting
{
    class CraftingExecutedMessageComposer : ServerPacket
    {
        public CraftingExecutedMessageComposer(bool Success, string ItemName)
            : base(ServerPacketHeader.CraftingExecutedMessageComposer)
        {
            base.WriteBoolean(Success);
            base.WriteString(ItemName);
            base.WriteString(ItemName);
        }
    }
}
