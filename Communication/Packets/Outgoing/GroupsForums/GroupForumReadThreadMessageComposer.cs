using System;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupForumReadThreadMessageComposer : ServerPacket
    {
        public GroupForumReadThreadMessageComposer(GameClient Session, int GroupId, int ThreadId, int StartIndex, int b, int indx, List<GroupForumPost> posts)
            : base(ServerPacketHeader.GroupForumReadThreadMessageComposer)
        {
            base.WriteInteger(GroupId);
            base.WriteInteger(ThreadId);
            base.WriteInteger(StartIndex);
            base.WriteInteger(b);

            foreach (GroupForumPost Post in posts)
            {
                base.WriteInteger(indx++ - 1);
                base.WriteInteger(indx - 1);
                base.WriteInteger(Post.PosterId);
                base.WriteString(Post.PosterName);
                base.WriteString(Post.PosterLook);
                base.WriteInteger(Convert.ToInt32(PlusEnvironment.GetUnixTimestamp()) - Post.Timestamp);
                base.WriteString(Post.PostContent);
                if (Post.Hidden)
                    base.WriteByte(10);
                else
                    base.WriteByte(0);
                base.WriteInteger(0);
                if (Post.Hider != 0)
                    base.WriteString(PlusEnvironment.GetHabboById(Post.Hider).Username);
                else
                    base.WriteString("");
                base.WriteInteger(0);
                base.WriteInteger(PlusEnvironment.GetHabboById(Post.PosterId).GetStats().ForumPosts);
            }
        }
    }
}