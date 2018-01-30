using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Marriage
{
    class SexCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_sex"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Faça sexo com seu parceiro casado"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
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

            if (Session.GetRoleplay().TryGetCooldown("sexo"))
                return;

            if (Session.GetRoleplay().MarriedTo == 0)
            {
                Session.SendWhisper("Você não pode completar esta ação quando não é casado(a)!", 1);
                return;
            }

            if (Session.GetRoleplay().MarriedTo != TargetClient.GetHabbo().Id)
            {
                Session.SendWhisper("Você só pode fazer sexo com sua esposa/seu esposo!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode fazer sexo com alguém que está morto!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Você não pode fazer sexo com alguém enquanto estiver dentro de um evento!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode fazer sexo com alguém está ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                if (Session.GetHabbo().Gender.ToLower().StartsWith("m") && TargetClient.GetHabbo().Gender.ToLower().StartsWith("f"))
                {
                    Session.Shout("*Agarra " + TargetClient.GetHabbo().Username + " pelos quadris e deixe-o no chão, tirando suas roupas*", 16);
                    RoleplayManager.Shout(TargetClient, "*Gemidos de " + Session.GetHabbo().Username + " quando empurra seu pênis dentro dela*", 16);
                }
                else if (Session.GetHabbo().Gender.ToLower().StartsWith("f") && TargetClient.GetHabbo().Gender.ToLower().StartsWith("m"))
                {
                    Session.Shout("*Pushes " + TargetClient.GetHabbo().Username + " down onto the floor, climbing ontop of him and sliding his clothes off*", 16);
                    RoleplayManager.Shout(TargetClient, "*Gemidos de " + Session.GetHabbo().Username + " quando desliza o dedo dentro dela*", 16);
                }
                else if (Session.GetHabbo().Gender.ToLower().StartsWith("m") && TargetClient.GetHabbo().Gender.ToLower().StartsWith("m"))
                {
                    Session.Shout("*Agarra " + TargetClient.GetHabbo().Username + " pelo peito e abate-o, tirando a roupa*", 16);
                    RoleplayManager.Shout(TargetClient, "*Gemidos de prazer de " + Session.GetHabbo().Username + " saltando para cima e para baixo seu pau*", 16);
                }
                else if (Session.GetHabbo().Gender.ToLower().StartsWith("f") && TargetClient.GetHabbo().Gender.ToLower().StartsWith("f"))
                {
                    Session.Shout("*Empurra " + TargetClient.GetHabbo().Username + " descendo no chão, subindo sobre ela e deslizando as roupas dela*", 16);
                    RoleplayManager.Shout(TargetClient, "*Gemidos de " + Session.GetHabbo().Username + " quando os dedos deslizam dentro dela*", 16);
                }
                else
                {
                    Session.Shout("*Faz amor doce com " + TargetClient.GetHabbo().Username + "*", 16);
                    RoleplayManager.Shout(TargetClient, "*Gemidos dos movimentos de " + Session.GetHabbo().Username + "*", 16);
                }
                Session.GetRoleplay().CooldownManager.CreateCooldown("sexo", 1000, 60);
                RoomUser.ApplyEffect(507);
                TargetUser.ApplyEffect(507);
                Session.GetRoleplay().SexTimer = 15;
                TargetClient.GetRoleplay().SexTimer = 15;
                RoleplayManager.GetLookAndMotto(Session);
                RoleplayManager.GetLookAndMotto(TargetClient);
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para fazer sexo com ele!", 1);
                return;
            }
            #endregion
        }
    }
}