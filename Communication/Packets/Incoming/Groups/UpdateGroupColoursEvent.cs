using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

using Plus.Database.Interfaces;


namespace Plus.Communication.Packets.Incoming.Groups
{
    class UpdateGroupColoursEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int Colour1 = Packet.PopInt();
            int Colour2 = Packet.PopInt();

            Group Group = null;

            if (GroupId < 1000)
                Group = GroupManager.GetJob(GroupId);
            else
                Group = GroupManager.GetGang(GroupId);

            if (Group == null)
                return;
          
            if (Group.CreatorId != Session.GetHabbo().Id && !Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Group.Id < 1000)
                    dbClient.SetQuery("UPDATE `rp_jobs` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` = '" + Group.Id + "'");
                else
                    dbClient.SetQuery("UPDATE `rp_gangs` SET `colour1` = @colour1, `colour2` = @colour2 WHERE `id` = '" + Group.Id + "'");
                dbClient.AddParameter("colour1", Colour1);
                dbClient.AddParameter("colour2", Colour2);
                dbClient.RunQuery();
            }

            Group.Colour1 = Colour1;
            Group.Colour2 = Colour2;

            Session.SendMessage(new GroupInfoComposer(Group, Session));
            if (Session.GetHabbo().CurrentRoom != null)
            {
                foreach (Item Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor.ToList())
                {
                    if (Item == null || Item.GetBaseItem() == null)
                        continue;

                    if (Item.GetBaseItem().InteractionType != InteractionType.GUILD_ITEM && Item.GetBaseItem().InteractionType != InteractionType.GUILD_GATE || Item.GetBaseItem().InteractionType != InteractionType.GUILD_FORUM)
                        continue;

                    Session.GetHabbo().CurrentRoom.SendMessage(new ObjectUpdateComposer(Item, Convert.ToInt32(Item.UserID)));
                }
            }
        }
    }
}
