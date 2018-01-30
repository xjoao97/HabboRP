using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class BlackListCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_blacklist"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Adiciona o usuário à lista negra."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                if (BlackListManager.BlackList.Count <= 0)
                {
                    Session.SendWhisper("Não há usuários na lista negra!", 1);
                    return;
                }
                else
                {
                    StringBuilder Message = new StringBuilder().Append("<----- Usuários atuais da lista negra ----->\n\n");

                    foreach (var user in BlackListManager.BlackList)
                    {
                        Message.Append(PlusEnvironment.GetHabboById(user).Username + "\n");
                    }
                    Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
                }
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id`,`username` FROM `users` where `username` = '" + Params[1] + "' LIMIT 1");
                    var UserRow = dbClient.getRow();

                    if (UserRow == null)
                    {
                        Session.SendWhisper("Esta pessoa não existe!", 1);
                        return;
                    }

                    int UserId = Convert.ToInt32(UserRow["id"]);
                    string Username = UserRow["username"].ToString();

                    if (BlackListManager.BlackList.Contains(UserId))
                    {
                        Session.SendWhisper("Esta pessoa já foi colocada na lista negra!", 1);
                        return;
                    }
                    else
                    {
                        BlackListManager.AddBlackList(UserId);
                        Session.Shout("*Adiciona imediatamente " + Username + " na lista negra*", 23);
                        return;
                    }
                }
            }
            else
            {
                if (BlackListManager.BlackList.Contains(TargetClient.GetHabbo().Id))
                {
                    Session.SendWhisper("Esta pessoa já foi colocada na lista negra!", 1);
                    return;
                }
                else
                {
                    if (TargetClient.GetRoleplay().IsWorking && GroupManager.HasJobCommand(TargetClient, "guide"))
                        TargetClient.GetRoleplay().IsWorking = false;

                    BlackListManager.AddBlackList(TargetClient.GetHabbo().Id);
                    Session.Shout("*Adiciona imediatamente " + TargetClient.GetHabbo().Username + " na lista negra*", 23);
                    TargetClient.SendNotification("Você foi adicionado a lista negra por " + Session.GetHabbo().Username + "!");
                    return;
                }
            }
        }
    }
}