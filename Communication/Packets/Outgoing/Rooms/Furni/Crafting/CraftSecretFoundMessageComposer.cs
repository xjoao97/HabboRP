using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items.Crafting;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Crafting
{
    class CraftSecretFoundMessageComposer : ServerPacket
    {
        public CraftSecretFoundMessageComposer(bool isrecipe, int count)
            : base(ServerPacketHeader.CraftSecretFoundMessageComposer)
        {
            base.WriteInteger(count);
            base.WriteBoolean(isrecipe);
        }
    }
}
