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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Hospital
{
    class HealCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hospital_heal"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Começa a curar o cidadão-alvo."; }
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

            if (TargetClient.GetRoleplay() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().TimerManager == null)
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

            if (!GroupManager.HasJobCommand(Session, "heal"))
            {
                Session.SendWhisper("Somente um funcionário do hospital pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode recuperar o HP de alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("Você não pode curar alguém que está na prisão!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().BeingHealed)
            {
                Session.SendWhisper("Você não pode curar alguém que já está sendo curado!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().CurHealth >= TargetClient.GetRoleplay().MaxHealth)
            {
                Session.SendWhisper("Este cidadão já está com o sangue cheio!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Hunger >= 100)
            {
                Session.SendWhisper("Este cidadão está com muita fome para curar! Diga para ele comer algum alimento.", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode curar alguém que está ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 5)
            {
                Session.Shout("*Começa a curar as feridas de " + TargetClient.GetHabbo().Username + "*", 4);
                TargetClient.GetRoomUser().ApplyEffect(0);
                TargetClient.GetRoleplay().BeingHealed = true;
                TargetClient.GetRoleplay().TimerManager.CreateTimer("heal", 1000, false);

                if (Session.GetRoleplay().LastKilled != TargetClient.GetHabbo().Id && TargetClient.GetHabbo().Id != Session.GetHabbo().Id)
                {
                    int Amount = 0;

                    if (Session.GetRoleplay().Level <= 10)
                        Amount = 1;
                    else
                        Amount = 2;

                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Healing", 1);

                    Session.GetRoleplay().LastKilled = TargetClient.GetHabbo().Id;

                    if (!Room.HitEnabled)
                    {
                        Session.GetHabbo().Credits += Amount;
                        Session.GetHabbo().UpdateCreditsBalance();
                        Session.SendWhisper("Você ganhou R$" + Amount + " extra por cura " + TargetClient.GetHabbo().Username + "!", 1);
                    }
                }

                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para curá-lo!", 1);
                return;
            }
            #endregion
        }
    }
}