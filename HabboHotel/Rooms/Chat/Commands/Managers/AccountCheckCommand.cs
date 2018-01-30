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
    class AccountCheckCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_check_account"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Verifica os outros usuários com base em MAC e IP."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 2)
            {
                Session.SendWhisper("Digite usuário que deseja verificar e se deseja verificar o IP.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                string Password = "";
                string IpReg = "";
                string IpLast = "";
                string MachineID = "";
                string Username = Params[1];

                if (TargetClient == null)
                {
                    dbClient.SetQuery("SELECT `ip_last`, `ip_reg`, `machine_id`, `password`, `username` FROM `users` where `username` = '" + Username + "' LIMIT 1");
                    DataRow Row = dbClient.getRow();

                    if (Row == null)
                    {
                        Session.SendWhisper("Desculpa! Esta pessoa não existe!", 1);
                        return;
                    }

                    Password = Row["password"].ToString();
                    IpReg = Row["ip_reg"].ToString();
                    IpLast = Row["ip_last"].ToString();
                    MachineID = Row["machine_id"].ToString();
                    Username = Row["username"].ToString();
                }
                else
                {
                    dbClient.SetQuery("SELECT `password`, `ip_reg` FROM `users` where `id` = '" + TargetClient.GetHabbo().Id + "' LIMIT 1");
                    DataRow Row = dbClient.getRow();

                    Password = Row["password"].ToString();
                    IpReg = Row["ip_reg"].ToString();
                    IpLast = TargetClient.GetConnection().getIp();
                    MachineID = TargetClient.GetHabbo().MachineId;
                    Username = TargetClient.GetHabbo().Username;
                }

                DataTable Table = null;
                #region IpLast/IpReg/MachineID/Password
                if (Params[2] == "yes")
                {
                    string[] IpRegCheck = IpReg.Split('.');
                    string[] IpLastCheck = IpLast.Split('.');

                    if (IpRegCheck.Length >= 4)
                        IpReg = IpRegCheck[0] + "." + IpRegCheck[1];

                    if (IpLastCheck.Length >= 4)
                        IpLast = IpLastCheck[0] + "." + IpLastCheck[1];

                    dbClient.SetQuery("SELECT `id`, `username` FROM `users` where `ip_last` like '%" + IpLast + "%' or `ip_reg` like '%" + IpReg + "%' or `ip_last` like '%" + IpReg + "%' or `ip_reg` like '%" + IpLast + "%' AND `ip_reg` != '::1' or (`machine_id` = '" + MachineID + "' AND `machine_id` != '') or `password` = '" + Password + "'");
                    Table = dbClient.getTable();
                }
                #endregion

                #region MachineId/Password
                else
                {
                    dbClient.SetQuery("SELECT `id`, `username` FROM `users` WHERE (`machine_id` = '" + MachineID + "' AND `machine_id` != '') OR `password` = '" + Password + "'");
                    Table = dbClient.getTable();
                }
                #endregion

                if (Table == null)
                    return;

                if (Table.Rows.Count == 0)
                {
                    Session.SendWhisper("Por algum motivo estranho, o banco de dados não contém nenhum dado nesse usuário.", 1);
                    return;
                }

                StringBuilder Message = new StringBuilder();
                Message.Append("<----- " + Username + " <-> Outras contas ----->\n");
                Message.Append("[ID] --- USUÁRIO\n\n");

                foreach (DataRow Row in Table.Rows)
                {
                    int UserId = Convert.ToInt32(Row["id"]);
                    string name = Row["username"].ToString();

                    Message.Append("[" + UserId + "] --- " + name + "\n");
                }
                Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
            }
        }
    }
}
