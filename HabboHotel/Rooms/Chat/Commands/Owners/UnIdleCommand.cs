using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class UnIdleCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_unidle"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Força um usuário a não ficar ausente."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de escolher um usuário!");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (User == null)
                return;

            if (User.IsAsleep)
            {
                User.UnIdle(true);
                Session.Shout("*Força o usuário " + User.GetUsername() + " acordar*", 23);
                User.GetClient().SendWhisper("Você foi forçado a acordar por " + Session.GetHabbo().Username + "!", 1);
                return;
            }
            else
            {
                Session.SendWhisper("Este usuário não está ausente!", 1);
                return;
            }
        }
    }
}
