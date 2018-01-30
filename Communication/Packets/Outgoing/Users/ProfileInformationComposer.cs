using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Users
{
    class ProfileInformationComposer : ServerPacket
    {
        public ProfileInformationComposer(Habbo Data, GameClient Session, List<Group> Groups, int friendCount, RoleplayBot Bot = null)
            : base(ServerPacketHeader.ProfileInformationMessageComposer)
        {
            if (Bot != null)
            {
                BotProfile(Session, Groups, friendCount, Bot);
            }
            else
            {

                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Data.AccountCreated).ToLocalTime();

                base.WriteInteger(Data.Id);
                base.WriteString(Data.Username);
                base.WriteString(Data.Look);
                base.WriteString(Data.Motto);
                base.WriteString(origin.ToString("dd/MM/yyyy"));
                base.WriteInteger((Data.GetStats() == null) ? 0 : Data.GetStats().AchievementPoints);
                base.WriteInteger(friendCount); // Friend Count
                base.WriteBoolean(Data.Id != Session.GetHabbo().Id && Session.GetHabbo().GetMessenger().FriendshipExists(Data.Id)); //  Is friend
                base.WriteBoolean(Data.Id != Session.GetHabbo().Id && !Session.GetHabbo().GetMessenger().FriendshipExists(Data.Id) && Session.GetHabbo().GetMessenger().RequestExists(Data.Id)); // Sent friend request
                base.WriteBoolean((PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Data.Id)) != null);

                base.WriteInteger(Groups.Where(x => x != null).ToList().Count);
                foreach (Group Group in Groups)
                {
                    if (Group == null)
                        continue;

                    base.WriteInteger(Group.Id);
                    base.WriteString(Group.Name);
                    base.WriteString(Group.Badge);
                    base.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour1, true));
                    base.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour2, false));
                    base.WriteBoolean((Data.Id == 0 && Group.Id == 1) ? true : ((Group.Id < 1000 && Data.Id > 0) ? true : false)); // todo favs
                    base.WriteInteger(0);//what the fuck
                    base.WriteBoolean(Group != null ? Group.ForumEnabled : true);//HabboTalk
                }

                base.WriteInteger(Convert.ToInt32(PlusEnvironment.GetUnixTimestamp() - Data.LastOnline)); // Last online
                base.WriteBoolean(true); // Show the profile
            }
        }

        public void BotProfile(GameClient Session, List<Group> Groups, int friendCount, RoleplayBot Bot)
        {
            int FakeBotId = Bot.Id + 1000000;
            var Habbo = PlusEnvironment.GetHabboById(1);

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Habbo.AccountCreated).ToLocalTime();

            base.WriteInteger(FakeBotId);
            base.WriteString(Bot.Name);
            base.WriteString(Bot.Figure);
            base.WriteString("Civil - [+R$]");
            base.WriteString(origin.ToString("dd/MM/yyyy"));
            base.WriteInteger(0); // Achievement
            base.WriteInteger(friendCount); // Friend Count
            base.WriteBoolean(Session.GetRoleplay().FriendsWithBot(Bot.Id)); //  Is friend
            base.WriteBoolean(false); // Sent friend request
            base.WriteBoolean(true);

            base.WriteInteger(Groups.Count);
            foreach (Group Group in Groups)
            {
                base.WriteInteger(Group.Id);
                base.WriteString(Group.Name);
                base.WriteString(Group.Badge);
                base.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour1, true));
                base.WriteString(PlusEnvironment.GetGame().GetGroupManager().GetGroupColour(Group.Colour2, false));

                if (Group.Id == Bot.Corporation)
                    base.WriteBoolean(true);
                else
                    base.WriteBoolean(false);
                base.WriteInteger(0);
                base.WriteBoolean(Group != null ? Group.ForumEnabled : true);
            }

            base.WriteInteger(0);
            base.WriteBoolean(true);
        }
    }
}