using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Support;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    class ModeratorSupportTicketComposer : ServerPacket
    {
        public ModeratorSupportTicketComposer(SupportTicket Ticket)
            : base(ServerPacketHeader.ModeratorSupportTicketMessageComposer)
        {
            base.WriteInteger(Ticket.Id);
            base.WriteInteger(Ticket.TabId);
            base.WriteInteger(1); // Type
            base.WriteInteger(Ticket.Category); // Category
            base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - (int)Ticket.Timestamp) * 1000);
            base.WriteInteger(Ticket.Score);
            base.WriteInteger(0);
            base.WriteInteger(Ticket.SenderId);
           base.WriteString(Ticket.SenderName);
            base.WriteInteger(Ticket.ReportedId);
           base.WriteString(Ticket.ReportedName);
            base.WriteInteger((Ticket.Status == TicketStatus.PICKED) ? Ticket.ModeratorId : 0);
           base.WriteString(Ticket.ModName);
           base.WriteString(Ticket.Message);
            base.WriteInteger(0);//No idea?
            base.WriteInteger(0);//String, int, int - this is the "matched to" a string
            {
               base.WriteString("fresh-hotel.org");
                base.WriteInteger(-1);
                base.WriteInteger(-1);

            }
        }
    }
}