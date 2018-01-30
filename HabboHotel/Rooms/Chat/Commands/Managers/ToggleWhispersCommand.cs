using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class ToggleWhispersCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_toggle_whispers"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite que você ignore todos os sussurros na sala, exceto o seu próprio."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().IgnorePublicWhispers = !Session.GetHabbo().IgnorePublicWhispers;
            Session.SendWhisper("Você agora " + (Session.GetHabbo().IgnorePublicWhispers ? "está" : "não está mais") + " ignorando sussurros!", 1);
        }
    }
}
