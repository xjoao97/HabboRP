using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using System.Data;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class UnBanCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ban_undo"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Desbane o nome de usuário (remove o ban de usuário/mac/ ip)."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de escolher um usuário!");
                return;
            }

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `users` WHERE `username` = '" + Params[1] + "' LIMIT 1");
                DataRow Row = dbClient.getRow();

                if (Row == null)
                {
                    Session.SendWhisper("Desculpe, este nome de usuário não pôde ser encontrado no banco de dados!", 1);
                    return;
                }

                int UserId = Convert.ToInt32(Row["id"]);
                string UserName = Row["username"].ToString();
                string LastIp = Row["ip_last"].ToString();
                string MachineId = Row["machine_id"].ToString();

                dbClient.SetQuery("SELECT count(id) FROM `bans` WHERE `value` = '" + UserId + "' OR `value` = '" + UserName + "' OR `value` = '" + MachineId + "'");
                int Count = dbClient.getInteger();

                if (Count < 1)
                {
                    Session.SendWhisper("Desculpe, mas este usuário não está banido!", 1);
                    return;
                }
                else
                {
                    dbClient.RunQuery("DELETE FROM `bans` WHERE `value` = '" + LastIp + "' OR `value` = '" + UserName + "' OR `value` = '" + MachineId + "'");
                    PlusEnvironment.GetGame().GetModerationManager().ReCacheBans();
                    Session.SendWhisper("Successo, você desbaniu o usuário '" + UserName + "'!", 1);
                    return;
                }
            }
        }
    }
}
