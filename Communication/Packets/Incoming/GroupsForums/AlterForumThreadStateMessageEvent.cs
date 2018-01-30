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
    class AlterForumThreadStateMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int ThreadId = Packet.PopInt();
            int StateToSet = Packet.PopInt();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE group_id = '{0}' AND id = '{1}' LIMIT 1;", GroupId, ThreadId));
                DataRow Row = dbClient.getRow();

                Group Group = GroupManager.GetJob(GroupId);

                if (Row != null)
                {
                    if (Convert.ToInt32(Row["poster_id"]) == Session.GetHabbo().Id || Group.IsAdmin(Session.GetHabbo().Id))
                    {
                        string state = "0";
                        if (StateToSet == 20 || StateToSet == 10)
                            state = "1";

                        dbClient.SetQuery(string.Format("UPDATE groups_forums_posts SET hidden = @hid, post_hider = @uid WHERE id = {0};", ThreadId));
                        dbClient.AddParameter("hid", state);
                        dbClient.AddParameter("uid", Session.GetHabbo().Id);
                        dbClient.RunQuery();
                    }
                }

                var Thread = new GroupForumPost(Row);

                Session.SendMessage(new RoomNotificationComposer(((StateToSet == 20) || (StateToSet == 10)) ? "forums.thread.hidden" : "forums.thread.restored"));

                if (Thread.ParentId != 0)
                    return;

                Session.SendMessage(new GroupForumThreadUpdateMessageComposer(Group, Thread, Thread.Pinned, Thread.Locked));

                dbClient.SetQuery("SELECT * FROM groups_forums_posts WHERE group_id = @gid AND parent_id = 0 ORDER BY timestamp DESC");
                dbClient.AddParameter("gid", GroupId);

                DataTable Table = dbClient.getTable();

                if (Table == null)
                {
                    Session.SendMessage(new GroupForumThreadRootMessageComposer(Group, 1, 0, 0, null, Session));
                    return;
                }

                int b = (Table.Rows.Count <= 20) ? Table.Rows.Count : 20;
                var Threads = new List<GroupForumPost>();

                int i = 1;

                while (i <= b)
                {
                    DataRow Row2 = Table.Rows[i - 1];

                    if (Row2 == null)
                    {
                        b--;
                        continue;
                    }

                    var thread = new GroupForumPost(Row2);

                    Threads.Add(thread);

                    i++;
                }

                Threads = Threads.OrderByDescending(x => x.Pinned).ToList();

                Session.SendMessage(new GroupForumThreadRootMessageComposer(Group, 2, 0, b, Threads, Session));
            }
        }
    }
}
