using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class UnFreezeRoomCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_freeze_room_undo"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Descongela todos os usuários congelados na sala atual!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Room.GetRoomUserManager().GetUserList().ToList().Count <= 1)
            {
                Session.SendWhisper("Você é a única pessoa na sala!", 1);
                return;
            }

            int count = 0;
            foreach (RoomUser User in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                    continue;

                if (!User.Frozen)
                    continue;

                count++;
                User.Frozen = false;

                if (User.CurrentEffect == 12)
                    User.ApplyEffect(0);

                User.GetClient().SendWhisper("Você foi descongelado por " + Session.GetHabbo().Username + "!", 1);
            }

            if (count > 0)
            {
                Session.Shout("*Descongela imediatamente todos do quarto*", 23);
                return;
            }
            else
            {
                Session.SendWhisper("Não existe usuários congelados neste quarto", 1);
                return;
            }
        }
    }
}
