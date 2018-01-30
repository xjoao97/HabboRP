using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class RegenMapsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_general_regen_maps"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "O mapa do jogo do seu quarto está quebrado? Conserte-o com este comando!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Room.GetGameMap().GenerateMaps();
            Session.SendWhisper("O mapa do jogo desta sala foi re-gerado com sucesso.", 1);
        }
    }
}
