using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangListCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_list"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista das 10 melhores gangues com base nos pontos da gangue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            lock (GroupManager.Gangs)
            {
                List<Group> GangList = GroupManager.Gangs.Values.Where(x => x.Id > 1000).OrderByDescending(x => x.GangScore).Take(10).ToList();

                StringBuilder Message = new StringBuilder();
                Message.Append("<---------- Top 10 Gangues por Pontos ---------->\n\n");

                foreach (Group Gang in GangList)
                {
                    Message.Append("----- " + Gang.Name + " -----\n");
                    Message.Append("Classificação: " + (GangList.FindIndex(x => x.Id == Gang.Id) + 1) + " out of " + String.Format("{0:N0}", GangList.Count) + "\n");
                    Message.Append("Matou: " + String.Format("{0:N0}", Gang.GangKills) + " pessoas\n");
                    Message.Append("Morreu: " + String.Format("{0:N0}", Gang.GangDeaths) + " vezes\n");
                    Message.Append("Pontos: " + String.Format("{0:N0}", Gang.GangScore) + " pontos\n");
                    Message.Append("Fundada por: " + (PlusEnvironment.GetHabboById(Gang.CreatorId) == null ? "HabboRPG" : PlusEnvironment.GetHabboById(Gang.CreatorId).Username) + "\n\n");
                }

                Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
            }
        }
    }
}