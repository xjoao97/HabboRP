using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class SummonStaffCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_summon_staff"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Convoca todos os membros da equipe on-line para a sala em que você está."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para enviar.", 1);
                return;
            }

            List<string> CantSummon = new List<string>();

            int OnlineStaff = 0;

            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client == Session)
                    continue;

                if (client.GetHabbo().CurrentRoom == Room)
                    continue;

                if (!client.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    continue;

                OnlineStaff++;

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

                RoleplayManager.SendUser(client, Room.Id, "Você foi puxado ao quarto por " + Session.GetHabbo().Username + "!");
            }
            if (OnlineStaff > 0)
                Session.Shout("*Puxa para o quarto todos os membros da equipe on-lines*", 23);
            else
            {
                Session.SendWhisper("Desculpa! Não há mais Staffs online neste momento!", 1);
                return;
            }

            if (CantSummon.Count > 0)
            {
                string Users = "";

                foreach (string user in CantSummon)
                {
                    Users += user + ",";
                }

                Session.SendMessage(new MOTDNotificationComposer("Desculpe, não pôde trazer os seguintes Staffs, pois estão dentro de um Evento!\n\n " + Users));
            }
        }
    }
}
