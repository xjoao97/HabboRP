using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class OverrideCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_override"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Dá-lhe a capacidade de caminhar sobre qualquer coisa."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            User.AllowOverride = !User.AllowOverride;
            Session.SendWhisper("Modo de Override atualizado.", 1);
        }
    }
}
