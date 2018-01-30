using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class EscortCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_related_escort"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Acompanha o usuário do algemado alvo dependendo do nível desejado."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode escoltar alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed && !TargetClient.GetRoleplay().Jailbroken)
            {
                Session.SendWhisper("Você não pode escoltar alguém que já está preso!", 1);
                return;
            }

            if (!TargetClient.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("Você não pode escoltar alguém que não está algemado!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode escoltar alguém que está ausente!", 1);
                return;
            }

            if (TargetClient == Session)
            {
                Session.SendWhisper("Você não pode se escoltar!", 1);
                return;
            }

            if (!TargetClient.GetRoleplay().Jailbroken)
            {
                if (!RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id))
                {
                    if (TargetClient.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("probation"))
                        TargetClient.GetRoleplay().TimerManager.ActiveTimers["probation"].EndTimer();

                    TargetClient.GetRoleplay().IsWanted = true;
                    TargetClient.GetRoleplay().WantedLevel = 1;
                    TargetClient.GetRoleplay().WantedTimeLeft = 10;

                    TargetClient.GetRoleplay().TimerManager.CreateTimer("procurado", 1000, false);
                    RoleplayManager.WantedList.TryUpdate(TargetClient.GetHabbo().Id, new Wanted(Convert.ToUInt32(TargetClient.GetHabbo().Id), Room.Id.ToString(), 1), RoleplayManager.WantedList[TargetClient.GetHabbo().Id]);
                }
            }

            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);
            Wanted Wanted = RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id) ? RoleplayManager.WantedList[TargetClient.GetHabbo().Id] : null;
            int WantedTime = Wanted == null ? 6 : Wanted.WantedLevel * 5;

            if (Distance <= 1)
            {
                if (TargetClient.GetRoleplay().IsWorking)
                {
                    WorkManager.RemoveWorkerFromList(TargetClient);
                    TargetClient.GetRoleplay().IsWorking = false;
                    TargetClient.GetHabbo().Poof();
                }

                Session.Shout("*Algema as mãos de " + TargetClient.GetHabbo().Username + ", coloca as algemas e prende por " + WantedTime + " minutos*", (GroupManager.HasJobCommand(Session, "guide") && Session.GetRoleplay().IsWorking ? 37 : 4));
                TargetClient.GetRoleplay().Cuffed = false;
                TargetClient.GetRoomUser().ApplyEffect(0);

                if (TargetClient.GetHabbo().Look.Contains("lg-78322"))
                {
                    if (!TargetClient.GetRoleplay().WantedFor.Contains("exposição indecente"))
                        TargetClient.GetRoleplay().WantedFor = TargetClient.GetRoleplay().WantedFor + "exposição indecente, ";
                }

                if (TargetUser.Frozen)
                    TargetUser.Frozen = false;

                if (!TargetClient.GetRoleplay().IsJailed)
                {
                    TargetClient.GetRoleplay().IsJailed = true;
                    TargetClient.GetRoleplay().JailedTimeLeft = WantedTime;
                    TargetClient.GetRoleplay().TimerManager.CreateTimer("jail", 1000, false);
                }

                if (TargetClient.GetRoleplay().Jailbroken && !JailbreakManager.FenceBroken)
                    TargetClient.GetRoleplay().Jailbroken = false;

                int JailRID = Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid"));

                if (TargetClient.GetHabbo().CurrentRoomId == JailRID)
                {
                    RoleplayManager.GetLookAndMotto(TargetClient);
                    RoleplayManager.SpawnBeds(TargetClient, "bed_silo_one");
                    TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Você foi escoltado por " + Session.GetHabbo().Username + " por " + WantedTime + " minutos!"));
                }
                else
                {
                    TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Você foi escoltado por " + Session.GetHabbo().Username + " por " + WantedTime + " minutos!"));
                    RoleplayManager.SendUser(TargetClient, JailRID);
                }

                PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] " + TargetClient.GetHabbo().Username + " acabou de ser escoltado para a prisão por " + Session.GetHabbo().Username + "!");
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Arrests", 1);
                Session.GetRoleplay().Arrests++;
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(TargetClient, "ACH_Arrested", 1);
                TargetClient.GetRoleplay().Arrested++;
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para escoltá-lo!", 1);
                return;
            }
            #endregion
        }
    }
}