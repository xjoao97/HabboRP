using System;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupForumThreadRootMessageComposer : ServerPacket
    {
        public GroupForumThreadRootMessageComposer(Group Group, int Type, int StartIndex, int b, List<GroupForumPost> Threads, GameClient Session)
            : base(ServerPacketHeader.GroupForumThreadRootMessageComposer)
        {
            if (Type == 1)
            {
                base.WriteInteger(Group.Id);
                base.WriteInteger(0);
                base.WriteInteger(0);
            }
            if (Type == 2)
            {
                base.WriteInteger(Group.Id);
                base.WriteInteger(StartIndex);
                base.WriteInteger(b);

                foreach (GroupForumPost Thread in Threads)
                {
                    base.WriteInteger(Thread.Id);
                    base.WriteInteger(Thread.PosterId);
                    base.WriteString(Thread.PosterName);
                    base.WriteString(Thread.Subject);
                    base.WriteBoolean(Thread.Pinned);
                    base.WriteBoolean(Thread.Locked);
                    base.WriteInteger((Convert.ToInt32(PlusEnvironment.GetUnixTimestamp()) - Thread.Timestamp));
                    base.WriteInteger(Thread.MessageCount + 1);
                    base.WriteInteger(0);
                    base.WriteInteger(0);
                    base.WriteInteger(0);
                    base.WriteString(Group.ForumLastPosterName);
                    base.WriteInteger((Convert.ToInt32(PlusEnvironment.GetUnixTimestamp()) - Thread.Timestamp));
                    base.WriteByte((Thread.Hidden) ? 10 : 1);
                    base.WriteInteger(0);
                    if (Thread.Hider != 0)
                        base.WriteString(PlusEnvironment.GetHabboById(Thread.Hider).Username);
                    else
                        base.WriteString("");
                    base.WriteInteger(0);
                }
            }
        }
    }
}