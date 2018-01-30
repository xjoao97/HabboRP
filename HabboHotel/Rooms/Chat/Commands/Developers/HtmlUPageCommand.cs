using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class HtmlUPageCommand : IChatCommand
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
            get { return "Abre uma página para o usuário por sua identificação ( parar sumo )"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Digite o nome do usuário seguido de uma página para enviar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetRoleplay() == null)
            {
                Session.SendWhisper("Não foi possível encontrar esse usuário!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().WebSocketConnection == null)
            {
                Session.SendWhisper("Não podemos alertar esse usuário neste momento! Seu WebSocket está desligado!", 1);
                return;
            }

            string Data = "action:broadcast,page:" + Params[2].ToLower();
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_htmlpage", Data);
            Session.SendWhisper("Página Aberta: '" + Params[2].ToLower() + "' para " + TargetClient.GetHabbo().Username, 1);
            return;
        }
    }
}
