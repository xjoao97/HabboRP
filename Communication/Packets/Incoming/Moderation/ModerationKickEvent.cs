using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Support;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    class ModerationKickEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("mod_kick"))
                return;

            int UserId = Packet.PopInt();
            string Message = Packet.PopString();

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().CurrentRoomId < 1 || Client.GetHabbo().Id == Session.GetHabbo().Id)
                return;

            if (Client.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("moderation_kick_permissions"));
                return;
            }

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;
            
            Room.GetRoomUserManager().RemoveUserFromRoom(Client, true, false);
        }
    }
}
