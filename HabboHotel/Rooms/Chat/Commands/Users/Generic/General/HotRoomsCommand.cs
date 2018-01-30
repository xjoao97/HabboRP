using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class HotRoomsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_hot_rooms"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Diz-lhe todas as salas movimentadas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Rooms = new StringBuilder();

            foreach (Room room in PlusEnvironment.GetGame().GetRoomManager().GetRooms().ToList().OrderByDescending(key => key.UserCount))
            {
                if (room.UserCount <= 0)
                    continue;

                Rooms.Append("[ID: " + room.RoomId + "] - [" + room.RoomData.Name + "] - Usuários: " + room.UserCount + "\n");
            }

            Session.SendMessage(new MOTDNotificationComposer(Rooms.ToString()));
        }
    }
}