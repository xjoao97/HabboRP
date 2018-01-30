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
    class ReadForumThreadMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int ThreadId = Packet.PopInt();
            int StartIndex = Packet.PopInt();
            int StopIndex = Packet.PopInt();

            Group Group = GroupManager.GetJob(GroupId);

            if (Group == null || !Group.ForumEnabled)
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM groups_forums_posts WHERE group_id = @groupid AND parent_id = @threadid OR id = @threadid ORDER BY timestamp ASC");
                dbClient.AddParameter("groupid", GroupId);
                dbClient.AddParameter("threadid", ThreadId);

                DataTable Table = dbClient.getTable();

                if (Table == null)
                    return;

                int b = (Table.Rows.Count <= 20) ? Table.Rows.Count : 20;
                var posts = new List<GroupForumPost>();

                int i = 1;

                while (i <= b)
                {
                    DataRow Row = Table.Rows[i - 1];

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
                Session.SendMessage(new GroupForumReadThreadMessageComposer(Session, GroupId, ThreadId, StartIndex,b, 0, posts));
            }
        }
    }
}
