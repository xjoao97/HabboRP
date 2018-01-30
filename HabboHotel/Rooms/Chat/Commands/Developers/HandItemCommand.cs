using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class HandItemCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_handitem"; }
        }

        public string Parameters
        {
            get { return "%item%"; }
        }

        public string Description
        {
            get { return "Permite que você carregue um item de mão."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            int ItemId = 0;
            if (!int.TryParse(Convert.ToString(Params[1]), out ItemId))
            {
                Session.SendWhisper("Digite um item válido.", 1);
                return;
            }

            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            User.CarryItem(ItemId);
        }
    }
}
