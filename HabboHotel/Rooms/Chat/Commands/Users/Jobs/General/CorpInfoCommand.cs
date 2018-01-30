using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class CorpInfoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_info"; }
        }

        public string Parameters
        {
            get { return "%empresa_id%"; }
        }

        public string Description
        {
            get { return "Mostra uma lista de informações da sua empresa ou de outra."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            Group Corp = null;
            GroupRank CorpRank = null;
            #endregion

            #region Conditions
            if (Params.Length == 1)
            {
                Corp = GroupManager.GetJob(Session.GetRoleplay().JobId);
                CorpRank = GroupManager.GetJobRank(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank);
            }
            else
            {
                int CorpId;
                if (int.TryParse(Params[1], out CorpId))
                {
                    Corp = GroupManager.GetJob(CorpId);
                    CorpRank = GroupManager.GetJobRank(CorpId, 1);
                }
                else
                {
                    Session.SendWhisper("Por favor, insira um ID de empresa válido! Digite ':corplista' para ver todas!", 1);
                    return;
                }
            }

            if (Corp == null)
            {
                Session.SendWhisper("Desculpe, mas este ID de empresa não existe!", 1);
                return;
            }

            if (Corp.Id <= 1)
            {
                if (Session.GetRoleplay().JobId <= 1)
                    Session.SendWhisper("Você está desempregado", 1);
                else
                    Session.SendWhisper("Desculpe, você não pode ver as informações da 'empresa' de 'Desempregados'!", 1);
                return;
            }
            #endregion

            #region Execute
            StringBuilder Message = new StringBuilder();
            Message.Append("<----- " + Corp.Name + " ----->\n\n");
            Message.Append("Descrição: " + Corp.Description + "\n");

            if (Corp.Ranks.ContainsKey(6))
            {
                if (Corp.Members.Values.Where(x => x.UserRank == 6).ToList().Count > 0)
                {
                    List<int> Members = Corp.Members.Values.Where(x => x.UserRank == 6).Select(x => x.UserId).ToList();
                    List<string> Names = Members.Select(x => PlusEnvironment.GetHabboById(x) == null ? "SKIPTHIS" : PlusEnvironment.GetHabboById(x).Username).Where(x => x != "SKIPTHIS").ToList();
                    Message.Append("Gerenciado por: " + String.Join(", ", Names) + "\n\n");
                }
                else
                    Message.Append("Gerenciado por: Ninguém\n\n");
            }
            else
                Message.Append("\n");

            foreach (int JobRank in Corp.Ranks.Keys.Where(x => x != 6))
            {
                GroupRank Rank = Corp.Ranks[JobRank];
                if (Corp.Members.Values.Where(x => x.UserRank == JobRank).ToList().Count > 0)
                {
                    List<int> Members = Corp.Members.Values.Where(x => x.UserRank == JobRank).Select(x => x.UserId).ToList();
                    List<string> Names = Members.Select(x => PlusEnvironment.GetHabboById(x) == null ? "SKIPTHIS" : PlusEnvironment.GetHabboById(x).Username).Where(x => x != "SKIPTHIS").ToList();
                    Message.Append(Rank.Name + "(s): " + String.Join(", ", Names) + "\n\n");
                }
                else
                    Message.Append(Rank.Name + "(s): Nenhum\n\n");
            }

            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
            #endregion
        }
    }
}