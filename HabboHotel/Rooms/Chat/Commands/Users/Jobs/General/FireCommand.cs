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
using Plus.HabboHotel.Cache;
using Plus.Communication.Packets.Outgoing.Guides;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class FireCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_fire"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Demite um trabalhador da sua empresa."; }
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

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar este usuário, talvez ele esteja offline.", 1);
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
                if (!GroupManager.HasJobCommand(Session, "fire"))
                {
                    Session.SendWhisper("Você não tem um cargo tão alto na empresa para usar este comando!", 1);
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

            if (TargetClient == Session)
            {
                Session.SendWhisper("Você não pode demitir você mesmo!", 1);
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

            Group OldJob = GroupManager.GetJob(TargetClient.GetRoleplay().JobId);

            TargetClient.GetRoleplay().TimeWorked = 0;
            TargetClient.GetRoleplay().JobId = 1;
            TargetClient.GetRoleplay().JobRank = 1;
            TargetClient.GetRoleplay().JobRequest = 0;

            Group Job = GroupManager.GetJob(TargetClient.GetRoleplay().JobId);
            Job.AddNewMember(TargetClient.GetHabbo().Id);
            Job.SendPackets(TargetClient);
            Session.SendMessage(new GroupInfoComposer(Job, Session));

            Session.Shout("*Demite " + TargetClient.GetHabbo().Username + " da empresa " + OldJob.Name + "*", Bubble);
            TargetClient.SendWhisper("Você foi demitido da empresa " + OldJob.Name + " por " + Session.GetHabbo().Username + "!", 1);
            #endregion
        }
    }
}