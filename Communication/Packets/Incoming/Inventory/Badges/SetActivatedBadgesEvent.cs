using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Quests;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;


namespace Plus.Communication.Packets.Incoming.Inventory.Badges
{
    class SetActivatedBadgesEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Session.GetHabbo().GetBadgeComponent().ResetSlots();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_badges` SET `badge_slot` = '0' WHERE `user_id` = '" + Session.GetHabbo().Id + "'");
            }

            for (int i = 0; i < 5; i++)
            {
                int Slot = Packet.PopInt();
                string Badge = Packet.PopString();

                if (Badge.Length == 0)
                    continue;

                if (!Session.GetHabbo().GetBadgeComponent().HasBadge(Badge) || Slot < 1 || Slot > 5)
                    return;

                Session.GetHabbo().GetBadgeComponent().GetBadge(Badge).Slot = Slot;

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `user_badges` SET `badge_slot` = " + Slot + " WHERE `badge_id` = @badge AND `user_id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                    dbClient.AddParameter("badge", Badge);
                    dbClient.RunQuery();
                }
            }

            //PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_BADGE);

            Room Room;

            if (Session.GetHabbo().InRoom && PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                Session.GetHabbo().CurrentRoom.SendMessage(new HabboUserBadgesComposer(Session.GetHabbo()));
            else
                Session.SendMessage(new HabboUserBadgesComposer(Session.GetHabbo()));
        }
    }
}
