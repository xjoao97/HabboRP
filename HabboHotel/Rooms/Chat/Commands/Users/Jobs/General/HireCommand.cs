using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class HireCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_hire"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Contrata um usuário para sua empresa."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ops, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar este usuário, talvez ele esteja offline.", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode fazer isso enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode fazer isso enquanto está preso!", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "hire"))
            {
                Session.SendWhisper("Você não tem um cargo tão alto na empresa para usar este comando.", 1);
                return;
            }

            if (Session.GetRoleplay().JobId == TargetClient.GetRoleplay().JobId)
            {
                Session.SendWhisper("Este cidadão já trabalha para você!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("emprego"))
            {
                Session.SendWhisper("Este cidadão já recebeu um emprego!", 1);
                return;
            }

            var Job = GroupManager.GetJob(Session.GetRoleplay().JobId);
            var JobRank = GroupManager.GetJobRank(Session.GetRoleplay().JobId, 1);

            if (JobRank.HasCommand("guide"))
            {
                if (BlackListManager.BlackList.Contains(TargetClient.GetHabbo().Id))
                {
                    Session.SendWhisper("Desculpe, mas este usuário foi listado na lista negra da corporação policial!", 1);
                    return;
                }
            }
            #endregion

            #region Execute
            TargetClient.GetRoleplay().OfferManager.CreateOffer("emprego", Session.GetHabbo().Id, Job.Id);
            Session.Shout("*Oferece para " + TargetClient.GetHabbo().Username + " um emprego no " + Job.Name + " no cargo " + JobRank.Name + "*", 4);
            TargetClient.SendWhisper("Você acabou de receber uma oferta de emprego no " + Job.Name + " no cargo " + JobRank.Name + "! Digite ':aceitar emprego' para começar!", 1);
            #endregion
        }
    }
}