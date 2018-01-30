using System;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupForumListingsMessageComposer : ServerPacket
    {
        public GroupForumListingsMessageComposer(int selectType, int qtdForums, int startIndex, List<Group> Groups)
            : base(ServerPacketHeader.GroupForumListingsMessageComposer)
        {
            base.WriteInteger(selectType);

            if (selectType == 0 || selectType == 1)
            {
                base.WriteInteger(qtdForums == 0 ? 1 : qtdForums);
                base.WriteInteger(startIndex);
                base.WriteInteger(Groups.Count);

                foreach (Group Group in Groups)
                {
                    base.WriteInteger(Group.Id);
                    base.WriteString(Group.Name);
                    base.WriteString(string.Empty);
                    base.WriteString(Group.Badge);
                    base.WriteInteger(0);
                    base.WriteInteger((int)Math.Round(Group.ForumScore));
                    base.WriteInteger(Group.ForumMessagesCount);
                    base.WriteInteger(0);
                    base.WriteInteger(0);
                    base.WriteInteger(Group.ForumLastPosterId);
                    base.WriteString(Group.ForumLastPosterName);
                    base.WriteInteger(Group.ForumLastPostTime);
                    base.WriteInteger(0);
                }
            }
            else if (selectType == 2)
            {
                base.WriteInteger(Groups.Count == 0 ? 1 : Groups.Count);
                base.WriteInteger(startIndex);
                base.WriteInteger(Groups.Count);

                foreach (Group Group in Groups)
                {
                    base.WriteInteger(Group.Id);
                    base.WriteString(Group.Name);
                    base.WriteString(string.Empty);
                    base.WriteString(Group.Badge);
                    base.WriteInteger(0);
                    base.WriteInteger((int)Math.Round(Group.ForumScore));
                    base.WriteInteger(Group.ForumMessagesCount);
                    base.WriteInteger(0);
                    base.WriteInteger(0);
                    base.WriteInteger(Group.ForumLastPosterId);
                    base.WriteString(Group.ForumLastPosterName);
                    base.WriteInteger(Group.ForumLastPostTime);
                    base.WriteInteger(0);
                }
            }
            else
            {
                base.WriteInteger(1);
                base.WriteInteger(startIndex);
                base.WriteInteger(0);
            }
        }
    }
}