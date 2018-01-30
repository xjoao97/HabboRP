using System;
using log4net;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache.Process;
using Plus.HabboHotel.GameClients;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Plus.HabboRoleplay.Web.Outgoing.Statistics;
namespace Plus.HabboHotel.Cache
{
    public class CacheManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Cache.CacheManager");
        public ConcurrentDictionary<int, UserCache> _usersCached;
        private ProcessComponent _process;

        public CacheManager()
        {
            this._usersCached = new ConcurrentDictionary<int, UserCache>();
            this._process = new ProcessComponent();
            this._process.Init();
            //log.Info("Cache Manager -> LOADED");
        }
        public bool ContainsUser(int Id)
        {
            return _usersCached.ContainsKey(Id);
        }

        public UserCache GenerateUser(int Id)
        {
            UserCache User = null;

            if (_usersCached.ContainsKey(Id))
                if (TryGetUser(Id, out User))
                    return User;

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Id);
            if (Client != null)
                if (Client.GetHabbo() != null)
                {
                    User = new UserCache(Id, Client.GetHabbo().Username, Client.GetRoleplay().Class, Client.GetHabbo().Look, Client.GetRoleplay().MarriedTo, Client.GetRoleplay().Level, GetUserComponent.ReturnUserStatistics(Client));
                    _usersCached.TryAdd(Id, User);
                    return User;
                }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username`, `motto`, `look` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                DataRow dRow = dbClient.getRow();

                dbClient.SetQuery("SELECT `id`,`class`,`married_to`,`level`,`level_exp`,`curhealth`,`maxhealth`,`curenergy`,`maxenergy` FROM `rp_stats` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                DataRow dRowRP = dbClient.getRow();

                if (dRow != null && dRowRP != null)
                {
                    User = new UserCache(Id, dRow["username"].ToString(), dRowRP["class"].ToString(), dRow["look"].ToString(), Convert.ToInt32(dRowRP["married_to"]), Convert.ToInt32(dRowRP["level"]), GetUserComponent.ReturnUserStatistics(dRow, dRowRP));
                    _usersCached.TryAdd(Id, User);
                }

                dRow = null;
                dRowRP = null;
            }

            return User;
        }

        public bool TryRemoveUser(int Id, out UserCache User)
        {
            return _usersCached.TryRemove(Id, out User);
        }

        public bool TryGetUser(int Id, out UserCache User)
        {
            return _usersCached.TryGetValue(Id, out User);
        }

        public bool TryUpdateUser(GameClient Session)
        {
            if (Session == null)
                return false;

            UserCache OldData;
            TryGetUser(Session.GetHabbo().Id, out OldData);

            using (UserCache NewData = new UserCache(Session.GetHabbo().Id, Session.GetHabbo().Username, Session.GetRoleplay().Class, Session.GetHabbo().Look, Session.GetRoleplay().MarriedTo, Session.GetRoleplay().Level, GetUserComponent.ReturnUserStatistics(Session)))
            {
                _usersCached.TryUpdate(Session.GetHabbo().Id, NewData, OldData);
            }

            return true;
        }

        public ICollection<UserCache> GetUserCache()
        {
            return this._usersCached.Values;
        }
    }
}