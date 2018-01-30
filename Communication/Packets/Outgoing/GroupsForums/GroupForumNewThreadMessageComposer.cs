using System;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Groups
{
    class GroupForumNewThreadMessageComposer : ServerPacket
    {
        public GroupForumNewThreadMessageComposer(GameClient Session, int groupid, int threadid, string subject, string content, int timestamp)
            : base(ServerPacketHeader.GroupForumNewThreadMessageComposer)
        {
            base.WriteInteger(groupid);
            base.WriteInteger(threadid);
            base.WriteInteger(Session.GetHabbo().Id);
            base.WriteString(subject);
            base.WriteString(content);
            base.WriteBoolean(false);
            base.WriteBoolean(false);
            base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - timestamp));
            base.WriteInteger(1);
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteString("");
            base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - timestamp));
            base.WriteByte(1);
            base.WriteInteger(1);
            base.WriteString("");
            base.WriteInteger(42);//useless
        }
    }
}