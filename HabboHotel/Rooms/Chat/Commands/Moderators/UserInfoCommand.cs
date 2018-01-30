using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class UserInfoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_user_info"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Veja outras informações do perfil dos usuários."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja visualizar.", 1);
                return;
            }

            DataRow UserData = null;
            DataRow UserInfo = null;
            string Username = Params[1];

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`username`,`mail`,`rank`,`motto`,`credits`,`activity_points`,`vip_points`,`event_points`,`online`,`rank_vip` FROM users WHERE `username` = @Username LIMIT 1");
                dbClient.AddParameter("Username", Username);
                UserData = dbClient.getRow();
            }

            if (UserData == null)
            {
                Session.SendWhisper("Opa, there is no user in the database with that username (" + Username + ")!", 1);
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(UserData["id"]) + "' LIMIT 1");
                UserInfo = dbClient.getRow();
                if (UserInfo == null)
                {
                    dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + Convert.ToInt32(UserData["id"]) + "')");

                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + Convert.ToInt32(UserData["id"]) + "' LIMIT 1");
                    UserInfo = dbClient.getRow();
                }
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

            StringBuilder HabboInfo = new StringBuilder();
            HabboInfo.Append(Convert.ToString(UserData["username"]) + "' - Conta:\r\r");
            HabboInfo.Append("Informações [Usuário]:\r");
            HabboInfo.Append("ID: " + String.Format("{0:N0}", Convert.ToInt32(UserData["id"])) + "\r");
            HabboInfo.Append("Cargo: " + Convert.ToInt32(UserData["rank"]) + "\r");
            HabboInfo.Append("Cargo VIP: " + Convert.ToInt32(UserData["rank_vip"]) + "\r");
            HabboInfo.Append("Email: " + Convert.ToString(UserData["mail"]) + "\r");
            HabboInfo.Append("Status: " + (TargetClient != null ? "Online" : "Offline") + "\r\r");

            HabboInfo.Append("Informações [R$]:\r");
            HabboInfo.Append("Grana: " + String.Format("{0:N0}", Convert.ToInt32(UserData["credits"])) + "\r");
            HabboInfo.Append("Créditos de Celular: " + String.Format("{0:N0}", Convert.ToInt32(UserData["activity_points"])) + "\r");
            HabboInfo.Append("Diamantes: " + String.Format("{0:N0}", Convert.ToInt32(UserData["vip_points"])) + "\r");
            HabboInfo.Append("Pontos de Evento: " + String.Format("{0:N0}", Convert.ToInt32(UserData["event_points"])) + "\r\r");

            HabboInfo.Append("Informações [Moderação]:\r");
            HabboInfo.Append("Bans: " + String.Format("{0:N0}", Convert.ToInt32(UserInfo["bans"])) + "\r");
            HabboInfo.Append("CFHs Enviados: " + String.Format("{0:N0}", Convert.ToInt32(UserInfo["cfhs"])) + "\r");
            HabboInfo.Append("CFHs Abusivos: " + String.Format("{0:N0}", Convert.ToInt32(UserInfo["cfhs_abusive"])) + "\r");

            if (TargetClient != null)
            {
                HabboInfo.Append("Informações [Atual]:\r");
                if (!TargetClient.GetHabbo().InRoom)
                    HabboInfo.Append("Atualmente não está em uma sala.\r");
                else
                {
                    HabboInfo.Append("Quarto: " + TargetClient.GetHabbo().CurrentRoom.Name + " (" + TargetClient.GetHabbo().CurrentRoom.RoomId + ")\r");
                    HabboInfo.Append("Dono do Quarto: " + TargetClient.GetHabbo().CurrentRoom.OwnerName + "\r");
                    HabboInfo.Append("Visitantes atuais: " + TargetClient.GetHabbo().CurrentRoom.UserCount + "/" + TargetClient.GetHabbo().CurrentRoom.UsersMax + "\r");
                    HabboInfo.Append("Invisivel: " + (TargetClient.GetRoleplay().Invisible ? "Sim" : "Não") + "\r");
                }
            }
            Session.SendNotification(HabboInfo.ToString());
        }
    }
}
