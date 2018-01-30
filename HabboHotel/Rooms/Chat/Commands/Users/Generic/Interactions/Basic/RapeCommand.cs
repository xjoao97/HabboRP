using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using System.Drawing;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Basic
{
    class RapeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_rape"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Estupra o usuário."; }
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

            if (Session.GetRoleplay().TryGetCooldown("estuprar"))
                return;

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("Você não pode estuprar alguém que está morto!", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode estuprar alguém que está ausente!!", 1);
                return;
            }

            if (TargetClient == Session)
            {
                Session.SendWhisper("Você não pode estuprar você mesmo!", 1);
                return;
            }

            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                if (!Session.GetRoleplay().WantedFor.Contains("assédio sexual"))
                    Session.GetRoleplay().WantedFor = Session.GetRoleplay().WantedFor + "assédio sexual[estupro], ";

                Session.Shout("*Estupra " + TargetClient.GetHabbo().Username + "*", 4);
                Session.GetRoleplay().CooldownManager.CreateCooldown("estuprar", 1000, ((Session.GetRoleplay().IsJailed) ? 20 : 8));
                RoomUser.ApplyEffect(EffectsList.Twinkle);
                TargetUser.ApplyEffect(EffectsList.Twinkle);
                Session.GetRoleplay().RapeTimer = 5;
                TargetClient.GetRoleplay().RapeTimer = 5;
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para estupra-lo!", 1);
                return;
            }
            #endregion
        }
    }
}