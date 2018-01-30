using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class TeleportCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_teleport"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "A capacidade de se teletransportar em qualquer lugar da sala."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            User.TeleportEnabled = !User.TeleportEnabled;
            Room.GetGameMap().GenerateMaps();
            Session.SendWhisper("Modo Teleporte atualizado.", 1);
        }
    }
}
