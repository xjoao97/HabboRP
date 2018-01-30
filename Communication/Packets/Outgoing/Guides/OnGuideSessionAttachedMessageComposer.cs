using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class OnGuideSessionAttachedComposer : ServerPacket
    {
        public OnGuideSessionAttachedComposer(int userid, string message, int type)
            : base(ServerPacketHeader.OnGuideSessionAttachedComposer)
        {
            if (type == 30)
                base.WriteBoolean(false);
            else
                base.WriteBoolean(true);
            base.WriteInteger(userid);
            base.WriteString(message);
            base.WriteInteger(type);
        }
    }
}