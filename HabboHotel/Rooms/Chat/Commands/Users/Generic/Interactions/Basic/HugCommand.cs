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
    class HugCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_hug"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Abrace o usuário."; }
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

            if (Session.GetRoleplay().TryGetCooldown("abracar"))
                return;

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("Você não pode abraçar alguém que está ausente!", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

            if (Distance <= 1)
            {
                Session.Shout("*Agarra " + TargetClient.GetHabbo().Username + ", dando-lhe um grande abraço*", 12);
                Session.GetRoleplay().CooldownManager.CreateCooldown("abracar", 1000, 5);
                RoomUser.ApplyEffect(EffectsList.Love);
                TargetUser.ApplyEffect(EffectsList.Love);
                Session.GetRoleplay().HugTimer = 5;
                TargetClient.GetRoleplay().HugTimer = 5;
                return;
            }
            else
            {
                Session.SendWhisper("Você deve se aproximar desse cidadão para abraçá-lo!", 1);
                return;
            }
            #endregion
        }
    }
}