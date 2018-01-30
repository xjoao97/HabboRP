using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class SummonAllCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_summon_all"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Convoca todos os usuários online para o quarto em que você está."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            List<string> CantSummon = new List<string>();

            int OnlineUsers = 0;

            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client == Session)
                    continue;

                if (client.GetHabbo().CurrentRoom != null)
                {
                    if (client.GetHabbo().CurrentRoom == Room)
                        continue;

                    if (client.GetHabbo().CurrentRoom.TutorialEnabled)
                        continue;
                }

                OnlineUsers++;

                if (client.GetRoleplay().Game != null)
                {
                    CantSummon.Add(client.GetHabbo().Username);
                    continue;
                }

                if (client.GetRoleplay().IsDead)
                {
                    client.GetRoleplay().IsDead = false;
                    client.GetRoleplay().ReplenishStats(true);
                    client.GetHabbo().Poof();
                }

                if (client.GetRoleplay().IsJailed)
                {
                    client.GetRoleplay().IsJailed = false;
                    client.GetRoleplay().JailedTimeLeft = 0;
                    client.GetHabbo().Poof();
                }

                RoleplayManager.SendUser(client, Room.Id, "Você foi convocado por " + Session.GetHabbo().Username + "!");
            }
            if (OnlineUsers > 0)
                Session.Shout("*Convoca todos os usuários online para a sala*", 23);
            else
            {
                Session.SendWhisper("Desculpa! Não há outros usuários online neste momento!", 1);
                return;
            }

            if (CantSummon.Count > 0)
            {
                string Users = "";

                foreach (string user in CantSummon)
                {
                    Users += user + ",";
                }

                Session.SendMessage(new MOTDNotificationComposer("Desculpe, não foi possível trazer os seguintes usuários, pois estão dentro de um Evento!\n\n " + Users));
            }
        }
    }
}
