using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Groups;


namespace Plus.Communication.Packets.Incoming.Users
{
    class OpenPlayerProfileEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int userID = Packet.PopInt();
            Boolean IsMe = Packet.PopBoolean();

            if (userID >= 5000000)
            {
                Group Group = GroupManager.GetGang(userID - 5000000);
                Session.SendMessage(new GroupInfoComposer(Group, Session, true));
                return;
            }

            if (userID > 1000000)
            {
                RoleplayBot Bot = RoleplayBotManager.GetCachedBotById(userID - 1000000);

                List<Group> BotGroups = new List<Group>();
                if (Bot.Corporation > 0)
                {
                    Group Job = GroupManager.GetJob(Bot.Corporation);

                    if (Job != null)
                        BotGroups.Add(Job);
                }
                else
                {
                    Group Job = GroupManager.GetJob(1);

                    if (Job != null)
                        BotGroups.Add(Job);
                }

                Group Gang = GroupManager.GetGang(1000);

                if (Gang != null)
                    BotGroups.Add(Gang);

                int BotFriendCount = 0;

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT COUNT(0) FROM `rp_bots_friendships` WHERE `bot_id` = '" + Bot.Id + "'");
                    BotFriendCount = dbClient.getInteger();
                }
                Session.SendMessage(new ProfileInformationComposer(null, Session, BotGroups, BotFriendCount, Bot));
                return;
            }

            Habbo targetData = PlusEnvironment.GetHabboById(userID);
            
            List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(targetData.Id);
            
            int friendCount = 0;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `messenger_friendships` WHERE (`user_one_id` = @userid OR `user_two_id` = @userid)");
                dbClient.AddParameter("userid", userID);
                friendCount = dbClient.getInteger();
            }

            Session.SendMessage(new ProfileInformationComposer(targetData, Session, Groups, friendCount));
        }
    }
}
