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
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Hospital
{
    class DischargeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hospital_discharge"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Revive o cidadão do hospital se eles estão mortos."; }
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
                RoomUser Bot = Room.GetRoomUserManager().GetBotByName(Params[1]);

                if (Bot != null && Bot.GetBotRoleplay() != null)
                {
                    ExecuteBot(Session, Bot, Room);
                    return;
                }

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

            if (!GroupManager.HasJobCommand(Session, "discharge"))
            {
                Session.SendWhisper("Somente um trabalhador do Hospital pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (!TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode reviver alguém que não está morto!", 1);
                return;
            }
            #endregion

            #region Execute
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(Session.GetRoomUser().Coordinate, TargetClient.GetRoomUser().Coordinate);

            if (Distance <= 5)
            {
                Session.Shout("*Revive " + TargetClient.GetHabbo().Username + " da cama de hospital*", 4);
                TargetClient.GetRoleplay().IsDead = false;
                TargetClient.GetRoleplay().DeadTimeLeft = 0;

                if (Session.GetRoleplay().LastKilled != TargetClient.GetHabbo().Id)
                {
                    int Amount = 0;

                    if (Session.GetRoleplay().Level <= 5)
                        Amount = 1;
                    else if (Session.GetRoleplay().Level > 5 && Session.GetRoleplay().Level <= 10)
                        Amount = 2;
                    else if (Session.GetRoleplay().Level > 10 && Session.GetRoleplay().Level <= 15)
                        Amount = 3;
                    else
                        Amount = 4;

                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Discharging", 1);

                    Session.GetRoleplay().LastKilled = TargetClient.GetHabbo().Id;

                    if (!Room.HitEnabled && !Room.ShootEnabled)
                    {
                        Session.GetHabbo().Credits += Amount;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Session.SendWhisper("Você ganhou R$" + Amount + " extra por reviver " + TargetClient.GetHabbo().Username + "!", 1);
                    }
                }
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para revive-lo!", 1);
                return;
            }
            #endregion
        }

        public void ExecuteBot(GameClient Session, RoomUser Bot, Room Room)
        {
            if (!Bot.GetBotRoleplay().Dead)
            {
                Session.SendWhisper("Desculpe, mas " + Bot.GetBotRoleplay().Name + " não está morto!", 1);
                return;
            }

            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(Session.GetRoomUser().Coordinate, Bot.Coordinate);

            if (Distance <= 5)
            {
                if (Bot.GetBotRoleplay().TimerManager.ActiveTimers.ContainsKey("botdeath"))
                    Bot.GetBotRoleplay().TimerManager.ActiveTimers["botdeath"].EndTimer();

                if (Bot.Frozen)
                    Bot.Frozen = false;

                Session.Shout("*Revive " + Bot.GetBotRoleplay().Name + " da cama de hospital*", 4);
                RoleplayManager.SpawnChairs(null, "val14_wchair", Bot);

                Bot.GetBotRoleplay().Dead = false;
                Room.SendMessage(new UsersComposer(Bot));

                if (Bot.GetBotRoleplay().RoamBot)
                    Bot.GetBotRoleplay().MoveRandomly();

                if (Session.GetRoleplay().LastKilled != (RoleplayBotManager.BotFriendMultiplyer + Bot.GetBotRoleplay().Id))
                {
                    int Amount = 0;

                    if (Session.GetRoleplay().Level <= 5)
                        Amount = 1;
                    else if (Session.GetRoleplay().Level > 5 && Session.GetRoleplay().Level <= 10)
                        Amount = 2;
                    else if (Session.GetRoleplay().Level > 10 && Session.GetRoleplay().Level <= 15)
                        Amount = 3;
                    else
                        Amount = 4;

                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Discharging", 1);

                    Session.GetRoleplay().LastKilled = (RoleplayBotManager.BotFriendMultiplyer + Bot.GetBotRoleplay().Id);

                    if (!Room.HitEnabled && !Room.ShootEnabled)
                    {
                        Session.GetHabbo().Credits += Amount;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Session.SendWhisper("Você ganhou R$" + Amount + " extra por reviver " + Bot.GetBotRoleplay().Name + "!", 1);
                    }
                }
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar de " + Bot.GetBotRoleplay().Name + " para revive-lo!", 1);
                return;
            }
        }
    }
}