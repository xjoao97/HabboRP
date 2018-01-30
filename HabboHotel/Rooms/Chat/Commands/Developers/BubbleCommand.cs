using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class BubbleCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_bubble"; }
        }

        public string Parameters
        {
            get { return "%id%"; }
        }

        public string Description
        {
            get { return "Use uma bolha personalizada para conversar!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um ID de bolha!", 1);
                return;
            }

            int Bubble = 0;
            if (!int.TryParse(Params[1].ToString(), out Bubble))
            {
                Session.SendWhisper("Por favor insira um número válido.", 1);
                return;
            }

            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Bubble, out Style) || (Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight)))
            {
                Session.SendWhisper("Opa, você não pode usar esta bolha!", 1);
                return;
            }

            User.LastBubble = Bubble;
            Session.GetHabbo().CustomBubbleId = Bubble;
            Session.SendWhisper("Bolha configurada para: " + Bubble);
        }
    }
}