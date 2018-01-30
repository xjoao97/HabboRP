using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    class AmbassadorWarningEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().GetPermissions().HasRight("ambassador"))
                return;

            int UserId = Packet.PopInt();

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client == null)
                return;
            else if (Client.GetHabbo().GetPermissions().HasRight("mod_tool"))
                return;

            Client.SendMessage(new RoomNotificationComposer("${notification.ambassador.alert.warning.title}", "${notification.ambassador.alert.warning.message}", "", "ok", "event:"));
        }
    }
}
