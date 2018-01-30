using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class AlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_alert"; }
        }

        public string Parameters
        {
            get { return "%usuário% %mensagem%"; }
        }

        public string Description
        {
            get { return "Alerta um usuário com uma mensagem de sua escolha."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja alertar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Você não pode se alertar!", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 2);

            TargetClient.SendNotification(Session.GetHabbo().Username + " alertou você com a seguinte mensagem:\n\n" + Message);
            Session.SendWhisper("Alerta enviado com sucesso para " + TargetClient.GetHabbo().Username + ".", 1);

        }
    }
}
