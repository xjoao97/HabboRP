using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class UnmuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mute_undo"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Desmuta um usuário atualmente silencioso."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja desmutar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (TargetClient.GetHabbo().TimeMuted <= 0)
            {
                Session.SendWhisper("Esta pessoa não está mutada!", 1);
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `time_muted` = '0' WHERE `id` = '" + TargetClient.GetHabbo().Id + "' LIMIT 1");
            }

            TargetClient.GetHabbo().TimeMuted = 0;
            TargetClient.SendNotification("Você foi desmutado por " + Session.GetHabbo().Username + "!");

            Session.Shout("*Desmuta imediatamente " + TargetClient.GetHabbo().Username + "*", 23);
            Session.SendWhisper("Você desmutou com sucesso " + TargetClient.GetHabbo().Username + "!", 1);
        }
    }
}