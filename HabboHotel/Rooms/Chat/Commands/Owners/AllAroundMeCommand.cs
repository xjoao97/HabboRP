using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class AllAroundMeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_all_around_me"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Puxa todos os usuários da sala para você!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();
            foreach (RoomUser U in Users.ToList())
            {
                if (U == null || Session.GetHabbo().Id == U.UserId)
                    continue;

                U.MoveTo(User.SquareInFront.X, User.SquareInFront.Y, true);
            }

            Session.Shout("*Puxa imediatamente todas as pessoas do quarto para minha frente*", 23);
        }
    }
}
