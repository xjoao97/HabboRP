using System;
using System.Linq;
using System.Text;
using System.Drawing;
using Plus.Utilities;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Criminal
{
    class TcomandosCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_tcomandos"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista de todos os comandos de trabalhos do HabboRPG."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.SendMessage(new InternalLinkComposer("habbopages/roleplay/trabalhos.txt?" + UnixTimestamp.GetNow()));
        }
    }
}