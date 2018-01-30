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
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class SuperHireCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_superhire"; }
        }

        public string Parameters
        {
            get { return "%usuário% %id_trabalho% %id_cargo%"; }
        }

        public string Description
        {
            get { return "Super contrata um usuário a hora que quiser."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length < 4)
            {
                Session.SendWhisper("Por favor, use o comando como ':scontratar (usuário) (ID do Emprego) (ID do Cargo)'!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            int jobId;
            if (!int.TryParse(Params[2], out jobId))
            {
                Session.SendWhisper("Por favor, insira um numero válido de Emprego!", 1);
                return;
            }

            int jobRank;
            if (!int.TryParse(Params[3], out jobRank))
            {
                Session.SendWhisper("Por favor, insira um numero válido de Cargo!!", 1);
                return;
            }

            if (!GroupManager.JobExists(jobId, jobRank))
            {
                Session.SendWhisper("Esse não é um trabalho válido!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().JobId == jobId && TargetClient.GetRoleplay().JobRank == jobRank)
            {
                Session.SendWhisper("Este cidadão já trabalha nesta empresa e tem este cargo!", 1);
                return;
            }

            var Job = GroupManager.GetJob(jobId);
            var JobRank = GroupManager.GetJobRank(jobId, jobRank);

            if (JobRank.HasCommand("guide"))
            {
                if (BlackListManager.BlackList.Contains(TargetClient.GetHabbo().Id))
                {
                    Session.SendWhisper("Desculpe, este usuário está na lista negra da polícia, então não pode ser contratado!", 1);
                    return;
                }
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

            int OriginalJob = TargetClient.GetRoleplay().JobId;
            var OldJob = GroupManager.GetJob(OriginalJob);

            TargetClient.GetRoleplay().TimeWorked = 0;
            TargetClient.GetRoleplay().JobId = jobId;
            TargetClient.GetRoleplay().JobRank = jobRank;
            TargetClient.GetRoleplay().JobRequest = 0;

            if (Job.Id == OriginalJob)
                Job.UpdateJobMember(TargetClient.GetHabbo().Id);
            else
                Job.AddNewMember(TargetClient.GetHabbo().Id, jobRank);

            Job.SendPackets(TargetClient);
            Session.SendMessage(new GroupInfoComposer(OldJob, Session));

            Session.Shout("*Super contrata imediatamente " + TargetClient.GetHabbo().Username + " na empresa '" + Job.Name + "' no cargo '" + JobRank.Name + "'*", 23);
            TargetClient.SendWhisper("Você contratou com sucesso " + Session.GetHabbo().Username + " na empresa '" + Job.Name + "' no cargo '" + JobRank.Name + "'!", 1);
            return;
            #endregion
        }
    }
}