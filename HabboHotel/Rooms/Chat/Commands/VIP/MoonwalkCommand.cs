using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.VIP
{
    class MoonwalkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_moonwalk"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Andar como Michael Jackson."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            User.moonwalkEnabled = !User.moonwalkEnabled;

            if (User.moonwalkEnabled)
                Session.SendWhisper("Moonwalk ativado!", 1);
            else
                Session.SendWhisper("Moonwalk desativado!", 1);
        }
    }
}
