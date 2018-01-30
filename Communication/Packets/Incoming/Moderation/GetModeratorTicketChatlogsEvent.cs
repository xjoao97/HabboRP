using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Support;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    class GetModeratorTicketChatlogsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_tickets"))
                return;

            SupportTicket Ticket = PlusEnvironment.GetGame().GetModerationTool().GetTicket(Packet.PopInt());
            if (Ticket == null)
                return;

            RoomData Data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Ticket.RoomId);
            if (Data == null)
                return;

            Session.SendMessage(new ModeratorTicketChatlogComposer(Ticket, Data, Ticket.Timestamp));
        }
    }
}
