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
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class UnBlackListCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_blacklist_undo"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Remove o usuário da lista negra."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o nome de usuário da pessoa que deseja desbloquear!", 1);
                return;
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

                    if (!BlackListManager.BlackList.Contains(UserId))
                    {
                        Session.SendWhisper("Esta pessoa não foi colocada na lista negra!", 1);
                        return;
                    }
                    else
                    {
                        BlackListManager.RemoveBlackList(UserId);
                        Session.Shout("*Remove imediatamente " + Username + " da lista negra*", 23);
                        return;
                    }
                }
            }
            else
            {
                if (!BlackListManager.BlackList.Contains(TargetClient.GetHabbo().Id))
                {
                    Session.SendWhisper("Esta pessoa não foi colocada na lista negra!", 1);
                    return;
                }
                else
                {
                    BlackListManager.RemoveBlackList(TargetClient.GetHabbo().Id);
                    Session.Shout("*Remove imediatamente " + TargetClient.GetHabbo().Username + " da lista negra*", 23);
                    TargetClient.SendNotification("Você foi removido da lista negra por " + Session.GetHabbo().Username + "!");
                    return;
                }
            }
        }
    }
}