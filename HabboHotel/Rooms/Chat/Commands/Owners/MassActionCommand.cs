using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class MassActionCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_action_mass"; }
        }

        public string Parameters
        {
            get { return "%actionid%"; }
        }

        public string Description
        {
            get { return "Forçar todos na sala a fazer uma ação."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira uma ID de ação válida.", 1);
                return;
            }

            int ActionId;
            if (!int.TryParse(Params[1], out ActionId))
            {
                Session.SendWhisper("Insira uma ID de ação válida.", 1);
                return;
            }

            if (ActionId == 5)
            {
                Session.SendWhisper("Desculpe, você não pode forçar todos fazerem uma ação!", 1);
                return;
            }

            List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();

            if (Users.Count <= 1)
            {
                Session.SendWhisper("Você é a única pessoa na sala!", 1);
                return;
            }

            foreach (RoomUser U in Users.ToList())
            {
                if (U == null)
                    continue;

                if (U.CarryItemID > 0)
                    U.CarryItemID = 0;

                if (U.DanceId > 0)
                    U.DanceId = 0;

                if (U.CurrentEffect > 0)
                    U.ApplyEffect(0);

                Room.SendMessage(new ActionComposer(U.VirtualId, ActionId));
            }

            Session.Shout("*Obriga imediatamente todos a realizar uma ação*", 23);
        }
    }
}
