using System;
using System.Linq;
using System.Text;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.HabboRoleplay.Bots.Types
{
    public class BusinessBot : RoleplayBotAI
    {
        int VirtualId;
        CryptoRandom Rand;
        public bool CheckForOtherWorkers;
        public int OnDutyCheckInterval;
        public int CurOnDutyCheckTime;

        public BusinessBot(int VirtualId)
        {
            this.OnDuty = true;
            this.CheckForOtherWorkers = true;
            this.CurOnDutyCheckTime = 0;
            this.VirtualId = VirtualId;

            Rand = new CryptoRandom();
        }

        public override void OnDeployed(GameClient Client)
        {

        }

        public override void OnDeath(GameClient Client)
        {

        }

        public override void OnArrest(GameClient Client)
        {

        }

        public override void OnAttacked(GameClient Client)
        {

        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
            if (!OnDuty)
                return;
        }

        public override void OnUserEnterRoom(GameClient Client)
        {
            if (!OnDuty)
                return;

            if (!GetRoomUser().IsWalking)
            {
                // Look at the user 
            }
        }

        public override void OnUserUseTeleport(GameClient Client, object[] Params)
        {
            if (!OnDuty)
                return;

            if (Client == null) return;
            if (Client.GetRoomUser() == null) return;

            if (Client == GetBotRoleplay().UserFollowing || Client == GetBotRoleplay().UserAttacking)
                GetBotRoleplay().StartTeleporting(GetRoomUser(), GetRoom(), Params);
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            GameClient Client = User.GetClient();

            if (Client == null)
                return;
            HandleRequest(Client, Message);
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
            if (!OnDuty)
                return;

            if (User.GetClient() == null)
                return;
            HandleRequest(User.GetClient(), Message);
        }

        public override void OnMessaged(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;
        }

        public override void HandleRequest(GameClient Client, string Message)
        {
            if (!OnDuty)
                return;

            if (GetBotRoleplay().WalkingToItem)
                return;

            if (RespondToSpeech(Client, Message))
                return;

            lock (GroupManager.Jobs)
            {
                List<string> JobNames = GroupManager.Jobs.Values.Select(x => x.Name.ToLower()).ToList();
                string Name = GetBotRoleplay().Name.ToLower();

                if (Message.ToLower() == "promover")
                {
                    var Job = GroupManager.GetJob(Client.GetRoleplay().JobId);

                    if (Job == null || Job.Id == 1)
                    {
                        string WhisperMessage = "Obter um emprego primeiro!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    if (!GroupManager.JobExists(Job.Id, Client.GetRoleplay().JobRank + 1) || Client.GetRoleplay().JobRank > 3)
                    {
                        string WhisperMessage = "Desculpe, mas não posso promovê-lo mais! Pergunte ao seu fundador corporativo em vez disso!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    if (GroupManager.HasJobCommand(Client, "guide"))
                    {
                        string WhisperMessage = "Desculpe, mas apenas o chefe da polícia pode promovê-lo!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    if (GroupManager.HasJobCommand(Client, "farming"))
                    {
                        string WhisperMessage = "Desculpe, mas os agricultores são promovidos automaticamente ao nivelar a agricultura deles!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }

                    int TimeRequired = Client.GetRoleplay().JobRank * 200;

                    if (Client.GetRoleplay().TimeWorked >= TimeRequired)
                    {
                        var OldJobRank = GroupManager.GetJobRank(Job.Id, Client.GetRoleplay().JobRank);
                        var NewJobRank = GroupManager.GetJobRank(Job.Id, Client.GetRoleplay().JobRank + 1);

                        GetRoomUser().Chat("*Promove " + Client.GetHabbo().Username + " de " + OldJobRank.Name + " para um " + NewJobRank.Name + " na empresa " + Job.Name + "*", true);

                        if (Client.GetRoleplay().IsWorking)
                        {
                            WorkManager.RemoveWorkerFromList(Client);
                            Client.GetRoleplay().IsWorking = false;
                            Client.GetHabbo().Poof();
                        }

                        Client.GetRoleplay().JobRank++;
                        Job.UpdateJobMember(Client.GetHabbo().Id);
                        return;
                    }
                    else
                    {
                        string WhisperMessage = "Desculpe, mas você precisa " + TimeRequired + " minutos trabalhados para ser promovido!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }
                }
                if (Message.ToLower() == Name)
                    GetRoomUser().Chat("Sim " + Client.GetHabbo().Username + ", você quer um emprego?", true);
                else if (Message.ToLower() == "emprego" || Message.ToLower() == "empregos" || Message.ToLower() == "trabalho")
                {
                    StringBuilder JobList = new StringBuilder().Append("<------ Trabalhos Disponíveis ------>\n\n");
                    List<GroupRank> JobRanks = GroupManager.Jobs.Values.Where(x => x.Id > 1).Select(x => GroupManager.GetJobRank(x.Id, 1)).Where(x => x != null).ToList();

                    foreach (GroupRank Rank in JobRanks)
                    {
                        Group Job = GroupManager.GetJob(Rank.JobId);

                        if (Job != null && Job.Members.Count < Rank.Limit)
                            JobList.Append("" + Job.Name + " - Salário R$" + Rank.Pay + " por 15 minutos. [" + (Rank.Limit - Job.Members.Count) + " vagas restantes]\n\n");
                    }
                    Client.SendMessage(new MOTDNotificationComposer(JobList.ToString()));
                }
                else if (JobNames.Contains(Message.ToLower()))
                {
                    Group Job = GroupManager.Jobs.Values.FirstOrDefault(x => x.Name.ToLower() == Message.ToLower());

                    if (Job == null)
                        return;

                    GroupRank JobRank = GroupManager.GetJobRank(Job.Id, 1);

                    if (JobRank == null)
                        return;

                    if (Job.Members.Values.Where(x => x.UserRank == 1).ToList().Count < JobRank.Limit)
                    {
                        if (JobRank.HasCommand("guide") && BlackListManager.BlackList.Contains(Client.GetHabbo().Id))
                        {
                            string WhisperMessage = "Desculpe, mas você foi na lista negra de se juntar à corporação policial!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            return;
						}
						
						if (JobRank.HasCommand("guide"))
						if (Client.GetRoleplay().Level < 5)
						{
						string WhisperMessage = "Desculpe, mas você precisa ser nível 5 ou mais para entrar neste emprego!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            return;
						}
						
						if (JobRank.HasCommand("exercito"))
						if (Client.GetRoleplay().Level < 3)
						{
						string WhisperMessage = "Desculpe, mas você precisa ser nível 3 ou mais para entrar neste emprego!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            return;
						}
						
						if (JobRank.HasCommand("carlos"))
						if (Client.GetRoleplay().JobRank < 2)
						{
						string WhisperMessage = "Você precisa ser um Cabo do Exército para entrar neste cargo";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            return;
						}
			
                        if (Client.GetRoleplay().JobId == Job.Id)
                        {
                            string WhisperMessage = "Você já trabalha na corporação " + Job.Name + "!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            return;
                        }

                        if (Client.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("emprego"))
                        {
                            string WhisperMessage = "Você já recebeu uma oferta de Emprego! Digite ':aceitar emprego' ou ':recusar emprego' ou :ofertas, para ver o que aceitar/recusar!";
                            Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                            return;
                        }

                        Client.GetRoleplay().OfferManager.CreateOffer("emprego", 0, Job.Id, this);
                        Client.SendWhisper("Você acabou de receber um emprego na empresa " + Job.Name + " como um " + JobRank.Name + "! Digite ':aceitar emprego' para aceitar!", 1);
                        GetRoomUser().Chat("*Oferece um emprego para " + Client.GetHabbo().Username + " na Empresa " + Job.Name + "*", true);
                        return;
                    }
                    else
                    {
                        string WhisperMessage = "Desculpe, mas a empresa " + Job.Name + " não está contratando ninguém agora!";
                        Client.SendMessage(new WhisperComposer(GetRoomUser().VirtualId, WhisperMessage, 0, 2));
                        return;
                    }
                }
            }
        }

        public override void StopActivities()
        {

        }

        public override void StartActivities()
        {

        }
    }
}