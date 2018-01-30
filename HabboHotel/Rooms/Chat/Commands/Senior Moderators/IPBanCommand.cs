using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Moderation;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class IPBanCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ban_ip"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Bane o IP e Conta do usuário."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja banir o IP.", 1);
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário no banco de dados.", 1);
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                Session.SendWhisper("Opa, você não pode proibir esse usuário.", 1);
                return;
            }

            String IPAddress = String.Empty;
            Double Expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            string Username = Habbo.Username;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");

                dbClient.SetQuery("SELECT `ip_last` FROM `users` WHERE `id` = '" + Habbo.Id + "' LIMIT 1");
                IPAddress = dbClient.getString();
            }

            string Reason = null;
            if (Params.Length >= 3)
                Reason = CommandManager.MergeParams(Params, 2);
            else
                Reason = "Sem razão específica, mas provavelmente fez merda.";

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

            Session.SendWhisper("Successo, você baniu o usuário '" + Username + "' no ip, '" + Reason + "'!", 1);
        
            if (!string.IsNullOrEmpty(IPAddress))
                PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.IP, IPAddress, Reason, Expire);
            PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Reason, Expire);

            if (TargetClient != null)
            {
                TargetClient.Disconnect(true);
            }
        }
    }
}