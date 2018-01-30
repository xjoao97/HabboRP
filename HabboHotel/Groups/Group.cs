using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Groups
{
    public class Group
    {
        public int Id;
        public string Name;
        public int AdminOnlyDeco;
        public string Badge;
        public int CreateTime;
        public int CreatorId;
        public string Description;
        public int RoomId;
        public int Colour1;
        public int Colour2;

        public bool ForumEnabled;
        public string ForumName;
        public string ForumDescription;
        public int ForumMessagesCount;
        public double ForumScore;
        public int ForumLastPosterId;
        public string ForumLastPosterName;
        public int ForumLastPosterTimestamp;

        public int WhoCanMod;
        public int WhoCanPost;
        public int WhoCanRead;
        public int WhoCanThread;

        public int GangKills;
        public int GangDeaths;
        public int GangScore;
        public int MediPacks;

        public GroupType GroupType;

        public ConcurrentDictionary<int, GroupRank> Ranks;
        public ConcurrentDictionary<int, GroupMember> Members;
        public List<int> Requests;
        
        public Group(int Id, string Name, string Description, string Badge, int RoomId, int Owner, int Time, int Type, int Colour1, int Colour2, int AdminOnlyDeco,
            bool forumEnabled, string forumName, string forumDescription, int forumMessagesCount, double forumScore, 
            int forumLastPosterId, string forumLastPosterName, int forumLastPosterTimestamp, 
            int WhoCanMod, int WhoCanPost, int WhoCanRead, int WhoCanThread, 
            ConcurrentDictionary<int, GroupRank> Ranks, ConcurrentDictionary<int, GroupMember> Members, List<int> Requests,
            int GangKills, int GangDeaths, int GangScore, int MediPacks)
        {
            this.Id = Id;
            this.Name = Name;
            this.Description = Description;
            this.RoomId = RoomId;
            this.Badge = Badge;
            this.CreateTime = Time;
            this.CreatorId = Owner;
            this.Colour1 = (Colour1 == 0) ? 1 : Colour1;
            this.Colour2 = (Colour2 == 0) ? 1 : Colour2;
            this.AdminOnlyDeco = AdminOnlyDeco;

            this.ForumEnabled = forumEnabled;
            this.ForumName = forumName;
            this.ForumDescription = forumDescription;
            this.ForumMessagesCount = forumMessagesCount;
            this.ForumScore = forumScore;
            this.ForumLastPosterId = forumLastPosterId;
            this.ForumLastPosterName = forumLastPosterName;
            this.ForumLastPosterTimestamp = forumLastPosterTimestamp;

            this.WhoCanMod = WhoCanMod;
            this.WhoCanPost = WhoCanPost;
            this.WhoCanRead = WhoCanRead;
            this.WhoCanThread = WhoCanThread;

            this.GangKills = GangKills;
            this.GangDeaths = GangDeaths;
            this.GangScore = GangScore;
            this.MediPacks = MediPacks;

            switch (Type)
            {
                case 0:
                    this.GroupType = GroupType.OPEN;
                    break;
                case 1:
                    this.GroupType = GroupType.LOCKED;
                    break;
                case 2:
                    this.GroupType = GroupType.PRIVATE;
                    break;
            }

            this.Ranks = Ranks;
            this.Members = Members;
            this.Requests = Requests;
        }

        public int ForumLastPostTime
        {
            get { return (Convert.ToInt32(PlusEnvironment.GetUnixTimestamp()) - ForumLastPosterTimestamp); }
        }

        public void UpdateForum()
        {
            if (!ForumEnabled)
                return;

            using (var adapter = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("UPDATE rp_jobs SET forum_messages_count = @msgcount , forum_score = @score , forum_lastposter_id = @lastposterid , forum_lastposter_name = @lastpostername , forum_lastposter_timestamp = @lasttimestamp WHERE id = @id");
                adapter.AddParameter("id", this.Id);
                adapter.AddParameter("msgcount", this.ForumMessagesCount);
                adapter.AddParameter("score", this.ForumScore.ToString());
                adapter.AddParameter("lastposterid", this.ForumLastPosterId);
                adapter.AddParameter("lastpostername", this.ForumLastPosterName);
                adapter.AddParameter("lasttimestamp", this.ForumLastPosterTimestamp);
                adapter.RunQuery();
            }
        }

        public void ClearRequests()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (this.Id < 1000)
                    dbClient.SetQuery("UPDATE `rp_stats` SET `job_request` = '0' WHERE `job_request` = '" + this.Id + "'");
                else
                    dbClient.SetQuery("UPDATE `rp_stats` SET `gang_request` = '0' WHERE `gang_request` = '" + this.Id + "'");
                dbClient.RunQuery();
            }

            this.Requests.Clear();
        }

        public bool IsMember(int UserId)
        {
            return this.Members.ContainsKey(UserId);
        }

        public bool IsAdmin(int UserId)
        {
            if (!this.Members.ContainsKey(UserId))
                return false;

            return this.Members[UserId].IsAdmin;
        }

        public bool HasRequest(int UserId)
        {
            return this.Requests.Contains(UserId);
        }

        public void HandleRequest(int UserId, bool Accepted)
        {
            if (!HasRequest(UserId))
                return;

            if (Accepted)
                AddNewMember(UserId);
            else
            {
                this.Requests.Remove(UserId);

                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

                if (Client != null && Client.GetRoleplay() != null)
                {
                    if (this.Id < 1000)
                        Client.GetRoleplay().JobRequest = 0;
                    else
                        Client.GetRoleplay().GangRequest = 0;
                }
                else
                {
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        if (this.Id < 1000)
                            dbClient.SetQuery("UPDATE `rp_stats` SET `job_request` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                        else
                            dbClient.SetQuery("UPDATE `rp_stats` SET `gang_request` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                        dbClient.RunQuery();
                    }
                }
            }
        }

        public void AddNewMember(int UserId, int RankId = 1, bool UpdateDatabase = false)
        {
            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);

            if (Habbo == null)
                return;

            RemoveAllOldGroups(UserId);
            RemoveAllOldRequests(UserId);

            GroupMember Member = new GroupMember(this.Id, UserId, RankId, RankId >= 6);

            this.Members.TryAdd(UserId, Member);

            if (Habbo.GetClient() == null || UpdateDatabase)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (this.Id < 1000)
                        dbClient.SetQuery("UPDATE `rp_stats` SET `job_id` = '" + this.Id + "', `job_rank` = '" + RankId + "', `job_request` = '0', `time_worked` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                    else
                        dbClient.SetQuery("UPDATE `rp_stats` SET `gang_id` = '" + this.Id + "', `gang_rank` = '" + RankId + "', `gang_request` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                    dbClient.RunQuery();
                }
            }
        }

        public void RemoveAllOldGroups(int UserId)
        {
            List<Group> OldGroups;

            if (this.Id < 1000)
                OldGroups = GroupManager.Jobs.Values.Where(x => x.Members.ContainsKey(UserId)).ToList();
            else
                OldGroups = GroupManager.Gangs.Values.Where(x => x.Members.ContainsKey(UserId)).ToList();

            if (OldGroups.Count > 0)
            {
                foreach(Group Group in OldGroups)
                {
                    GroupMember Junk;
                    Group.Members.TryRemove(UserId, out Junk);
                }
            }
        }

        public void RemoveAllOldRequests(int UserId)
        {
            List<Group> OldGroups;

            if (this.Id < 1000)
                OldGroups = GroupManager.Jobs.Values.Where(x => x.Requests.Contains(UserId)).ToList();
            else
                OldGroups = GroupManager.Gangs.Values.Where(x => x.Requests.Contains(UserId)).ToList();

            if (OldGroups.Count > 0)
            {
                foreach (Group Group in OldGroups)
                {
                    Group.Requests.Remove(UserId);
                }
            }
        }

        public void MakeAdmin(int UserId)
        {
            if (!this.Ranks.ContainsKey(6) || this.Id >= 1000)
                return;

            if (!IsMember(UserId))
                return;

            RemoveAllOldRequests(UserId);

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rp_stats` SET `job_id` = '" + this.Id + "', `job_rank` = '6', `job_request` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                dbClient.RunQuery();
            }

            this.Members[UserId].UserRank = 6;
            this.Members[UserId].IsAdmin = true;

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client != null)
            {
                if (Client.GetRoleplay() != null && Client.GetRoleplay().IsWorking)
                {
                    WorkManager.RemoveWorkerFromList(Client);
                    Client.GetRoleplay().IsWorking = false;
                    Client.GetHabbo().Poof();
                }

                Client.GetRoleplay().JobRank = 6;
                Client.GetRoleplay().JobRequest = 0;
                Client.SendNotification("Você acabou de ser promovido para o gerente da Corporação [" + this.Name + "]");
            }
        }

        public void TakeAdmin(int UserId)
        {
            if (!IsMember(UserId) || this.Id >= 1000)
                return;

            RemoveAllOldRequests(UserId);

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rp_stats` SET `job_id` = '" + this.Id + "', `job_rank` = '1', `job_request` = '0' WHERE `id` = '" + UserId + "' LIMIT 1");
                dbClient.RunQuery();
            }

            this.Members[UserId].UserRank = 1;
            this.Members[UserId].IsAdmin = false;

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client != null)
            {
                if (Client.GetRoleplay() != null && Client.GetRoleplay().IsWorking)
                {
                    WorkManager.RemoveWorkerFromList(Client);
                    Client.GetRoleplay().IsWorking = false;
                    Client.GetHabbo().Poof();
                }

                Client.GetRoleplay().JobRank = 1;
                Client.GetRoleplay().JobRequest = 0;
                Client.SendNotification("Você acabou de ser removido como Gerente da Corporação [" + this.Name + "]");
            }
        }

        public void UpdateJobMember(int UserId)
        {
            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client != null && Client.GetRoleplay() != null)
            {

                if (!this.Members.ContainsKey(UserId))
                    return;

                this.Members[UserId].UserRank = Client.GetRoleplay().JobRank;
                this.Members[UserId].IsAdmin = Client.GetRoleplay().JobRank == 6;
            }
        }

        public void UpdateGangMember(int UserId)
        {
            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            if (Client != null && Client.GetRoleplay() != null)
            {
                this.Members[UserId].UserRank = Client.GetRoleplay().GangRank;
                this.Members[UserId].IsAdmin = Client.GetRoleplay().GangRank == 5;
            }
        }

        public void TransferGangOwnership(GameClient Session, GameClient Client)
        {
            if (Session == null || Session.GetRoleplay() == null || Session.GetHabbo() == null)
                return;

            if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null)
                return;

            this.CreatorId = Client.GetHabbo().Id;
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rp_gangs` SET `owner_id` = '" + Client.GetHabbo().Id + "' WHERE `id` = '" + this.Id + "' LIMIT 1");
                dbClient.RunQuery();
            }

            Session.GetRoleplay().GangRank = 1;
            Session.GetRoleplay().GangRequest = 0;
            UpdateGangMember(Session.GetHabbo().Id);

            Client.GetRoleplay().GangRank = 6;
            Client.GetRoleplay().GangRequest = 0;
            UpdateGangMember(Client.GetHabbo().Id);

            UserCache Junk = null;
            PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(Session.GetHabbo().Id, out Junk);
            PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Session.GetHabbo().Id);

            UserCache Junk2 = null;
            PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(Client.GetHabbo().Id, out Junk2);
            PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Client.GetHabbo().Id);

            SendPackets(Session);
            SendPackets(Client);
        }

        public void SendPackets(GameClient Client)
        {
            if (Client == null || Client.GetHabbo() == null)
                return;

            if (Client.GetRoomUser() != null && Client.GetRoomUser().GetRoom() != null)
            {
                if (this.Id < 1000)
                {
                    Client.GetRoomUser().GetRoom().SendMessage(new UpdateFavouriteGroupComposer(Client.GetHabbo().Id, this, Client.GetRoomUser().VirtualId));
                    Client.GetRoomUser().GetRoom().SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                }
                Client.SendMessage(new GroupInfoComposer(this, Client));
                Client.GetRoomUser().GetRoom().SendMessage(new GroupMemberUpdatedComposer(this.Id, Client.GetHabbo(), 4));
                Client.GetRoomUser().GetRoom().SendMessage(new HabboGroupBadgesComposer(this));
            }
            else
            {
                if (this.Id < 1000)
                    Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                Client.SendMessage(new GroupInfoComposer(this, Client));
                Client.SendMessage(new GroupMemberUpdatedComposer(this.Id, Client.GetHabbo(), 4));
                Client.SendMessage(new HabboGroupBadgesComposer(this));
            }
        }

        public void SendMembersPackets(GameClient Client)
        {
            if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                return;

            List<int> Members = new List<int>();
            List<GroupMember> Administrators = this.Members.Values.Where(x => x.IsAdmin).OrderBy(x => x.UserId).ToList();
            List<GroupMember> NonAdministrators = this.Members.Values.Where(x => !x.IsAdmin).OrderBy(x => x.UserId).ToList();

            List<GroupMember> MembersToCheck = new List<GroupMember>();
            MembersToCheck.AddRange(Administrators);
            MembersToCheck.AddRange(NonAdministrators);

            MembersToCheck = MembersToCheck.Take(500).ToList();

            if (this.Id <= 1000 && !Members.Contains(0))
                Members.Add(0);

            foreach (GroupMember Member in MembersToCheck)
            {
                if (!Members.Contains(Member.UserId))
                    Members.Add(Member.UserId);
            }

            int FinishIndex = 14 < this.Members.Count ? 14 : this.Members.Count;
            int MembersCount = Members.Count;

            Client.SendMessage(new GroupMembersComposer(this, Members, MembersCount, 0, (this.CreatorId == Client.GetHabbo().Id || this.IsAdmin(Client.GetHabbo().Id) || Client.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager")), 0, ""));
            Client.SendMessage(new GroupInfoComposer(this, Client));
        }

        public void Dispose()
        {
            if (this.Id < 1000)
                this.Ranks.Clear();
            this.Members.Clear();
            this.Requests.Clear();
        }
    }
}
