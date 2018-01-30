using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Owners
{
    class MassEnableCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_enable_mass"; }
        }

        public string Parameters
        {
            get { return "%efeito_id%"; }
        }

        public string Description
        {
            get { return "Dê a cada usuário na sala uma habilitação de ID específica."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite um ID de efeito.", 1);
                return;
            }

            int EnableId = 0;
            if (!int.TryParse(Params[1], out EnableId))
            {
                Session.SendWhisper("Digite um ID de efeito.", 1);
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
                if (U == null || U.RidingHorse)
                    continue;

                if (U.CarryItemID > 0)
                    U.CarryItem(0);

                if (U.DanceId > 0)
                    U.DanceId = 0;

                U.ApplyEffect(EnableId);
            }
        
            Session.Shout("*Altera imediatamente todos os efeitos dos usuários da sala*", 23);
        }
    }
}
