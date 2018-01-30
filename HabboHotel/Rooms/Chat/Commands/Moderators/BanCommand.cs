using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class BanCommand : IChatCommand
    {

        public string PermissionRequired
        {
            get { return "command_ban"; }
        }

        public string Parameters
        {
            get { return "%usuário% %tempo% %razão% "; }
        }

        public string Description
        {
            get { return "Remova um quebrador de regras da cidade por um período fixo de tempo."; ; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja banir.", 1);
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário no banco de dados.", 1);
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_soft_ban") && !Session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                Session.SendWhisper("Opa, você não pode banir esse usuário.", 1);
                return;
            }

            Double Expire = 0;
            string Minutes = Params[2];

            if (Minutes == "perm")
                Expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            else
                Expire = (PlusEnvironment.GetUnixTimestamp() + (Convert.ToDouble(Minutes) * 60));

            string Reason = null;
            if (Params.Length >= 4)
                Reason = CommandManager.MergeParams(Params, 3);
            else
                Reason = "Sem razão específica, mas provavelmente fez merda.";

            string Username = Habbo.Username;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
            }

            PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Reason, Expire);

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient != null)
            {
                TargetClient.Disconnect(true);
            }
            int Hours = 0;
            int MinutesLeft = 0;

            if (Minutes != "perm")
            {
                Hours = Convert.ToInt32(Math.Floor(Convert.ToDouble(Minutes) / 60));
                MinutesLeft = Convert.ToInt32(Minutes);
            }

            if (Hours > 0)
            {
                MinutesLeft -= Hours * 60;
                Session.SendWhisper("Você baniu com sucesso'" + Username + "' por " + Hours + " hora(s) e " + MinutesLeft + " minuto(s) com a razão: '" + Reason + "'!", 1);
            }
            else
            {
                if (Minutes == "perm")
                    Session.SendWhisper("Você baniu o usuário '" + Username + "' permanente com a razão '" + Reason + "'!", 1);
                else
                    Session.SendWhisper("Você baniu o usuário '" + Username + "' por " + MinutesLeft + " minuto(s) com a razão '" + Reason + "'!", 1);
            }
        }
    }
}