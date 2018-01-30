using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users;
using System.Collections.Concurrent;

using Plus.Database.Interfaces;
using Plus.Utilities;
using log4net;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Groups
{
    public class GroupManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Groups.GroupManager");

        public Dictionary<int, GroupBackGroundColours> BackGroundColours;
        public List<GroupBaseColours> BaseColours;
        public List<GroupBases> Bases;

        public Dictionary<int, GroupSymbolColours> SymbolColours;
        public List<GroupSymbols> Symbols;

        public static ConcurrentDictionary<int, Group> Jobs = new ConcurrentDictionary<int, Group>();
        public static ConcurrentDictionary<int, Group> Gangs = new ConcurrentDictionary<int, Group>();
        public static ConcurrentDictionary<int, GroupRank> GenericGangRanks = new ConcurrentDictionary<int, GroupRank>();

        public void Initialize()
        {
            GetGenericData();
            GetJobData();
            GetGangData();

            log.Info("Carregado " + Jobs.Count + " trabalhos e " + Gangs.Count + " gangues.");
        }

        public void GetGenericData()
        {
            Bases = new List<GroupBases>();
            Symbols = new List<GroupSymbols>();
            BaseColours = new List<GroupBaseColours>();
            SymbolColours = new Dictionary<int, GroupSymbolColours>();
            BackGroundColours = new Dictionary<int, GroupBackGroundColours>();

            ClearGenericData();
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `groups_items` WHERE `enabled` = '1'");
                DataTable dItems = dbClient.getTable();

                foreach (DataRow dRow in dItems.Rows)
                {
                    switch (dRow[0].ToString())
                    {
                        case "base":
                            Bases.Add(new GroupBases(Convert.ToInt32(dRow[1]), dRow[2].ToString(), dRow[3].ToString()));
                            break;

                        case "symbol":
                            Symbols.Add(new GroupSymbols(Convert.ToInt32(dRow[1]), dRow[2].ToString(), dRow[3].ToString()));
                            break;

                        case "color":
                            BaseColours.Add(new GroupBaseColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;

                        case "color2":
                            SymbolColours.Add(Convert.ToInt32(dRow[1]), new GroupSymbolColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;

                        case "color3":
                            BackGroundColours.Add(Convert.ToInt32(dRow[1]), new GroupBackGroundColours(Convert.ToInt32(dRow[1]), dRow[2].ToString()));
                            break;
                    }
                }
            }
        }

        public void ClearGenericData()
        {
            Bases.Clear();
            Symbols.Clear();
            BaseColours.Clear();
            SymbolColours.Clear();
            BackGroundColours.Clear();
        }

        public void GetJobData()
        {
            Jobs.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_jobs`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        string Name = Row["name"].ToString();
                        string Description = Row["desc"].ToString();
                        string Badge = Row["badge"].ToString();
                        int OwnerId = Convert.ToInt32(Row["owner_id"]);
                        int Created = Convert.ToInt32(Row["created"]);
                        int RoomId = Convert.ToInt32(Row["room_id"]);
                        int State = Convert.ToInt32(Row["state"]);
                        int Colour1 = Convert.ToInt32(Row["colour1"]);
                        int Colour2 = Convert.ToInt32(Row["colour2"]);
                        int AdminOnlyDeco = Convert.ToInt32(Row["admindeco"]);

                        bool ForumEnabled = PlusEnvironment.EnumToBool(Row["forum_enabled"].ToString());
                        int ForumMessagesCount = Convert.ToInt32(Row["forum_messages_count"]);
                        double ForumScore = Convert.ToDouble(Row["forum_score"]);
                        int LastPosterId = Convert.ToInt32(Row["forum_lastposter_id"]);
                        string LastPosterName = PlusEnvironment.GetHabboById(LastPosterId) == null ? "HabboRPG" : PlusEnvironment.GetHabboById(LastPosterId).Username;
                        int LastPosterTimeStamp = Convert.ToInt32(Row["forum_lastposter_timestamp"]);

                        int WhoCanRead = Convert.ToInt32(Row["who_can_read"]);
                        int WhoCanPost = Convert.ToInt32(Row["who_can_post"]);
                        int WhoCanThread = Convert.ToInt32(Row["who_can_thread"]);
                        int WhoCanMod = Convert.ToInt32(Row["who_can_mod"]);

                        ConcurrentDictionary<int, GroupRank> Ranks = GenerateJobRanks(Id);

                        List<int> Requests;
                        ConcurrentDictionary<int, GroupMember> Members = GenerateJobMembers(Id, out Requests);

                        Group Job = new Group(Id, Name, Description, Badge, RoomId, OwnerId, Created, State, Colour1, Colour2, AdminOnlyDeco,
                                              ForumEnabled, Name, Description, ForumMessagesCount, ForumScore, LastPosterId, LastPosterName, LastPosterTimeStamp,
                                              WhoCanMod, WhoCanPost, WhoCanRead, WhoCanThread, Ranks, Members, Requests, 0, 0, 0, 0);

                        if (!Jobs.ContainsKey(Id))
                            Jobs.TryAdd(Id, Job);
                    }
                }
            }
        }

        public ConcurrentDictionary<int, GroupRank> GenerateJobRanks(int Id)
        {
            ConcurrentDictionary<int, GroupRank> Ranks = new ConcurrentDictionary<int, GroupRank>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_jobs_ranks` WHERE `job` = '" + Id + "'");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int JobRank = Convert.ToInt32(Row["rank"]);
                        string Name = Row["name"].ToString();
                        string MaleFigure = Row["male_figure"].ToString();
                        string FemaleFigure = Row["female_figure"].ToString();
                        int Pay = Convert.ToInt32(Row["pay"]);
                        string[] Commands = Row["commands"].ToString().Split(',');
                        string[] WorkRooms = Row["workrooms"].ToString().Split(',');
                        int Limit = Convert.ToInt32(Row["limit"]);

                        GroupRank Rank = new GroupRank(Id, JobRank, Name, MaleFigure, FemaleFigure, Pay, Commands, WorkRooms, Limit);

                        if (!Ranks.ContainsKey(JobRank))
                            Ranks.TryAdd(JobRank, Rank);
                    }
                }
            }
            return Ranks;
        }

        public ConcurrentDictionary<int, GroupMember> GenerateJobMembers(int Id, out List<int> Requests)
        {
            ConcurrentDictionary<int, GroupMember> Members = new ConcurrentDictionary<int, GroupMember>();
            Requests = new List<int>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id,job_rank FROM `rp_stats` WHERE `job_id` = '" + Id + "'");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["id"]);
                        int Rank = Convert.ToInt32(Row["job_rank"]);
                        bool IsAdmin = Rank == 6;

                        GroupMember Member = new GroupMember(Id, UserId, Rank, IsAdmin);

                        if (!Members.ContainsKey(UserId))
                            Members.TryAdd(UserId, Member);
                    }
                }

                dbClient.SetQuery("SELECT `id` FROM `rp_stats` WHERE `job_request` = '" + Id + "'");
                DataTable RequestTable = dbClient.getTable();

                if (RequestTable != null)
                {
                    foreach (DataRow Row in RequestTable.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["id"]);

                        if (!Requests.Contains(UserId))
                            Requests.Add(UserId);
                    }
                }
            }

            return Members;
        }

        public void GetGangData()
        {
            Gangs.Clear();
            GenericGangRanks.Clear();
            GenericGangRanks = GenerateGangRanks();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_gangs`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int Id = Convert.ToInt32(Row["id"]);
                        string Name = Row["name"].ToString();
                        string Description = Row["desc"].ToString();
                        string Badge = Row["badge"].ToString();
                        int OwnerId = Convert.ToInt32(Row["owner_id"]);
                        int Created = Convert.ToInt32(Row["created"]);
                        int RoomId = Convert.ToInt32(Row["room_id"]);
                        int State = Convert.ToInt32(Row["state"]);
                        int Colour1 = Convert.ToInt32(Row["colour1"]);
                        int Colour2 = Convert.ToInt32(Row["colour2"]);
                        int AdminOnlyDeco = Convert.ToInt32(Row["admindeco"]);

                        bool ForumEnabled = PlusEnvironment.EnumToBool(Row["forum_enabled"].ToString());
                        int ForumMessagesCount = Convert.ToInt32(Row["forum_messages_count"]);
                        double ForumScore = Convert.ToDouble(Row["forum_score"]);
                        int LastPosterId = Convert.ToInt32(Row["forum_lastposter_id"]);
                        string LastPosterName = PlusEnvironment.GetHabboById(LastPosterId) == null ? "HabboRPG" : PlusEnvironment.GetHabboById(LastPosterId).Username;
                        int LastPosterTimeStamp = Convert.ToInt32(Row["forum_lastposter_timestamp"]);

                        int WhoCanRead = Convert.ToInt32(Row["who_can_read"]);
                        int WhoCanPost = Convert.ToInt32(Row["who_can_post"]);
                        int WhoCanThread = Convert.ToInt32(Row["who_can_thread"]);
                        int WhoCanMod = Convert.ToInt32(Row["who_can_mod"]);

                        int Kills = Convert.ToInt32(Row["gang_kills"]);
                        int Deaths = Convert.ToInt32(Row["gang_deaths"]);
                        int Score = Convert.ToInt32(Row["gang_score"]);
                        int MediPacks = Convert.ToInt32(Row["medipacks"]);

                        List<int> Requests;
                        ConcurrentDictionary<int, GroupMember> Members = GenerateGangMembers(Id, out Requests);

                        Group Gang = new Group(Id, Name, Description, Badge, RoomId, OwnerId, Created, State, Colour1, Colour2, AdminOnlyDeco,
                                              ForumEnabled, Name, Description, ForumMessagesCount, ForumScore, LastPosterId, LastPosterName, LastPosterTimeStamp,
                                              WhoCanMod, WhoCanPost, WhoCanRead, WhoCanThread, GenericGangRanks, Members, Requests, Kills, Deaths, Score, MediPacks);

                        if (!Gangs.ContainsKey(Id))
                            Gangs.TryAdd(Id, Gang);
                    }
                }
            }
        }

        public ConcurrentDictionary<int, GroupRank> GenerateGangRanks()
        {
            ConcurrentDictionary<int, GroupRank> Ranks = new ConcurrentDictionary<int, GroupRank>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_gangs_ranks` WHERE `gang` = '1000'");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int GangRank = Convert.ToInt32(Row["rank"]);
                        string Name = Row["name"].ToString();
                        string MaleFigure = "";
                        string FemaleFigure = "";
                        int Pay = 0;
                        string[] Commands = Row["commands"].ToString().Split(',');
                        string[] WorkRooms = "".Split(',');
                        int Limit = Convert.ToInt32(Row["limit"]);

                        GroupRank Rank = new GroupRank(1000, GangRank, Name, MaleFigure, FemaleFigure, Pay, Commands, WorkRooms, Limit);

                        if (!Ranks.ContainsKey(GangRank))
                            Ranks.TryAdd(GangRank, Rank);
                    }
                }
            }
            return Ranks;
        }

        public ConcurrentDictionary<int, GroupMember> GenerateGangMembers(int Id, out List<int> Requests)
        {
            ConcurrentDictionary<int, GroupMember> Members = new ConcurrentDictionary<int, GroupMember>();
            Requests = new List<int>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id,gang_rank FROM `rp_stats` WHERE `gang_id` = '" + Id + "'");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["id"]);
                        int Rank = Convert.ToInt32(Row["gang_rank"]);
                        bool IsAdmin = Rank >= 5;

                        GroupMember Member = new GroupMember(Id, UserId, Rank, IsAdmin);

                        if (!Members.ContainsKey(UserId))
                            Members.TryAdd(UserId, Member);
                    }
                }

                dbClient.SetQuery("SELECT `id` FROM `rp_stats` WHERE `gang_request` = '" + Id + "'");
                DataTable RequestTable = dbClient.getTable();

                if (RequestTable != null)
                {
                    foreach (DataRow Row in RequestTable.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["id"]);

                        if (!Requests.Contains(UserId))
                            Requests.Add(UserId);
                    }
                }
            }

            return Members;
        }
        
        public static bool JobExists(int JobId, int RankId)
        {
            Group Job = GetJob(JobId);

            if (Job != null && Job.Ranks.ContainsKey(RankId))
                return true;

            return false;
        }

        public static Group GetJob(int Id)
        {
            if (Jobs.ContainsKey(Id))
                return Jobs[Id];

            return null;
        }

        public static Group GetJobByName(string Name)
        {
            if (Jobs.Values.Where(x => x.Name.ToLower() == Name.ToLower()).ToList().Count > 0)
                return Jobs.Values.FirstOrDefault(x => x.Name.ToLower() == Name.ToLower());

            return null;
        }

        public static Group GetGang(int Id)
        {
            if (Gangs.ContainsKey(Id))
                return Gangs[Id];

            return null;
        }

        public static List<GroupMember> GetGangMembersByRank(int GangId, int RankId)
        {
            Group Gang = GetGang(GangId);

            if (Gang != null && Gang.Ranks.ContainsKey(RankId))
                return Gang.Members.Values.Where(x => x.UserRank == RankId).ToList();

            return null;
        }

        public static GroupRank GetJobRank(int JobId, int RankId)
        {
            Group Group = GetJob(JobId);

            if (Group != null && Group.Ranks.ContainsKey(RankId))
                return Group.Ranks[RankId];

            return null;
        }

        public static GroupRank GetGangRank(int GangId, int RankId)
        {
            Group Group = GetGang(GangId);

            if (Group != null && Group.Ranks.ContainsKey(RankId))
                return Group.Ranks[RankId];

            return null;
        }

        public static int GetMessageCountForThread(int id)
        {
            using (var queryReactor = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.SetQuery(string.Format("SELECT COUNT(*) FROM groups_forums_posts WHERE parent_id='{0}'", id));
                return int.Parse(queryReactor.getString());
            }
        }

        public static bool HasJobCommand(GameClient Session, string command)
        {
            if (Session == null || Session.GetRoleplay() == null)
                return false;

            int JobId = Session.GetRoleplay().JobId;
            int JobRank = Session.GetRoleplay().JobRank;

            if (JobId == 1)
                return false;

            Group Job = GetJob(JobId);
            GroupRank Rank = GetJobRank(JobId, JobRank);

            if (Job == null || Rank == null)
                return false;

            if (!Rank.HasCommand(command))
                return false;

            return true;
        }

        public static bool HasGangCommand(GameClient Session, string command)
        {
            if (Session == null || Session.GetRoleplay() == null)
                return false;

            int GangId = Session.GetRoleplay().GangId;
            int GangRank = Session.GetRoleplay().GangRank;

            if (GangId == 1000)
                return false;

            Group Gang = GetGang(GangId);
            GroupRank Rank = GetGangRank(GangId, GangRank);

            if (Gang == null || Rank == null)
                return false;

            if (!Rank.HasCommand(command))
                return false;

            return true;
        }

        public bool TryCreateGroup(Habbo Player, string Name, string Description, int RoomId, string Badge, int Colour1, int Colour2, out Group Group)
        {
            Group = new Group(0, Name, Description, Badge, RoomId, Player.Id, (int)PlusEnvironment.GetUnixTimestamp(), 1, Colour1, Colour2, 0,
                false, Name, Description, 0, 0, 0, "", 0, 1, 2, 2, 3, GenericGangRanks, new ConcurrentDictionary<int, GroupMember>(), new List<int>(), 0, 0, 0, 0);

            try
            {
                if (Gangs.Values.Where(x => x.CreatorId == Player.Id).ToList().Count > 0)
                    return false;

                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Badge))
                    return false;

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `rp_gangs` (`name`, `desc`, `badge`, `owner_id`, `created`, `room_id`, `state`, `colour1`, `colour2`, `admindeco`) VALUES (@name, @desc, @badge, @owner, UNIX_TIMESTAMP(), @room, '0', @colour1, @colour2, '0')");
                    dbClient.AddParameter("name", Group.Name);
                    dbClient.AddParameter("desc", Group.Description);
                    dbClient.AddParameter("owner", Group.CreatorId);
                    dbClient.AddParameter("badge", Group.Badge);
                    dbClient.AddParameter("room", Group.RoomId);
                    dbClient.AddParameter("colour1", Group.Colour1);
                    dbClient.AddParameter("colour2", Group.Colour2);
                    Group.Id = Convert.ToInt32(dbClient.InsertQuery());

                    if (Gangs.ContainsKey(Group.Id))
                        return false;

                    Gangs.TryAdd(Group.Id, Group);

                    Player.GetClient().GetRoleplay().GangId = Group.Id;
                    Player.GetClient().GetRoleplay().GangRank = 6;
                    Player.GetClient().GetRoleplay().GangRequest = 0;

                    Group.AddNewMember(Player.Id, 6, true);

                    UserCache Junk;
                    PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(Player.Id, out Junk);
                    PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Player.Id);
                }
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return false;
            }

            return true;
        }

        public string CheckActiveSymbol(string Symbol)
        {
            if (Symbol == "s000" || Symbol == "s00000")
            {
                return "";
            }
            return Symbol;
        }

        public string GetGroupColour(int Index, bool Colour1)
        {
            if (Colour1)
            {
                if (SymbolColours.ContainsKey(Index))
                {
                    return SymbolColours[Index].Colour;
                }
            }
            else
            {
                if (BackGroundColours.ContainsKey(Index))
                {
                    return BackGroundColours[Index].Colour;
                }
            }

            return "4f8a00";
        }

        public void DeleteGroup(int Id)
        {
            Group Group = null;
            if (Gangs.ContainsKey(Id))
                Gangs.TryRemove(Id, out Group);

            if (Group != null)
                Group.Dispose();
        }

        public List<Group> GetGroupsForUser(int UserId)
        {
            List<Group> Groups = new List<Group>();

            lock (Jobs)
            {
                if (Jobs.Values.Where(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId).ToList().Count > 0)
                    Groups.Add(Jobs.Values.FirstOrDefault(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId));
            }

            lock (Gangs)
            {
                if (Gangs.Values.Where(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId).ToList().Count > 0)
                    Groups.Add(Gangs.Values.FirstOrDefault(x => x.Members.ContainsKey(UserId) || x.CreatorId == UserId));
            }

            return Groups;
        }
    }
}