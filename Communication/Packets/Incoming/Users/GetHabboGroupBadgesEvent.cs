using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Users;

namespace Plus.Communication.Packets.Incoming.Users
{
    class GetHabboGroupBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            Dictionary<int, string> Badges = new Dictionary<int, string>();
            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (User.IsBot && User.GetBotRoleplay() != null)
                {
                    Group BotGroup = GroupManager.GetJob(User.GetBotRoleplay().Corporation);

                    if (BotGroup == null)
                        continue;

                    if (!Badges.ContainsKey(BotGroup.Id))
                        Badges.Add(BotGroup.Id, BotGroup.Badge);

                    continue;
                }

                if (User.IsPet || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetRoleplay() == null)
                    continue;

                Group Group = GroupManager.GetJob(User.GetClient().GetRoleplay().JobId);

                if (Group == null)
                    continue;

                if (!Badges.ContainsKey(Group.Id))
                    Badges.Add(Group.Id, Group.Badge);
            }

            Group Job = GroupManager.GetJob(Session.GetRoleplay().JobId);
            if (Job != null)
            {
                if (!Badges.ContainsKey(Job.Id))
                    Badges.Add(Job.Id, Job.Badge);
            }

            Room.SendMessage(new HabboGroupBadgesComposer(Badges));
            Session.SendMessage(new HabboGroupBadgesComposer(Badges));
        }
    }
}