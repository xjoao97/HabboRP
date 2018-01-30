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
    class RoomIDCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_room_id"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Diz-lhe o id do quarto em que você está."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Room == null)
            {
                Session.SendWhisper("Por algum motivo estranho, os dados desta sala não foram encontrados!", 1);
                return;
            }

            Session.SendWhisper("Você está atualmente no [Quarto ID: " + Room.Id + "]!", 1);
            return;
        }
    }
}