using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class HALCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hotel_alert_link"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envie uma mensagem para todo o hotel, com um link."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 2)
            {
                Session.SendWhisper("Digite uma mensagem e um URL para enviar.", 1);
                return;
            }

            string URL = Params[1];

            string Message = CommandManager.MergeParams(Params, 2);
            PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer("Alerta do Hotel!", Message + "\r\n" + "- " + Session.GetHabbo().Username, "", "Clique aqui!", URL));
            return;
        }
    }
}
