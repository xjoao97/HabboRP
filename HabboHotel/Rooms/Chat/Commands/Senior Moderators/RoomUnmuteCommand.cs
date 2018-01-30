using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class RoomUnmuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mute_room_undo"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desmuta o quarto."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Room.RoomMuted == true)
            {
                Room.RoomMuted = false;
                Session.Shout("*Desmuta imediatamente todos do quarto*", 23);
            }
            else
            {
                Session.SendWhisper("Este quarto não está mutado!", 1);
            }
        }
    }
}