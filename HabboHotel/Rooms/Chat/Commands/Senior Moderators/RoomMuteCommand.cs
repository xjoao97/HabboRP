using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class RoomMuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mute_room"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Mutes the room."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Room.RoomMuted == false)
            {
                Room.RoomMuted = true;

                string Msg = CommandManager.MergeParams(Params, 1);
                Session.Shout("*Muta imediatamente todos do quarto*", 23);
            }
            else
            {
                Session.SendWhisper("Este quarto já está mutado!", 1);
            }
        }
    }
}
