using System;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupForumNewResponseMessageComposer : ServerPacket
    {
        public GroupForumNewResponseMessageComposer(GameClient Session, Group group, int groupid, int threadid, string content, int timestamp)
            : base(ServerPacketHeader.GroupForumNewResponseMessageComposer)
        {
            base.WriteInteger(groupid);
            base.WriteInteger(threadid);
            base.WriteInteger(group.ForumMessagesCount);
            base.WriteInteger(0);
            base.WriteInteger(Session.GetHabbo().Id);
            base.WriteString(Session.GetHabbo().Username);
            base.WriteString(Session.GetHabbo().Look);
            base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - timestamp));
            base.WriteString(content);
            base.WriteByte(0);
            base.WriteInteger(0);
            base.WriteString("");
            base.WriteInteger(0);
        }
    }
}