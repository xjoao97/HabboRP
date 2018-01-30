using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police
{
    class VoteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_related_court_vote"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Vote se o réu for considerado culpado ou inocente."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (!RoleplayManager.CourtTrialStarted)
            {
                Session.SendWhisper("Desculpe, mas você não pode votar agora. Por favor, tente novamente mais tarde!", 1);
                return;
            }

            if (!RoleplayManager.InvitedUsersToJuryDuty.Contains(Session))
            {
                Session.SendWhisper("Desculpe, mas você não pode votar porque nunca foi convidado a participar do júri!", 1);
                return;
            }

            if (!RoleplayManager.CourtVoteEnabled)
            {
                Session.SendWhisper("Desculpe, mas você ainda não tem permissão para votar. Por favor, tente novamente mais tarde!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("votar", false))
            {
                Session.SendWhisper("Você já votou!", 1);
                return;
            }

            #endregion

            #region Execute

            string Type = Params[1];

            switch(Type)
            {
                case "inno":
                case "innocent":
				case "inocente":
                    {
                        RoleplayManager.InnocentVotes++;
                        Session.SendWhisper("Seu voto [INOCENTE] foi enviado! Por favor, espere...", 1);

                        Session.GetRoleplay().CooldownManager.CreateCooldown("votar", 1000, 60);
                        break;
                    }

                case "guilty":
				case "culpado":
                    {
                        RoleplayManager.GuiltyVotes++;
                        Session.SendWhisper("Seu voto [CULPADO] foi enviado! Por favor, espere...", 1);

                        Session.GetRoleplay().CooldownManager.CreateCooldown("votar", 1000, 60);
                        break;
                    }

                default:
                    {
                        Session.SendWhisper("Ação inválida! Use apenas 'inocente' ou 'culpado' de todos os crimes!", 1);
                        break;
                    }
            }

            #endregion
        }
    }
}