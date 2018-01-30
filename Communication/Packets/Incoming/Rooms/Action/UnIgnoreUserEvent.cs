using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Action;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    class UnIgnoreUserEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            String Username = Packet.PopString();

            Habbo User = PlusEnvironment.GetHabboByUsername(Username);
            if (User == null || !Session.GetHabbo().MutedUsers.Contains(User.Id))
                return;

            Session.GetHabbo().MutedUsers.Remove(User.Id);
            Session.SendMessage(new IgnoreStatusComposer(3, Username));
        }
    }
}
