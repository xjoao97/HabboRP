using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;

using log4net;

using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Moderation
{
    public sealed class ModerationManager
    {
        private static ILog log = LogManager.GetLogger("Plus.HabboHotel.Moderation.ModerationManager");

        private int _ticketCount = 1;
        private List<string> _userPresets = new List<string>();
        private List<string> _roomPresets = new List<string>();
        private Dictionary<string, ModerationBan> _bans = new Dictionary<string, ModerationBan>();
        private Dictionary<int, string> _userActionPresetCategories = new Dictionary<int, string>();
        private Dictionary<int, List<ModerationPresetActionMessages>> _userActionPresetMessages = new Dictionary<int, List<ModerationPresetActionMessages>>();
        private ConcurrentDictionary<int, ModerationTicket> _modTickets = new ConcurrentDictionary<int, ModerationTicket>();

        public ModerationManager()
        {
            this.Init();

            //log.Info("Moderation Manager -> LOADED");
        }

        public void Init()
        {
            if (this._userPresets.Count > 0)
                this._userPresets.Clear();
            if (this._userActionPresetCategories.Count > 0)
                this._userActionPresetCategories.Clear();
            if (this._userActionPresetMessages.Count > 0)
                this._userActionPresetMessages.Clear();
            if (this._bans.Count > 0)
                this._bans.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable PresetsTable = null;
                dbClient.SetQuery("SELECT * FROM `moderation_presets`;");
                PresetsTable = dbClient.getTable();

                if (PresetsTable != null)
                {
                    foreach (DataRow Row in PresetsTable.Rows)
                    {
                        string Type = Convert.ToString(Row["type"]).ToLower();
                        switch (Type)
                        {
                            case "user":
                                this._userPresets.Add(Convert.ToString(Row["message"]));
                                break;

                            case "room":
                                this._roomPresets.Add(Convert.ToString(Row["message"]));
                                break;
                        }
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable PresetsActionCats = null;
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_categories`;");
                PresetsActionCats = dbClient.getTable();

                if (PresetsActionCats != null)
                {
                    foreach (DataRow Row in PresetsActionCats.Rows)
                    {
                        this._userActionPresetCategories.Add(Convert.ToInt32(Row["id"]), Convert.ToString(Row["caption"]));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable PresetsActionMessages = null;
                dbClient.SetQuery("SELECT * FROM `moderation_preset_action_messages`;");
                PresetsActionMessages = dbClient.getTable();

                if (PresetsActionMessages != null)
                {
                    foreach (DataRow Row in PresetsActionMessages.Rows)
                    {
                        int ParentId = Convert.ToInt32(Row["parent_id"]);

                        if (!this._userActionPresetMessages.ContainsKey(ParentId))
                        {
                            this._userActionPresetMessages.Add(ParentId, new List<ModerationPresetActionMessages>());
                        }

                        this._userActionPresetMessages[ParentId].Add(new ModerationPresetActionMessages(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["parent_id"]), Convert.ToString(Row["caption"]), Convert.ToString(Row["message_text"]),
                            Convert.ToInt32(Row["mute_hours"]), Convert.ToInt32(Row["ban_hours"]), Convert.ToInt32(Row["ip_ban_hours"]), Convert.ToInt32(Row["trade_lock_days"]), Convert.ToString(Row["notice"])));
                    }
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetBans = null;
                dbClient.SetQuery("SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                GetBans = dbClient.getTable();

                if (GetBans != null)
                {
                    foreach (DataRow dRow in GetBans.Rows)
                    {
                        string value = Convert.ToString(dRow["value"]);
                        string reason = Convert.ToString(dRow["reason"]);
                        double expires = (double)dRow["expire"];
                        string type = Convert.ToString(dRow["bantype"]);

                        ModerationBan Ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (Ban != null)
                        {
                            if (expires > PlusEnvironment.GetUnixTimestamp())
                            {
                                if (!this._bans.ContainsKey(value))
                                    this._bans.Add(value, Ban);
                            }
                            else
                            {
                                dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" + BanTypeUtility.FromModerationBanType(Ban.Type) + "' AND `value` = @Key LIMIT 1");
                                dbClient.AddParameter("Key", value);
                                dbClient.RunQuery();
                            }
                        }
                    }
                }
            }

            //log.Info("Carregado " + (this._userPresets.Count + this._roomPresets.Count) + " moderadores atuais.");
            //log.Info("Carregado " + this._userActionPresetCategories.Count + " categorias de moderação.");
            //log.Info("Carregado " + this._userActionPresetMessages.Count + " moderation action preset messages.");
            log.Info("Carregado " + this._bans.Count + " usuário(s) e computadores banidos.");
        }

        public void ReCacheBans()
        {
            if (this._bans.Count > 0)
                this._bans.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataTable GetBans = null;
                dbClient.SetQuery("SELECT `bantype`,`value`,`reason`,`expire` FROM `bans` WHERE `bantype` = 'machine' OR `bantype` = 'user'");
                GetBans = dbClient.getTable();

                if (GetBans != null)
                {
                    foreach (DataRow dRow in GetBans.Rows)
                    {
                        string value = Convert.ToString(dRow["value"]);
                        string reason = Convert.ToString(dRow["reason"]);
                        double expires = (double)dRow["expire"];
                        string type = Convert.ToString(dRow["bantype"]);

                        ModerationBan Ban = new ModerationBan(BanTypeUtility.GetModerationBanType(type), value, reason, expires);
                        if (Ban != null)
                        {
                            if (expires > PlusEnvironment.GetUnixTimestamp())
                            {
                                if (!this._bans.ContainsKey(value))
                                    this._bans.Add(value, Ban);
                            }
                            else
                            {
                                dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" + BanTypeUtility.FromModerationBanType(Ban.Type) + "' AND `value` = @Key LIMIT 1");
                                dbClient.AddParameter("Key", value);
                                dbClient.RunQuery();
                            }
                        }
                    }
                }
            }

            //log.Info("Cached " + this._bans.Count + " username and machine bans.");
        }

        public void BanUser(string Mod, ModerationBanType Type, string BanValue, string Reason, double ExpireTimestamp)
        {
            string BanType = (Type == ModerationBanType.IP ? "ip" : Type == ModerationBanType.MACHINE ? "machine" : "user");
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `bans` (`bantype`, `value`, `reason`, `expire`, `added_by`,`added_date`) VALUES ('" + BanType + "', '" + BanValue + "', @reason, " + ExpireTimestamp + ", '" + Mod + "', '" + PlusEnvironment.GetUnixTimestamp() + "');");
                dbClient.AddParameter("reason", Reason);
                dbClient.RunQuery();
            }

            if (Type == ModerationBanType.MACHINE || Type == ModerationBanType.USERNAME)
            {
                if (!this._bans.ContainsKey(BanValue))
                    this._bans.Add(BanValue, new ModerationBan(Type, BanValue, Reason, ExpireTimestamp));
            }
        }

        public ICollection<string> UserMessagePresets
        {
            get { return this._userPresets; }
        }

        public ICollection<string> RoomMessagePresets
        {
            get { return this._roomPresets; }
        }

        public ICollection<ModerationTicket> GetTickets
        {
            get { return this._modTickets.Values; }
        }

        public Dictionary<string, List<ModerationPresetActionMessages>> UserActionPresets
        {
            get
            {
                Dictionary<string, List<ModerationPresetActionMessages>> Result = new Dictionary<string, List<ModerationPresetActionMessages>>();
                foreach (KeyValuePair<int, string> Category in this._userActionPresetCategories.ToList())
                {
                    Result.Add(Category.Value, new List<ModerationPresetActionMessages>());

                    if (this._userActionPresetMessages.ContainsKey(Category.Key))
                    {
                        foreach (ModerationPresetActionMessages Data in this._userActionPresetMessages[Category.Key])
                        {
                            Result[Category.Value].Add(Data);
                        }
                    }
                }
                return Result;
            }
        }

        public bool TryAddTicket(ModerationTicket Ticket)
        {
            Ticket.Id = this._ticketCount++;
            return this._modTickets.TryAdd(Ticket.Id, Ticket);
        }

        public bool TryGetTicket(int TicketId, out ModerationTicket Ticket)
        {
            return this._modTickets.TryGetValue(TicketId, out Ticket);
        }

        /// <summary>
        /// Runs a quick check to see if a ban record is cached in the server.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Ban"></param>
        /// <returns></returns>
        public bool IsBanned(string Key, out ModerationBan Ban)
        {
            if (this._bans.TryGetValue(Key, out Ban))
            {
                if (!Ban.Expired)
                    return true;

                //This ban has expired, let us quickly remove it here.
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `bans` WHERE `bantype` = '" + BanTypeUtility.FromModerationBanType(Ban.Type) + "' AND `value` = @Key LIMIT 1");
                    dbClient.AddParameter("Key", Key);
                    dbClient.RunQuery();
                }

                //And finally, let us remove the ban record from the cache.
                if (this._bans.ContainsKey(Key))
                    this._bans.Remove(Key);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Run a quick database check to see if this ban exists in the database.
        /// </summary>
        /// <param name="MachineId">The value of the ban.</param>
        /// <returns></returns>
        public bool MachineBanCheck(string MachineId)
        {
            ModerationBan MachineBanRecord = null;
            if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(MachineId, out MachineBanRecord))
            {
                DataRow BanRow = null;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'machine' AND `value` = @value LIMIT 1");
                    dbClient.AddParameter("value", MachineId);
                    BanRow = dbClient.getRow();

                    //If there is no more ban record, then we can simply remove it from our cache!
                    if (BanRow == null)
                    {
                        PlusEnvironment.GetGame().GetModerationManager().RemoveBan(MachineId);
                        return false;
                    }
                }
            }
            return true;
        }
       
        /// <summary>
        /// Run a quick database check to see if this ban exists in the database.
        /// </summary>
        /// <param name="Username">The value of the ban.</param>
        /// <returns></returns>
        public bool UsernameBanCheck(string Username)
        {
            ModerationBan UsernameBanRecord = null;
            if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(Username, out UsernameBanRecord))
            {
                DataRow BanRow = null;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `bans` WHERE `bantype` = 'user' AND `value` = @value LIMIT 1");
                    dbClient.AddParameter("value", Username);
                    BanRow = dbClient.getRow();

                    //If there is no more ban record, then we can simply remove it from our cache!
                    if (BanRow == null)
                    {
                        PlusEnvironment.GetGame().GetModerationManager().RemoveBan(Username);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Remove a ban from the cache based on a given value.
        /// </summary>
        /// <param name="Value"></param>
        public void RemoveBan(string Value)
        {
            this._bans.Remove(Value);
        }
    }
}
