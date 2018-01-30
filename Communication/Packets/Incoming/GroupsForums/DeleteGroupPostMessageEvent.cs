using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class DeleteGroupPostMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int groupId = Packet.PopInt();
            int parentId = Packet.PopInt();
            int index = Packet.PopInt() + 1;
            int StateToSet = Packet.PopInt();

            Group group = GroupManager.GetJob(groupId);
            bool IsAdmin = false;
            if (group.IsAdmin(Session.GetHabbo().Id) || Session.GetHabbo().GetPermissions().HasRight("corporation_rights") || group.CreatorId == Session.GetHabbo().Id || Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
                IsAdmin = true;

            if (!IsAdmin)
                return;

            if (group == null || !group.ForumEnabled)
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE parent_id = {0} ORDER BY id", parentId));
                DataTable Table = dbClient.getTable();

                int t = 0;

                foreach (DataRow Row in Table.Rows)
                {
                    t++;
                    if (t == index)
                    {
                        string state = "0";
                        if (StateToSet == 20 || StateToSet == 10)
                            state = "1";
                        
                        dbClient.RunQuery("UPDATE `groups_forums_posts` SET `hidden` = @hid WHERE id = @id");
                        dbClient.AddParameter("id", Convert.ToInt32(Row["id"]));
                        dbClient.AddParameter("hid", state);
                        dbClient.RunQuery();
                    }
                }

                Session.SendMessage(new RoomNotificationComposer(((StateToSet == 20) || (StateToSet == 10)) ? "forums.message.hidden" : "forums.message.restored"));

                dbClient.SetQuery("SELECT * FROM groups_forums_posts WHERE group_id = @groupid AND parent_id = @threadid OR id = @threadid ORDER BY timestamp ASC");
                dbClient.AddParameter("groupid", groupId);
                dbClient.AddParameter("threadid", parentId);

                DataTable Table2 = dbClient.getTable();

                if (Table2 == null)
                    return;

                int b = (Table2.Rows.Count <= 20) ? Table2.Rows.Count : 20;
                var posts = new List<GroupForumPost>();

                int i = 1;

                while (i <= b)
                {
                    DataRow Row = Table2.Rows[i - 1];

                    if (Row == null)
                    {
                        b--;
                        continue;
                    }

                    var thread = new GroupForumPost(Row);

                    if (thread.ParentId == 0 && thread.Hidden)
                        return;

                    posts.Add(thread);

                    i++;
                }
                Session.SendMessage(new GroupForumReadThreadMessageComposer(Session, groupId, parentId, 0, b, 0, posts));
            }
        }
    }
}
