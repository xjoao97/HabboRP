using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class NoticeHotelAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hotel_alert_notice"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envie uma mensagem para todo o hotel por sussurro."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para enviar.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);

            lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;

                    client.SendMessage(new RoomNotificationComposer("staff_notice", "message", "[Notícia da Equipe] " + Message + " - de " + Session.GetHabbo().Username));
                }
            }
        }
    }
}
