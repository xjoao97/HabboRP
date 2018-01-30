using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class LogOutCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_log_out"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Sai para fora do RP."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.Disconnect(false);
        }
    }
}
