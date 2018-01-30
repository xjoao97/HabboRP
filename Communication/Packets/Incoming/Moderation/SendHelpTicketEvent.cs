using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Action;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    class SendHelpTicketEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int UserId = Packet.PopInt();

            // Ignore the user who was just reported
            if (PlusEnvironment.GetHabboById(UserId) != null)
                Session.SendMessage(new IgnoreStatusComposer(1, PlusEnvironment.GetHabboById(UserId).Username));
        }
    }
}