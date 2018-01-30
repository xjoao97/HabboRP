using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    class UnbanUserFromRoomEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Instance = Session.GetHabbo().CurrentRoom;
            if (Instance == null || !Instance.CheckRights(Session, true))
                return;

            int UserId = Packet.PopInt();
            int RoomId = Packet.PopInt();

            if (Instance.BannedUsers().Contains(UserId))
            {
                Instance.Unban(UserId);
                Session.SendMessage(new UnbanUserFromRoomComposer(RoomId, UserId));
            }
        }
    }
}