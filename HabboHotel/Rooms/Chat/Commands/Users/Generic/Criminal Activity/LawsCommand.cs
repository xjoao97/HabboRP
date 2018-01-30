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
    class LawsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_criminal_activity_laws"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Mostra a lista de Leis da cidade do HabboRP."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.SendMessage(new InternalLinkComposer("habbopages/roleplay/laws.txt?" + UnixTimestamp.GetNow()));
        }
    }
}