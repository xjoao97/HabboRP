using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class WarpAllToMeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_warp_all_to_me"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Teleporta todos os usuários da sala para você."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            var Point = new System.Drawing.Point(Session.GetRoomUser().X, Session.GetRoomUser().Y);

            int count = 0;
            List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();
            foreach (RoomUser U in Users.ToList())
            {
                if (U == null || Session.GetHabbo().Id == U.UserId)
                    continue;

                var TargetPoint = new System.Drawing.Point(U.X, U.Y);

                if (Point == TargetPoint)
                    continue;

                U.ClearMovement(true);

                if (U.TeleportEnabled)
                    U.MoveTo(Point);
                else
                {
                    U.TeleportEnabled = true;
                    U.MoveTo(Point);
                    U.TeleportEnabled = false;
                }
                count++;
            }

            if (count > 0)
                Session.Shout("*Amarra imediatamente todos usuários da sala para em mim*", 23);
            else
                Session.SendWhisper("Não há mais ninguém na sala para puxar!", 1);
            return;
        }
    }
}