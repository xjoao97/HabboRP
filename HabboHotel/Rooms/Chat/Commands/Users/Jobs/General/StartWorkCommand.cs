using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class StartWorkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_corp_work_start"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Começa a trabalhar."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você já está trabalhando!", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode trabalhar enquanto está morto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode trabalhar enquanto está preso!", 1);
                return;
            }

            if (Session.GetRoleplay().JobId == 1)
            {
                Session.SendWhisper("Você não pode trabalhar, você está desempregado!", 1);
                return;
            }

            if (Session.GetRoleplay().IsWorkingOut)
            {
                Session.SendWhisper("Você não pode trabalhar enquanto está malhando!", 1);
                return;
            }

            if (Session.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("sendhome"))
            {
                Session.SendWhisper("Você não pode trabalhar enquanto foi enviado para casa!", 1);
                return;
            }

            if (!GroupManager.JobExists(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
            {
                Session.GetRoleplay().TimeWorked = 0;
                Session.GetRoleplay().JobId = 1;
                Session.GetRoleplay().JobRank = 1;
                Session.GetRoleplay().JobRequest = 0;

                Group NewJob = GroupManager.GetJob(Session.GetRoleplay().JobId);
                NewJob.AddNewMember(Session.GetHabbo().Id);
                NewJob.SendPackets(Session);

                Session.SendWhisper("Desculpe, seu trabalho não existe! Ele foi removido.", 1);
                return;
            }

            Group Job = GroupManager.GetJob(Session.GetRoleplay().JobId);
            GroupRank Rank = GroupManager.GetJobRank(Job.Id, Session.GetRoleplay().JobRank);

            if (!Rank.CanWorkHere(Room.Id))
            {
                Session.SendWhisper("Este não é um dos seus quartos de trabalho! Você trabalha no(s) Quarto(s): [" + String.Join(",", Rank.WorkRooms) + "].", 1);
                return;
            }

            if (Session.GetRoleplay().CurEnergy <= 0)
            {
                Session.SendWhisper("Você não tem energia suficiente para trabalhar!", 1);
                return;
            }

            if (GroupManager.HasJobCommand(Session, "policial") && RoleplayManager.PurgeStarted)
            {
                Session.SendWhisper("Você não pode começar a trabalhar como policial em uma purga!", 1);
                return;
            }

            if (GroupManager.HasJobCommand(Session, "policial") && Room.RoomData.TurfEnabled)
            {
                Session.SendWhisper("Você não pode trabalhar como policial dentro de um território!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("startwork", true))
                return;

            #endregion

            #region Execute
            if (GroupManager.HasJobCommand(Session, "guide"))
            {
                Session.SendMessage(new HelperToolConfigurationComposer(Session));
                return;
            }

            #region Farming Level Check
            int JobRank = 1;
            if (GroupManager.HasJobCommand(Session, "farming"))
            {
                if (!Session.GetRoleplay().FarmingStats.HasPlantSatchel && !Session.GetRoleplay().FarmingStats.HasSeedSatchel)
                {
                    Session.SendWhisper("O agricultor precisa de um Saco de plantas e um Saco de sementes para que eles possam começar a trabalhar! Compre um no supermercado.", 1);
                    return;
                }

                if (Session.GetRoleplay().FarmingStats.Level < 6)
                    JobRank = 1;
                else if (Session.GetRoleplay().FarmingStats.Level >= 6 && Session.GetRoleplay().FarmingStats.Level < 11)
                    JobRank = 2;
                else if (Session.GetRoleplay().FarmingStats.Level >= 11)
                    JobRank = 3;

                if (JobRank != Session.GetRoleplay().JobRank)
                {
                    Session.GetRoleplay().JobRank = JobRank;
                    Job.UpdateJobMember(Session.GetHabbo().Id);
                }
            }
            #endregion

            Session.GetRoleplay().IsWorking = true;
            RoleplayManager.GetLookAndMotto(Session);
            WorkManager.AddWorkerToList(Session);
            Session.GetRoleplay().TimerManager.CreateTimer("work", 1000, true);
            Session.GetRoleplay().CooldownManager.CreateCooldown("startwork", 1000, 10);
            return;
            #endregion
        }
    }
}