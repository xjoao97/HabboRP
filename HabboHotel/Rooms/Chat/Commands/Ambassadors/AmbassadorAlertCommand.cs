using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Ambassadors
{
    class AmbassadorAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_alert_ambassador"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envia uma mensagem digitada por você para os embaixadores online atuais."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para enviar.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);

            PlusEnvironment.GetGame().GetClientManager().AmbassadorWhisperAlert(Message, Session);
        }
    }
}
