using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class AdminJailCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_jail"; }
        }

        public string Parameters
        {
            get { return "%usuário% %estrelas%"; }
        }

        public string Description
        {
            get { return "Prende um cidadão para um número selecionado de estrelas."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário ou o número de estrelas de 1-6!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            var RoomUser = Session.GetRoomUser();
            var TargetRoomUser = TargetClient.GetRoomUser();

            if (RoomUser == null || TargetRoomUser == null)
                return;

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Esta pessoa já está presa!", 1);
                return;
            }

            int Stars;
            if (!int.TryParse(Params[2], out Stars))
            {
                Session.SendWhisper("Digite um número de estrelas válido de 1-6", 1);
                return;
            }
            else
            {
                if (Stars < 1 || Stars > 6)
                {
                    Session.SendWhisper("Digite um número de estrelas válido de 1-6", 1);
                    return;
                }
            }

            int WantedTime = Stars * 5;
            TargetClient.GetRoleplay().WantedLevel = Stars;
            if (TargetClient.GetRoleplay().IsDead)
            {
                TargetClient.GetRoleplay().IsDead = false;
                TargetClient.GetRoleplay().ReplenishStats(true);
                TargetClient.GetHabbo().Poof();
            }

            if (TargetClient.GetRoleplay().IsWorking)
            {
                WorkManager.RemoveWorkerFromList(TargetClient);
                TargetClient.GetRoleplay().IsWorking = false;
                TargetClient.GetHabbo().Poof();
            }

            if (TargetClient.GetRoleplay().Cuffed)
                TargetClient.GetRoleplay().Cuffed = false;

            if (TargetClient.GetRoleplay().OnProbation)
                TargetClient.GetRoleplay().OnProbation = false;

            if (TargetRoomUser.Frozen)
                TargetRoomUser.Frozen = false;

            TargetClient.GetRoleplay().IsJailed = true;
            TargetClient.GetRoleplay().JailedTimeLeft = WantedTime;
            TargetClient.GetRoleplay().TimerManager.CreateTimer("jail", 1000, false);

            int JailRID = Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid"));

            if (TargetClient.GetHabbo().CurrentRoomId == JailRID)
            {
                RoleplayManager.GetLookAndMotto(TargetClient);
                RoleplayManager.SpawnBeds(TargetClient, "bed_silo_one");
                TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Você foi preso por " + Session.GetHabbo().Username + " por " + WantedTime + " minutos!"));
            }
            else
            {
                TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Você foi preso por " + Session.GetHabbo().Username + " por " + WantedTime + " minutos!"));
                RoleplayManager.SendUser(TargetClient, JailRID);
            }

            if (RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id))
            {
                Wanted Junk;
                RoleplayManager.WantedList.TryRemove(TargetClient.GetHabbo().Id, out Junk);
                PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] " + TargetClient.GetHabbo().Username + " acabou de ser preso por " + Session.GetHabbo().Username + "! Bom trabalho.");
            }

            Session.Shout("*Prende imediatamente o " + TargetClient.GetHabbo().Username + " por " + WantedTime + " minutos*", 23);
            return;
        }
    }
}