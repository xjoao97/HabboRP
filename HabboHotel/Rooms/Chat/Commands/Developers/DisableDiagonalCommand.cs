using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.Communication.Packets.Outgoing.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class DisableDiagonalCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_disable_diagonal"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desativa a diagonal na sala atual."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Room.GetGameMap().DiagonalEnabled = !Room.GetGameMap().DiagonalEnabled;
            Session.SendWhisper("Atualizou com sucesso a diagonal para esta sala.");
            Session.Shout("*Desativa a diagonal para este quarto*", 23);
        }
    }
}
