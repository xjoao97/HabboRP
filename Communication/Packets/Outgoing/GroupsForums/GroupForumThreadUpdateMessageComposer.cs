using System;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupForumThreadUpdateMessageComposer : ServerPacket
    {
        public GroupForumThreadUpdateMessageComposer(Group Group, GroupForumPost Thread, bool Pin, bool Lock)
            : base(ServerPacketHeader.GroupForumThreadUpdateMessageComposer)
        {
            base.WriteInteger(Group.Id);
            base.WriteInteger(Thread.Id);
            base.WriteInteger(Thread.PosterId);
            base.WriteString(Thread.PosterName);
            base.WriteString(Thread.Subject);
            base.WriteBoolean(Pin);
            base.WriteBoolean(Lock);
            base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - Thread.Timestamp));
            base.WriteInteger(Thread.MessageCount + 1);
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteString("");
            base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - Thread.Timestamp));
            base.WriteByte((Thread.Hidden) ? 10 : 1);
            base.WriteInteger(1);
            base.WriteString(PlusEnvironment.GetHabboById(Thread.Hider).Username);
            base.WriteInteger(0);
        }
    }
}