using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class SuperFastwalkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_fast_walk_super"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Dá-lhe a capacidade de andar muito, muito rápido."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            User.SuperFastWalking = !User.SuperFastWalking;

            if (User.FastWalking)
                User.FastWalking = false;

            Session.SendWhisper("Modo de caminhada atualizado.", 1);
        }
    }
}
