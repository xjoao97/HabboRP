using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class DanceCommand :IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_dance"; }
        }

        public string Parameters
        {
            get { return "%id_danca%"; }
        }

        public string Description
        {
            get { return "Começa a dançar com base no id da dança."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser ThisUser = Session.GetRoomUser();
            if (ThisUser == null)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma ID de uma dança.", 1);
                return;
            }

            int DanceId;
            if (int.TryParse(Params[1], out DanceId))
            {
                if (DanceId > 4 || DanceId < 0)
                {
                    Session.SendWhisper("O ID da dança deve estar entre 1 e 4!", 1);
                    return;
                }

                Session.GetHabbo().CurrentRoom.SendMessage(new DanceComposer(ThisUser, DanceId));
            }
            else
                Session.SendWhisper("Digite uma ID de dança válida.", 1);
        }
    }
}
