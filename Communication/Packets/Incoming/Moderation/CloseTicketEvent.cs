using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    class CloseTicketEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            int Result = Packet.PopInt(); // result, 1 = useless, 2 = abusive, 3 = resolved
            int Junk = Packet.PopInt(); // ? 
            int TicketId = Packet.PopInt(); // id

            PlusEnvironment.GetGame().GetModerationTool().CloseTicket(Session, TicketId, Result);
        }
    }
}