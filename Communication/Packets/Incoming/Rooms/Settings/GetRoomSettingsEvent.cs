using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Settings;

namespace Plus.Communication.Packets.Incoming.Rooms.Settings
{
    class GetRoomSettingsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int RoomId = Packet.PopInt();

            Room Room = HabboRoleplay.Misc.RoleplayManager.GenerateRoom(RoomId);
            if (Room == null || !Room.CheckRights(Session, true))
                return;

            Session.SendMessage(new RoomSettingsDataComposer(Room));
        }
    }
}
