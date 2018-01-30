using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using System;


namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    internal class EventAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_hotel_alert_event";
            }
        }
        public string Parameters
        {
            get
            {
                return "%info preço%";
            }
        }
        public string Description
        {
            get
            {
                return "Envie um alerta de hotel para o seu evento!";
            }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem de prêmio!", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer(Session, "event", Message));
            return;
        }
    }
}
