using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangRankCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_rank"; }
        }

        public string Parameters
        {
            get { return "%usuário% %cargo_id%"; }
        }

        public string Description
        {
            get { return "Define o cargo do usuário selecionado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length != 3)
            {
                Session.SendWhisper("Digite um nome de usuário e um cargo para atribuí-los!", 1);
                return;
            }

            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            GroupRank GangRank = GroupManager.GetGangRank(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank);

            if (Gang == null)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (Gang.Id <= 1000)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (!GroupManager.HasGangCommand(Session, "grank"))
            {
                Session.SendWhisper("Você não possui uma classificação suficientemente alta para usar esse comando!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == Session)
            {
                Session.SendWhisper("Você não pode usar este comando em você mesmo!", 1);
                return;
            }

            if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetRoleplay() == null)
            {
                Session.SendWhisper("Este usuário não pôde ser encontrado, talvez ele esteja offline.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().GangId != Gang.Id)
            {
                Session.SendWhisper("Este usuário não está na mesma gangue do que você!", 1);
                return;
            }

            int Rank;

            if (int.TryParse(Params[2], out Rank))
            {
                if (Rank < 1 || Rank > 5)
                {
                    Session.SendWhisper("Insira um ranking válido (1 à 5)!", 1);
                    return;
                }

                GroupRank NewGangRank = GroupManager.GetGangRank(Gang.Id, Rank);

                if (GroupManager.GetGangMembersByRank(Gang.Id, Rank).Count >= NewGangRank.Limit && NewGangRank.Limit != 0)
                {
                    Session.SendWhisper("Desculpa! Há muitos membros no cargo " + NewGangRank.Name + "!", 1);
                    return;
                }

                if (Rank > TargetClient.GetRoleplay().GangRank)
                    Session.Shout("*Promove " + TargetClient.GetHabbo().Username + " para o cargo " + NewGangRank.Name + " em sua gangue " + Gang.Name + "*", 4);
                else
                    Session.Shout("*Rebaixa " + TargetClient.GetHabbo().Username + " para o cargo " + NewGangRank.Name + " em sua gangue " + Gang.Name + "*", 4);

                TargetClient.GetRoleplay().GangRank = Rank;
                TargetClient.GetRoleplay().GangRequest = 0;

                Gang.UpdateGangMember(TargetClient.GetHabbo().Id);
            }
            else
            {
                Session.SendWhisper("Insira um ranking de gangues válido (1 à 5)!", 1);
                return;
            }
        }
    }
}