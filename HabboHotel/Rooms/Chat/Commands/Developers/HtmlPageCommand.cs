using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class HtmlPageCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_open_div"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Abre uma página para o hotel "; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor, insira a página que deseja abrir para todo o hotel!", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().BroadCastWebEvent("event_htmlpage", "action:broadcast,page:" + Params[1].ToLower());
            Session.SendWhisper("Enviado com sucesso a página " + Params[1].ToLower() + " para todo o hotel!!", 1);
            return;
        }
    }
}
