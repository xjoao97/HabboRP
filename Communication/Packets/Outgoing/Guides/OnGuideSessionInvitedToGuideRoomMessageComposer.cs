using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Guides
{
    class OnGuideSessionInvitedToGuideRoomComposer : ServerPacket
    {
        public OnGuideSessionInvitedToGuideRoomComposer(GameClient Session)
            : base(ServerPacketHeader.OnGuideSessionInvitedToGuideRoomComposer)
        {
            Room room = Session.GetHabbo().CurrentRoom;

            if (room == null)
            {
                base.WriteInteger(0);
                base.WriteString(string.Empty);
            }
            else
            {
                base.WriteInteger(room.RoomId);
                base.WriteString(room.RoomData.Name);
            }
        }
    }
}