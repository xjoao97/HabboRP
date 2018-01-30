using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class AllEyesOnMeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_all_eyes_on_me"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Faz com que todos os usuários nos do quartos olhem para você."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();
            foreach (RoomUser U in Users.ToList())
            {
                if (U == null || Session.GetHabbo().Id == U.UserId)
                    continue;

                U.SetRot(Rotation.Calculate(U.X, U.Y, ThisUser.X, ThisUser.Y), false);
            }

            Session.Shout("*Faz com que todos olhem para mim*", 23);
        }
    }
}
