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

namespace Plus.Communication.Packets.Incoming.Groups
{
    class PublishForumThreadMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            TimeSpan Span = DateTime.Now - Session.GetHabbo().LastForumMessageUpdateTime;

            if (Span.TotalSeconds < 20)
                return;

            int groupId = Packet.PopInt();
            int threadId = Packet.PopInt();
            string subject = Packet.PopString();
            string content = Packet.PopString();

            Group group = GroupManager.GetJob(groupId);

            if (group == null || !group.ForumEnabled)
                return;

            int timestamp = Convert.ToInt32(PlusEnvironment.GetUnixTimestamp());

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (threadId != 0)
                {
                    dbClient.SetQuery(string.Format("SELECT * FROM groups_forums_posts WHERE id = {0}", threadId));
                    DataRow Row = dbClient.getRow();

                    var Post = new GroupForumPost(Row);

                    if (Post.Locked || Post.Hidden)
                    {
                        Session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forums_cancel"));
                        return;
                    }
                }
                Session.GetHabbo().LastForumMessageUpdateTime = DateTime.Now;

                dbClient.SetQuery("INSERT INTO groups_forums_posts (group_id, parent_id, timestamp, poster_id, poster_name, poster_look, subject, post_content) VALUES (@gid, @pard, @ts, @pid, @pnm, @plk, @subjc, @content)");
                dbClient.AddParameter("gid", groupId);
                dbClient.AddParameter("pard", threadId);
                dbClient.AddParameter("ts", timestamp);
                dbClient.AddParameter("pid", Session.GetHabbo().Id);
                dbClient.AddParameter("pnm", Session.GetHabbo().Username);
                dbClient.AddParameter("plk", Session.GetHabbo().Look);
                dbClient.AddParameter("subjc", subject);
                dbClient.AddParameter("content", content);

                threadId = dbClient.getInteger();
            }

            group.ForumScore += 0.25;
            group.ForumLastPosterName = Session.GetHabbo().Username;
            group.ForumLastPosterId = Session.GetHabbo().Id;
            group.ForumLastPosterTimestamp = timestamp;
            group.ForumMessagesCount++;
            group.UpdateForum();

            if (threadId == 0)
                Session.SendMessage(new GroupForumNewThreadMessageComposer(Session, groupId, threadId, subject, content, timestamp));
            else
                Session.SendMessage(new GroupForumNewResponseMessageComposer(Session, group, groupId, threadId, content, timestamp));

            Session.GetHabbo().GetStats().ForumPosts++;
        }
    }
}
