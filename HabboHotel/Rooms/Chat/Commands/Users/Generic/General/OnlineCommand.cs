using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class OnlineCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_whos_online"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Avisa todos os cidadãos atuais online."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Online = new StringBuilder();

            var Users = PlusEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null && !x.GetHabbo().AppearOffline).ToList();
            var HiddenUsers = PlusEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null && x.GetHabbo().AppearOffline).ToList();

            lock (Users)
            {
                Online.Append("Temos " + String.Format("{0:N0}", Users.Count) + " Usuários onlines (" + String.Format("{0:N0}", HiddenUsers.Count) + " Invisíveis): \n");

                foreach (var client in Users)
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;

                    if (Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                        Online.Append("[" + client.GetHabbo().Id + "] - " + client.GetHabbo().Username + "\n");
                    else
                        Online.Append("- " + client.GetHabbo().Username + "\n");
                }
            }

            if (Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                lock (HiddenUsers)
                {
                    foreach (var client in HiddenUsers)
                    {
                        if (client == null || client.GetHabbo() == null)
                            continue;

                        if (Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                            Online.Append("[" + client.GetHabbo().Id + "] - " + client.GetHabbo().Username + "\n");
                        else
                            Online.Append("- " + client.GetHabbo().Username + "\n");
                    }
                }
            }

            Session.SendMessage(new MOTDNotificationComposer(Online.ToString()));
        }
    }
}