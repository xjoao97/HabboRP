using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class HtmlUIPageCommand : IChatCommand
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
            get { return "Redireciona um usuário pelo seu ID"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Digite o nome do usuário seguido de uma página para enviar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(Params[1]));

            string Data = "action:broadcast,page:" + Params[2].ToLower();
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_htmlpage", Data);
            Session.SendWhisper("Página Aberta: '" + Params[2].ToLower() + "' para " + TargetClient.GetHabbo().Username, 1);
            return;
        }
    }
}
