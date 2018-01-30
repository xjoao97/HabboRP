using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class GetGroupCreationWindowEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null)
                return;

            List<RoomData> ValidRooms = new List<RoomData>();
            RoomData RoomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(1);

            ValidRooms.Add(RoomData);

            Session.SendMessage(new GroupCreationWindowComposer(ValidRooms));
        }
    }
}
