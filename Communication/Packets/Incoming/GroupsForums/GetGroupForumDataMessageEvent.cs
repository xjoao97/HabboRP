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
    class GetGroupForumDataMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            Group Group = GroupManager.GetJob(GroupId);

            if (Group == null || !Group.ForumEnabled)
                return;

            Session.SendMessage(new GroupForumDataMessageComposer(Group, Session));
        }
    }
}
