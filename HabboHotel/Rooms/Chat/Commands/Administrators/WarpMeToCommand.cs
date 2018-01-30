using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class WarpMeToCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_warp_me_to"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Teleporte-se para outro usuário."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja amarrar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Você não pode se ir para sí mesmo!", 1);
                return;
            }

            var Point = new System.Drawing.Point(Session.GetRoomUser().X, Session.GetRoomUser().Y);
            var TargetPoint = new System.Drawing.Point(TargetUser.X, TargetUser.Y);

            if (Point == TargetPoint)
            {
                Session.SendWhisper("Você já está encima deste usuário!", 1);
                return;
            }

            Session.GetRoomUser().ClearMovement(true);

            if (Session.GetRoomUser().TeleportEnabled)
                Session.GetRoomUser().MoveTo(TargetPoint);
            else
            {
                Session.GetRoomUser().TeleportEnabled = true;
                Session.GetRoomUser().MoveTo(TargetPoint);
                Session.GetRoomUser().TeleportEnabled = false;
            }
            Session.Shout("*Teleporta para cima de " + TargetClient.GetHabbo().Username + "*", 23);
            return;
        }
    }
}