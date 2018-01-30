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
    class ArrestCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_arrest"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Prende o usuário alvo dependendo do nível de procurado."; }
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

            if (!GroupManager.HasJobCommand(Session, "arrest"))
            {
                Session.SendWhisper("Apenas um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode prender alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed && !TargetClient.GetRoleplay().Jailbroken)
            {
                Session.SendWhisper("Você não pode prender alguém que está preso!", 1);
                return;
            }

            if (!TargetClient.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("Você não pode prender alguém que não está algemado!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode prender alguém que está ausente!", 1);
                return;
            }

            if (!TargetClient.GetRoleplay().Jailbroken)
            {
                if (!RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id))
                {
                    Session.SendWhisper("Este usuário não é procurado!", 1);
                    return;
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

                Session.Shout("*Algema " + TargetClient.GetHabbo().Username + " e envia para a prisão por " + WantedTime + " minutos*", 37);
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
                    TargetClient.GetRoleplay().TimerManager.CreateTimer("preso", 1000, false);
                }

                if (TargetClient.GetRoleplay().Jailbroken && !JailbreakManager.FenceBroken)
                    TargetClient.GetRoleplay().Jailbroken = false;

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

                PlusEnvironment.GetGame().GetClientManager().JailAlert("[Alerta RÁDIO] " + TargetClient.GetHabbo().Username + " acabou de ser preso por " + Session.GetHabbo().Username + "! Bom trabalho.");
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Arrests", 1);
                Session.GetRoleplay().Arrests++;
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(TargetClient, "ACH_Arrested", 1);
                TargetClient.GetRoleplay().Arrested++;
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para prendê-lo!", 1);
                return;
            }
            #endregion
        }

        public void ExecuteBot(GameClient Session, RoomUser Bot, Room Room)
        {
            if (!Bot.GetBotRoleplay().Jailed)
            {
                Session.SendWhisper("Desculpe, mas " + Bot.GetBotRoleplay().Name + " não está preso!", 1);
                return;
            }

            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(Session.GetRoomUser().Coordinate, Bot.Coordinate);
            Wanted Wanted = RoleplayManager.WantedList.ContainsKey(Bot.GetBotRoleplay().Id) ? RoleplayManager.WantedList[Bot.GetBotRoleplay().Id] : null;
            int WantedTime = Wanted == null ? 5 : Wanted.WantedLevel * 5;

            if (Distance <= 1)
            {
                // cba rn
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar de " + Bot.GetBotRoleplay().Name + "para prendê-lo!", 1);
                return;
            }
        }
    }
}