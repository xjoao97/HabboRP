using System.Collections;
using System.Collections.Generic;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.Badges;
using Plus.HabboHotel.Users.Inventory;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Users.UserDataManagement
{
    public class UserData
    {
        public int userID;
        public Habbo user;
        public Subscriptions.HC_Subscriptions.SubscriptionData Subscriptions;

        public Dictionary<int, Relationship> Relations;
        public ConcurrentDictionary<string, UserAchievement> achievements;
        public List<Badge> badges;
        public List<int> favouritedRooms;
        public Dictionary<int, MessengerRequest> requests;
        public Dictionary<int, MessengerBuddy> friends;
        public List<int> ignores;
        public Dictionary<int, int> quests;
        public List<RoomData> rooms;
        public HashSet<int> SuggestedPolls;

        public UserData(int userID, ConcurrentDictionary<string, UserAchievement> achievements, List<int> favouritedRooms, List<int> ignores,
            List<Badge> badges, Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests, List<RoomData> rooms, Dictionary<int, int> quests, Habbo user, 
            Dictionary<int, Relationship> Relations, Subscriptions.HC_Subscriptions.SubscriptionData Subscription)
        {
            this.userID = userID;
            this.achievements = achievements;
            this.favouritedRooms = favouritedRooms;
            this.ignores = ignores;
            this.badges = badges;
            this.friends = friends;
            this.requests = requests;
            this.rooms = rooms;
            this.quests = quests;
            this.user = user;
            this.Relations = Relations;
            this.Subscriptions = Subscription;
        }
    }
}