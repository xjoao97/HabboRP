using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class NameCheckCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_check_name"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Verifica nomes de usuários antigos se eles já foram alterados."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja verificar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                int UserId = 0;
                string Username = Params[1];

                if (TargetClient == null)
                {
                    dbClient.SetQuery("SELECT `id`, `username` FROM `users` where `username` = '" + Username + "' LIMIT 1");
                    DataRow Row = dbClient.getRow();

                    if (Row == null)
                    {
                        Session.SendWhisper("Esta pessoa não existe!", 1);
                        return;
                    }

                    UserId = Convert.ToInt32(Row["id"]);
                    Username = Row["username"].ToString();
                }
                else
                {
                    UserId = TargetClient.GetHabbo().Id;
                    Username = TargetClient.GetHabbo().Username;
                }

                dbClient.SetQuery("SELECT `new_name`, `old_name` FROM `logs_client_namechange` WHERE `user_id` = '" + UserId + "'");
                DataTable Table = dbClient.getTable();

                if (Table.Rows.Count == 0)
                {
                    Session.SendWhisper("Este usuário nunca mudou seu nome antes!", 1);
                    return;
                }

                StringBuilder Message = new StringBuilder();
                Message.Append("<----- " + Username + " Alterações de Nome ----->\n");
                Message.Append("Nome antigo ---> Novo nome\n\n");

                foreach (DataRow Row in Table.Rows)
                {
                    string OldName = Row["old_name"].ToString();
                    string NewName = Row["new_name"].ToString();

                    Message.Append(OldName + " ---> " + NewName + "\n");
                }
                Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
            }
        }
    }
}
