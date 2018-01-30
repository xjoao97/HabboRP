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
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class DemoteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_demote"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Rebaixa um trabalhador da sua empresa."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int Bubble = 0;
            #endregion

            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir o usuário!", 1);
                return;
            }

            if (Session.GetRoleplay().JobId <= 0 && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Você não faz parte de uma empresa!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar este usuário, talvez ele esteja offline!", 1);
                return;
            }

            int JobRank = TargetClient.GetRoleplay().JobRank;

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

            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (!GroupManager.HasJobCommand(Session, "demote"))
                {
                    Session.SendWhisper("Você não tem um cargo suficientemente alto na empresa para usar este comando!", 1);
                    return;
                }

                if (Session.GetRoleplay().JobId != TargetClient.GetRoleplay().JobId)
                {
                    Session.SendWhisper("Este cidadão não trabalha para você!", 1);
                    return;
                }

                Bubble = 4;
            }

            if (Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                Bubble = 23;

            if (!GroupManager.JobExists(TargetClient.GetRoleplay().JobId, (JobRank - 1))) 
            {
                Session.SendWhisper("Você não pode mais rebaixar seu trabalhador!", 1);
                return;
            }
            #endregion

            #region Execute
            if (TargetClient.GetRoleplay().IsWorking)
            {
                WorkManager.RemoveWorkerFromList(TargetClient);
                TargetClient.GetRoleplay().IsWorking = false;
                TargetClient.GetHabbo().Poof();

                if (GroupManager.HasJobCommand(TargetClient, "guide"))
                {
                    PlusEnvironment.GetGame().GetGuideManager().RemoveGuide(TargetClient);
                    TargetClient.SendMessage(new HelperToolConfigurationComposer(TargetClient));

                    #region End Existing Calls
                    if (TargetClient.GetRoleplay().GuideOtherUser != null)
                    {
                        TargetClient.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                        TargetClient.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                        if (TargetClient.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                        {
                            TargetClient.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                            TargetClient.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                        }

                        TargetClient.GetRoleplay().GuideOtherUser = null;
                        TargetClient.SendMessage(new OnGuideSessionDetachedComposer(0));
                        TargetClient.SendMessage(new OnGuideSessionDetachedComposer(1));
                    }
                    #endregion
                }
            }

            TargetClient.GetRoleplay().JobRank--;

            Group Job = GroupManager.GetJob(TargetClient.GetRoleplay().JobId);
            GroupRank Rank = GroupManager.GetJobRank(TargetClient.GetRoleplay().JobId, TargetClient.GetRoleplay().JobRank);

            Job.UpdateJobMember(TargetClient.GetHabbo().Id);
            Session.Shout("*Rebaixa " + TargetClient.GetHabbo().Username + " na empresa " + Job.Name + " para " + Rank.Name + "*", Bubble);
            #endregion
        }
    }
}