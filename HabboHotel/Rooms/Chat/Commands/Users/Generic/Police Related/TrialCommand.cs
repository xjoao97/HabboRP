using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Police
{
    class TrialCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_related_court_trial"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Requests a court trial."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (RoleplayManager.CourtTrialStarted || RoleplayManager.CourtTrialIsStarting)
            {
                Session.SendWhisper("Desculpe, um julgamento está sendo realizado atualmente por " + (RoleplayManager.Defendant != null && RoleplayManager.Defendant.GetHabbo() != null ? " por " + RoleplayManager.Defendant.GetHabbo().Username : "") + ". Por favor tente novamente!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode solicitar um julgamento judicial enquanto você não está preso!", 1);
                return;
            }

            if (Session.GetRoleplay().Trialled)
            {
                Session.SendWhisper("Desculpe, você solicitou recentemente um julgamento, tente novamente mais tarde!", 1);
                return;
            }

            if (Session.GetRoleplay().JailedTimeLeft < 10)
            {
                Session.SendWhisper("Desculpe, apenas prisioneiros presos que têm 10 ou mais minutos na prisão podem solicitar um julgamento!", 1);
                return;
            }

            #endregion

            #region Execute

            List<GameClients.GameClient> RandomUsers = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null orderby new Utilities.CryptoRandom().Next() select Client).ToList();

            lock (RandomUsers)
            {
                foreach (var client in RandomUsers.Take(50))
                {
                    if (client == null || client.GetHabbo() == null || client.GetRoomUser() == null || client.GetRoleplay() == null || client.GetRoomUser().IsAsleep || 
                        client.GetRoleplay().IsJailed || client.GetRoleplay().IsDead || client.GetRoleplay().IsWanted || client.GetRoleplay().Game != null ||
                        client.GetRoleplay().IsWorkingOut)
                        continue;

                    RoleplayManager.InvitedUsersToJuryDuty.Add(client);
                    client.SendWhisper("Você foi convidado ao tribunal [Quarto ID: " + Convert.ToInt32(RoleplayData.GetData("court", "roomid")) + "] / [Lado de fora: ID: " + Convert.ToInt32(RoleplayData.GetData("court", "outsideroomid")) + "] para participar do julgamento. Você tem " + Convert.ToInt32(RoleplayData.GetData("court", "invitationtime")) + " minuto(s) para ir até lá!", 26);
                    client.SendMessage(new RoomNotificationComposer("jury_invitation", "message", "Você foi convidado para um julgamento! Você tem " + Convert.ToInt32(RoleplayData.GetData("court", "invitationtime")) + " minuto(s) para ir até lá!"));
                }
            }

            Session.SendWhisper("Você solicitou um julgamento. Por favor, espere " + Convert.ToInt32(RoleplayData.GetData("court", "invitationtime")) + " minuto(s), o Juíz está convidando 50 cidadões aleatórios para participar!", 1);

            Session.GetRoleplay().Trialled = true;
            RoleplayManager.CourtTrialIsStarting = true;
            RoleplayManager.Defendant = Session;
            RoleplayManager.TimerManager.CreateTimer("juryinvitation", 1000, false);

            #endregion
        }
    }
}