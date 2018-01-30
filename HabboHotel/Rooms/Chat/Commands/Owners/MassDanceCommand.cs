using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class MassDanceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_dance_mass"; }
        }

        public string Parameters
        {
            get { return "%danca_id%"; }
        }

        public string Description
        {
            get { return "Força todos na sala a fazer uma dança de sua escolha."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira um ID de dança válida. (1-4)", 1);
                return;
            }

            int DanceId;
            if (!int.TryParse(Params[1], out DanceId))
            {
                Session.SendWhisper("Insira um ID de dança válida. (1-4)", 1);
                return;
            }

            if (DanceId < 0 || DanceId > 4)
            {
                Session.SendWhisper("Insira um ID de dança válida. (1-4)", 1);
                return;
            }

            List<RoomUser> Users = Room.GetRoomUserManager().GetRoomUsers();

            if (Users.Count <= 1)
            {
                Session.SendWhisper("Você é a única pessoa no quarto!", 1);
                return;
            }

            foreach (RoomUser U in Users.ToList())
            {
                if (U == null)
                    continue;

                if (U.CarryItemID > 0)
                    U.CarryItemID = 0;

                if (U.CurrentEffect > 0)
                    U.ApplyEffect(0);

                U.DanceId = DanceId;
                Room.SendMessage(new DanceComposer(U, DanceId));
            }

            Session.Shout("*Força todos usuários a dançar*", 23);
        }
    }
}
