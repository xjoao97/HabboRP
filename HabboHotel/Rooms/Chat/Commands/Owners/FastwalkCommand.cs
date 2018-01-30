using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class FastwalkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_fast_walk"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Dá a capacidade de andar muito rápido."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            User.FastWalking = !User.FastWalking;

            if (User.SuperFastWalking)
                User.SuperFastWalking = false;

            Session.SendWhisper("Modo de caminhada atualizado.", 1);
        }
    }
}
