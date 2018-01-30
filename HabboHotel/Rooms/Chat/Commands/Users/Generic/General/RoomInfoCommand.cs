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
    class RoomInfoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_room_info"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Diz-lhe o id do quarto e outras informações sobre a sala."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder room = new StringBuilder();
            Room RoomInfo = Session.GetHabbo().CurrentRoom;

            if (RoomInfo == null)
                return;

            var RoomUsers = RoomInfo.GetRoomUserManager().GetUserList().Where(x => !x.IsBot && x.GetClient() != null && x.GetClient().GetRoleplay() != null).ToList();

            room.Append("====================\nInformação do quarto: " + RoomInfo.RoomData.Name + " (ID: " + RoomInfo.RoomData.Id + ")\n====================\n\n");
            room.Append("Quarto ID: " + RoomInfo.RoomData.Id + "\n");
            room.Append("Nome do Quarto: " + RoomInfo.RoomData.Name + "\n");
            room.Append("Dono do Quarto: " + RoomInfo.RoomData.OwnerName + " (ID: " + RoomInfo.RoomData.OwnerId + ")\n");
            room.Append("Usuários atualmente: " + (Session.GetHabbo().GetPermissions().HasRight("mod_tool") ? String.Format("{0:N0}", RoomUsers.Count) : String.Format("{0:N0}", RoomUsers.Where(x => !x.GetClient().GetRoleplay().Invisible).ToList().Count)) + "\n\n");
            room.Append("Usuários no Quarto:\n");

            foreach (RoomUser user in RoomUsers)
            {
                if (user == null)
                    continue;
                if (user.GetClient() == null)
                    continue;
                if (user.GetClient().GetHabbo() == null)
                    continue;
                if (user.GetClient().GetRoleplay() == null)
                    continue;
                if (user.GetClient().GetRoleplay().Invisible && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    continue;

                room.Append(user.GetClient().GetHabbo().Username + "\n");
            }

            Session.SendMessage(new MOTDNotificationComposer(room.ToString()));
        }
    }
}