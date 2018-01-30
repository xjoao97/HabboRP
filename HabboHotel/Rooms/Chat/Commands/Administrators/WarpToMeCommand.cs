using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class WarpToMeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_warp_to_me"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Teleporta um usuário para você."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor, insira o usuário que deseja teleportar para você", 1);
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
                Session.SendWhisper("Você não pode fazer isso em você mesmo!", 1);
                return;
            }

            var Point = new System.Drawing.Point(Session.GetRoomUser().X, Session.GetRoomUser().Y);
            var TargetPoint = new System.Drawing.Point(TargetUser.X, TargetUser.Y);

            if (Point == TargetPoint)
            {
                Session.SendWhisper("Esta pessoa já está em cima de você!", 1);
                return;
            }

            TargetUser.ClearMovement(true);

            if (TargetUser.TeleportEnabled)
                TargetUser.MoveTo(Point);
            else
            {
                TargetUser.TeleportEnabled = true;
                TargetUser.MoveTo(Point);
                TargetUser.TeleportEnabled = false;
            }
            Session.Shout("*Teleporta o usuário " + TargetClient.GetHabbo().Username + " para mim*", 23);
            return;
        }
    }
}