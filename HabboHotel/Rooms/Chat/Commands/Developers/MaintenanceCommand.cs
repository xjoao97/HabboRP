using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Availability;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class MaintenanceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_maintenance"; }
        }

        public string Parameters
        {
            get { return "%minutos% %duração%"; }
        }

        public string Description
        {
            get { return "Coloque o hotel em manutenção por um período específico de minutos e uma quantidade específica de duração para quando está de volta."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {


            if (Params.Length < 3)
            {
                Session.SendWhisper("Opa, você deve selecionar minuto(s) de duração.", 1);
                return;
            }

            
            int Minutes = Convert.ToInt32(Params[1]);
            int Duration = Convert.ToInt32(Params[2]);

            PlusEnvironment.GetGame().GetClientManager().SendMessage(new MaintenanceStatusComposer(Minutes, Duration));
            Session.SendWhisper("Sucesso, o hotel cairá em " + Minutes + " minuto(s) e vai estar online de volta daqui à " + Duration + " minuto(s)!");
        }
    }
}
