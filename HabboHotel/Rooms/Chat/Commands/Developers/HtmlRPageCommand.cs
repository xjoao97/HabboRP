using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class HtmlRPageCommand : IChatCommand
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
            get { return "Abre a página para o quarto"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite a página que deseja abrir para todo o hotel!", 1);
                return;
            }

            if (Room != null && Room.GetRoomUserManager() != null && Room.GetRoomUserManager().GetRoomUsers() != null)
            {
                lock (Room.GetRoomUserManager().GetRoomUsers().ToList())
                {
                    foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers().ToList())
                    {
                        if (User == null)
                            continue;

                        if (User.IsBot)
                            continue;

                        if (User.GetClient() == null)
                            continue;

                        if (User.GetClient().LoggingOut)
                            continue;

                        if (User.GetClient().GetRoleplay() == null)
                            continue;

                        if (User.GetClient().GetRoleplay().WebSocketConnection == null)
                            continue;

                        string Data = "action:broadcast,page:" + Params[1].ToLower();
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User.GetClient(), "event_htmlpage", Data);
                    }
                    Session.SendWhisper("Enviado com sucesso a página " + Params[1].ToLower() + " para o quarto!", 1);
                    return;
                }
            }
            else
            {
                Session.SendWhisper("Ocorreu um erro! ", 1);
                return;
            }
        }
    }
}
