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
    class GangInfoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_info"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista das informações da sua gangue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);

            if (Gang == null || Gang.Id <= 1000)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            int ScoreRanking = GroupManager.Gangs.Count;
            int KillRanking = GroupManager.Gangs.Count;
            int DeathRanking = GroupManager.Gangs.Count;

            lock (GroupManager.Gangs)
            {
                List<Group> GangList = GroupManager.Gangs.Values.Where(x => x.Id > 1000).ToList();
                ScoreRanking = GangList.OrderByDescending(x => x.GangScore).ToList().FindIndex(x => x.Id == Gang.Id);
                KillRanking = GangList.OrderByDescending(x => x.GangKills).ToList().FindIndex(x => x.Id == Gang.Id);
                DeathRanking = GangList.OrderBy(x => x.GangDeaths).ToList().FindIndex(x => x.Id == Gang.Id);

                StringBuilder Message = new StringBuilder();
                Message.Append("----- " + Gang.Name + " -----\n\n");
                Message.Append("Matou: " + String.Format("{0:N0}", Gang.GangKills) + " pessoas ---> Classificaçao " + String.Format("{0:N0}", KillRanking) + " de " + String.Format("{0:N0}", GangList.Count) + "\n");
                Message.Append("Morreu: " + String.Format("{0:N0}", Gang.GangDeaths) + " vezes ---> Classificaçao " + String.Format("{0:N0}", DeathRanking) + " out of " + String.Format("{0:N0}", GangList.Count) + "\n");
                Message.Append("Pontos: " + String.Format("{0:N0}", Gang.GangScore) + " pontos ---> Classificaçao " + String.Format("{0:N0}", ScoreRanking) + " out of " + String.Format("{0:N0}", GangList.Count) + "\n\n");
                Message.Append("Fundada Por: " + (PlusEnvironment.GetHabboById(Gang.CreatorId) == null ? "HabboRPG" : PlusEnvironment.GetHabboById(Gang.CreatorId).Username) + "\n");

                if (Gang.Members.Values.Where(x => x.UserRank == 5).ToList().Count > 0)
                {
                    GroupMember Member = Gang.Members.Values.FirstOrDefault(x => x.UserRank == 5);
                    Message.Append("Co-Fundada por: " + (PlusEnvironment.GetHabboById(Member.UserId) == null ? "Ninguém" : PlusEnvironment.GetHabboById(Member.UserId).Username) + "\n");
                }
                else
                    Message.Append("Co-Fundada por: Ninguém\n");

                if (Gang.Members.Values.Where(x => x.UserRank == 4).ToList().Count > 0)
                {
                    List<int> Members = Gang.Members.Values.Where(x => x.UserRank == 4).Select(x => x.UserId).ToList();
                    List<string> Names = Members.Select(x => PlusEnvironment.GetHabboById(x) == null ? "SKIPTHIS" : PlusEnvironment.GetHabboById(x).Username).Where(x => x != "SKIPTHIS").ToList();
                    Message.Append("Médicos: " + String.Join(", ", Names) + "\n");
                }
                else
                    Message.Append("Médicos: Nenhum\n");

                if (Gang.Members.Values.Where(x => x.UserRank == 3).ToList().Count > 0)
                {
                    List<int> Members = Gang.Members.Values.Where(x => x.UserRank == 3).Select(x => x.UserId).ToList();
                    List<string> Names = Members.Select(x => PlusEnvironment.GetHabboById(x) == null ? "SKIPTHIS" : PlusEnvironment.GetHabboById(x).Username).Where(x => x != "SKIPTHIS").ToList();
                    Message.Append("Atiradores: " + String.Join(", ", Names) + "\n");
                }
                else
                    Message.Append("Atiradores: Nenhum\n");

                if (Gang.Members.Values.Where(x => x.UserRank == 2).ToList().Count > 0)
                {
                    List<int> Members = Gang.Members.Values.Where(x => x.UserRank == 2).Select(x => x.UserId).ToList();
                    List<string> Names = Members.Select(x => PlusEnvironment.GetHabboById(x) == null ? "SKIPTHIS" : PlusEnvironment.GetHabboById(x).Username).Where(x => x != "SKIPTHIS").ToList();
                    Message.Append("Fighters: " + String.Join(", ", Names) + "\n");
                }
                else
                    Message.Append("Lutadores: Nenhum\n");

                if (Gang.Members.Values.Where(x => x.UserRank == 1).ToList().Count > 0)
                {
                    List<int> Members = Gang.Members.Values.Where(x => x.UserRank == 1).Select(x => x.UserId).ToList();
                    List<string> Names = Members.Select(x => PlusEnvironment.GetHabboById(x) == null ? "SKIPTHIS" : PlusEnvironment.GetHabboById(x).Username).Where(x => x != "SKIPTHIS").Take(10).ToList();
                    Message.Append("Primeiros 10 Recrutas: " + String.Join(", ", Names) + "\n");
                }
                else
                    Message.Append("Primeiros 10 Recrutas: Nenhum\n");

                Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
            }
        }
    }
}