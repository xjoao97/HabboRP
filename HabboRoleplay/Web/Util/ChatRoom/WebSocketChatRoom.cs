using Newtonsoft.Json;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Web.Util.ChatRoom
{
    public class WebSocketChatRoom : IDisposable
    {
        public string ChatName { get; set; }
        public int ChatOwner { get; set; }
        public ConcurrentDictionary<GameClient, int> ChatUsers { get; set; }
        public Dictionary<object, object> ChatValues { get; set; }
        public bool FromDB { get; set; }
        public List<int> ChatAdmins { get; set; }
        public ConcurrentDictionary<int, double> BannedUsers = new ConcurrentDictionary<int, double>();
        public ConcurrentDictionary<int, double> MutedUsers = new ConcurrentDictionary<int, double>();    
        public ConcurrentDictionary<int, int> UnbannedUsers = new ConcurrentDictionary<int, int>();
        public ConcurrentDictionary<int, int> UnmutedUsers = new ConcurrentDictionary<int, int>();
        public ConcurrentDictionary<int, Dictionary<string, string>> ChatLogs = new ConcurrentDictionary<int, Dictionary<string, string>>();

        public bool MutedRoom = false;

        /// <summary>
        /// Constructor for a new WebSocket ChatRoom
        /// </summary>
        /// <param name="ChatName">Chat's name</param>
        /// <param name="ChatOwner">Chat's owner ID</param>
        /// <param name="ChatValues">Chat's values such e.g password, gang, locked</param>
        /// <param name="ChatAdmins">Chat's list of admin ID's</param>
        /// <param name="FromDB">Whether or not chat originates from the Database</param>
        public WebSocketChatRoom(string ChatName, int ChatOwner, Dictionary<object, object> ChatValues, List<int> ChatAdmins, bool FromDB = false)
        {
            if (WebSocketChatManager.RunningChatRooms == null)
                return;

            this.ChatName = ChatName.ToLower();
            this.ChatOwner = ChatOwner;
            this.ChatUsers = new ConcurrentDictionary<GameClient, int>();
            this.ChatValues = ChatValues;
            this.FromDB = FromDB;
            this.ChatAdmins = ChatAdmins;

            WebSocketChatRoom Room;

            if (WebSocketChatManager.RunningChatRooms.ContainsKey(this.ChatName))
            {
                this.Stop("Este grupo foi atualizado! Reencontre-o novamente.");
            }

            WebSocketChatManager.RunningChatRooms.TryAdd(this.ChatName, this);
        }

        /// <summary>
        /// Refresh the chats data e.g bans & mutes
        /// </summary>
        public void RefreshChatRoomData()
        {

            this.ChatValues.Clear();
            this.BannedUsers.Clear();
            this.MutedUsers.Clear();
            this.ChatAdmins.Clear();

            DataTable ChatRoomsData;

            DataRow TheChat;

            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT owner_id, password, locked, admins, gang_id from `rp_chat_rooms` WHERE name = @chatname");
                DB.AddParameter("chatname", this.ChatName);
                TheChat = DB.getRow();

               
                if (TheChat != null)
                {
                    int OwnerID = Convert.ToInt32(TheChat["owner_id"]);
                    int GangID = Convert.ToInt32(TheChat["gang_id"]);
                    string Password = Convert.ToString(TheChat["password"]);
                    this.ChatAdmins = (!String.IsNullOrEmpty(Convert.ToString(TheChat["admins"])) && Convert.ToString(TheChat["admins"]).Contains(":")) ? (Convert.ToString(TheChat["admins"]).StartsWith(":")) ? Convert.ToString(TheChat["admins"]).Remove(0, 1).Split(':').Select(int.Parse).ToList() : Convert.ToString(TheChat["admins"]).Split(':').Select(int.Parse).ToList() : new List<int>();
                    bool Locked = PlusEnvironment.EnumToBool(Convert.ToString(TheChat["locked"]));
                    
                    #region Refresh chat admins
                    
                    #endregion

                    #region Refresh chat owner
                    this.ChatOwner = OwnerID;

                    #endregion

                    #region Refresh chat password
                    this.SetChatPassword(Password);
                    #endregion

                    #region Refresh gang id
                    this.SetChatGang(GangID);
                    #endregion

                    #region Refresh locked
                    this.SetLockStatus(Locked);
                    #endregion
                    

                }
            }

                using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {

                    DB.SetQuery("SELECT * from `rp_chat_rooms_data` WHERE chat_name = @chatname");
                    DB.AddParameter("chatname", this.ChatName);

                    ChatRoomsData = DB.getTable();

                if (ChatRoomsData == null)
                    return;

                foreach (DataRow ChatRoomData in ChatRoomsData.Rows)
                {
                    string DataType = Convert.ToString(ChatRoomData["data_type"]);
                    string DataValue = Convert.ToString(ChatRoomData["data_value"]);
                    double DataExpire = Convert.ToDouble(ChatRoomData["data_timestamp_expire"]);

                    if (DataExpire <= PlusEnvironment.GetUnixTimestamp())
                    {
                        DB.RunQuery("DELETE FROM rp_chat_rooms_data WHERE data_type = '" + DataType + "' AND data_value = '" + DataValue + "'");
                        continue;
                    }

                    if (DataType == "ban")
                        this.InsertBanData(DataValue, DataExpire);

                    if (DataType == "mute")
                        this.InsertMuteData(DataValue, DataExpire);
                }
            }
        }

        /// <summary>
        /// Checks if the chat user is exploiting entry / other actions
        /// </summary>
        /// <param name="User">User to check</param>
        /// <returns></returns>
        public bool ExploitingAction(GameClient User)
        {
            if (!this.ChatUsers.ContainsKey(User))
            {
                if (User.GetRoleplay().WebSocketConnection == null)
                {
                    User.SendWhisper("Não é possível fazer isso porque você não está neste chat!", 1);
                }
                else
                {
                    User.GetRoleplay().SendTopAlert("Não é possível fazer isso porque você não está neste chat!");
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a target user can bypass restrictions 
        /// </summary>
        /// <param name="User">Target user</param>
        /// <returns>true or false</returns>
        public bool CanBypassRestrictions(GameClient User, bool ChatOwner = false)
        {

            if (User.GetHabbo().Id == this.ChatOwner)
                return true;

            if (User.GetHabbo().VIPRank > 1)
                return true;

            if (!ChatOwner)
            {
                if (this.ChatAdmins.Contains(User.GetHabbo().Id))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Add chatlog to the chat dictionary
        /// </summary>
        /// <param name="User">User who sent message</param>
        /// <param name="Message">Message user sent</param>
        public void AddChatLog(GameClient User, string Message)
        {
            this.ChatLogs.TryAdd(this.ChatLogs.Count + 1, new Dictionary<string, string>() { { "chat_name", this.ChatName }, { "user_id", Convert.ToString(User.GetHabbo().Id) }, { "chat_message", Message }, { "timestamp", Convert.ToString(PlusEnvironment.GetUnixTimestamp()) } });
        }

        /// <summary>
        /// Save the chats chatlogs
        /// </summary>
        public void SaveChatLogs()
        {
            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach(KeyValuePair<int, Dictionary<string, string>> ChatLog in this.ChatLogs)
                {
                    Dictionary<string, string> ChatLogDataDictionary = ChatLog.Value;
                    string ChatName = ChatLogDataDictionary["chat_name"];
                    string UserID = ChatLogDataDictionary["user_id"];
                    string Timestamp = ChatLogDataDictionary["timestamp"];
                    string ChatMessage = ChatLogDataDictionary["chat_message"];

                    DB.SetQuery("INSERT INTO `rp_chat_rooms_logs` (`chat_name`,`chat_message`,`user_id`,`timestamp`) VALUES (@chatname, @chatmessage, @userid, @timestamp)");
                    DB.AddParameter("chatname", ChatName);
                    DB.AddParameter("chatmessage", ChatMessage);
                    DB.AddParameter("userid", UserID);
                    DB.AddParameter("timestamp", Timestamp);
                    DB.RunQuery();
                }
            }
        }

        /// <summary>
        /// Saves the chat rooms data to database
        /// </summary>
        public void SaveChatRoomData()
        {
            try
            {
                #region Save owner, password, gang, locked
                using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB.SetQuery("UPDATE `rp_chat_rooms` SET `owner_id` = @ownerid, `password` = @password, `gang_id` = @gangid, `locked` = @locked WHERE `name` = @chatname");
                    DB.AddParameter("ownerid", this.ChatOwner);
                    DB.AddParameter("password", this.ChatValues["password"]);
                    DB.AddParameter("gangid", this.ChatValues["gang"]);
                    DB.AddParameter("locked", PlusEnvironment.BoolToEnum(Convert.ToBoolean(this.ChatValues["locked"])));
                    DB.AddParameter("chatname", this.ChatName);
                    DB.RunQuery();
                }
                #endregion

                #region Save admins

                string AdminString = "";

                foreach (int ChatAdmin in this.ChatAdmins)
                {
                    AdminString += ChatAdmin + ",";
                }

                if (AdminString.EndsWith(","))
                {
                    AdminString = AdminString.Remove(AdminString.Length - 1);
                }

                using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB.SetQuery("UPDATE `rp_chat_rooms` SET `admins` = @admins WHERE `name` = @chatname");
                    DB.AddParameter("admins", AdminString);
                    DB.AddParameter("chatname", this.ChatName);
                    DB.RunQuery();
                }

                #endregion

                #region Save banned users
                foreach (KeyValuePair<int, double> BannedUser in this.BannedUsers)
                {
                    using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        DataRow ExistingRow = null;
                        DB.SetQuery("SELECT NULL FROM `rp_chat_rooms_data` WHERE `chat_name` = @chatname AND `data_type` = 'ban' AND `data_value` = @value");
                        DB.AddParameter("chatname", this.ChatName);
                        DB.AddParameter("value", BannedUser.Key);

                        ExistingRow = DB.getRow();

                        if (ExistingRow != null)
                        {

                            DB.SetQuery("UPDATE `rp_chat_rooms_data` SET `data_timestamp_expire` = @expire WHERE `chat_name` = @chatname AND `data_type` = 'ban' AND `data_value` = @value");
                            DB.AddParameter("chatname", this.ChatName);
                            DB.AddParameter("expire", BannedUser.Value);
                            DB.AddParameter("value", BannedUser.Key);
                            DB.RunQuery();

                            continue;
                        }

                        DB.RunQuery("INSERT INTO `rp_chat_rooms_data` (`chat_name`,`data_type`,`data_value`,`data_timestamp_expire`) VALUES ('" + this.ChatName + "', 'ban', '" + BannedUser.Key + "', '" + BannedUser.Value + "')");
                    }
                }

                foreach (KeyValuePair<int, int> UnbannedUser in this.UnbannedUsers)
                {
                    int UsersID = UnbannedUser.Key;
                    using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        DB.SetQuery("DELETE FROM `rp_chat_rooms_data` WHERE `chat_name` = @chatname AND `data_type` = 'ban' AND `data_value` = @data");
                        DB.AddParameter("chatname", this.ChatName);
                        DB.AddParameter("data", UsersID);
                        DB.RunQuery();
                    }
                }
                #endregion

                #region Save muted users
                foreach (KeyValuePair<int, double> MutedUser in this.MutedUsers)
                {
                    using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        DataRow ExistingRow = null;
                        DB.SetQuery("SELECT NULL FROM `rp_chat_rooms_data` WHERE `chat_name` = @chatname AND `data_type` = 'mute' AND `data_value` = @value");
                        DB.AddParameter("chatname", this.ChatName);
                        DB.AddParameter("value", MutedUser.Key);

                        ExistingRow = DB.getRow();

                        if (ExistingRow != null)
                        {
                            DB.SetQuery("UPDATE `rp_chat_rooms_data` SET `data_timestamp_expire` = @expire WHERE `chat_name` = @chatname");
                            DB.AddParameter("chatname", this.ChatName);
                            DB.AddParameter("expire", MutedUser.Value);
                            DB.RunQuery();
                            continue;
                        }

                        DB.SetQuery("INSERT INTO `rp_chat_rooms_data` (`chat_name`, `data_type`, `data_value`, `data_timestamp_expire`) VALUES (@chatname, 'mute', @value, @expire)");
                        DB.AddParameter("chatname", this.ChatName);
                        DB.AddParameter("value", MutedUser.Key);
                        DB.AddParameter("expire", MutedUser.Value);
                        DB.RunQuery();
                    }
                }
                foreach (KeyValuePair<int, int> UnmutedUser in this.UnmutedUsers)
                {
                    int UsersID = UnmutedUser.Key;
                    using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        DB.SetQuery("DELETE FROM `rp_chat_rooms_data` WHERE `chat_name` = @chatname AND `data_type` = 'mute' AND `data_value` = @data");
                        DB.AddParameter("chatname", this.ChatName);
                        DB.AddParameter("data", UsersID);
                        DB.RunQuery();
                    }
                }
                #endregion

                this.SaveChatLogs();

            }
            catch(Exception e)
            {

            }
        }

        /// <summary>
        /// Insert muted user data into mutedusers list
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="MuteExpire"></param>
        public void InsertMuteData(string UserID, double MuteExpire)
        {
            this.MutedUsers.TryAdd(Convert.ToInt32(UserID), MuteExpire);
        }

        /// <summary>
        /// Insert banned user data into bannedusers list
        /// </summary>
        /// <param name="UserID"></param>
        /// <param name="BanExpire"></param>
        public void InsertBanData(string UserID, double BanExpire)
        {
            this.BannedUsers.TryAdd(Convert.ToInt32(UserID), BanExpire);
        }

        /// <summary>
        /// Adds user to chat list to proceed authorisation {FIRED WHEN A USER JOINED}
        /// </summary>
        /// <param name="User">Target user</param>
        /// <returns></returns>
        public bool OnUserJoin(GameClient User)
        {
            if (User == null)
                return false;

            if (User.LoggingOut)
                return false;

            if (User.GetHabbo() == null)
                return false;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir.", 1);
                return false;
            }

            if (!this.ChatUsers.ContainsKey(User))
                 this.ChatUsers.TryAdd(User, User.GetHabbo().Id);

            return true;
        }

        /// <summary>
        /// Removes user from chat list {FIRED WHEN A USER LEFT}
        /// </summary>
        /// <param name="User">Target user</param>
        /// <returns></returns>
        public bool OnUserLeft(GameClient User)
        {
            if (User == null)
                return false;

            if (!this.ChatUsers.ContainsKey(User))
                return false;

            lock (this.ChatUsers)
            {
                int OutV;

                if (User.GetHabbo() == null)
                {
                    this.ChatUsers.TryRemove(User, out OutV);
                    return false;
                }
              
                this.ChatUsers.TryRemove(User, out OutV);
                this.DecomposeChatDIV(User);

                this.BroadCastChatData(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
                {
                    { "event", "chatManager" },
                    { "chatname", ChatName },
                    { "action", "newleftchat" },
                    { "chatusername", User.GetHabbo().Username },
                    { "chatuserfigure", User.GetHabbo().Look }
                }));
            }

            return true;
        }

        /// <summary>
        /// Warns user with a POP-UP HTML DIV
        /// </summary>
        /// <param name="User">Target user</param>
        /// <param name="Message">Desired notice</param>
        public void WarnUser(GameClient User, string Message)
        {
            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "newwarnchat" },
                { "chatname", this.ChatName },
                { "chatmessage", Message }
            }));
        }

        /// <summary>
        /// Sends user a GREY chat alert 
        /// </summary>
        /// <param name="User">Target user</param>
        /// <param name="Message">Desired notice</param>
        public void SendGreyChatAlert(GameClient User, string Message)
        {
            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "newgreymsg" },
                { "chatname", this.ChatName },
                { "chatmessage", Message }
            }));
        }

        /// <summary>
        /// Begin's chat joining process, {sends chatroomwebevent to join}
        /// </summary>
        /// <param name="User">Target user</param>
        public void BeginChatJoin(GameClient User)
        {
            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User, "event_chatroom", JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "chatname", ChatName },
                { "action", "joinedchat" }
            }));
        }

        /// <summary>
        /// Authorises chat joining {sends data directly to JSON handler to insert chat data into Javascript object}
        /// </summary>
        /// <param name="User">Target user</param>
        public void AuthoriseChatJoin(GameClient User)
        {
            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "authorisechat" },
                { "chatname", this.ChatName }
            }));
        }

        /// <summary>
        /// Builds the chat HTML DIV User Interface
        /// </summary>
        /// <param name="User">Target user</param>
        public void BuildChatDIV(GameClient User)
        {
            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "buildchat" },
                { "chatname", this.ChatName },
                { "chatownerfigure", this.ReturnOwnersFigure() }
            }));
        }

        /// <summary>
        /// Returns the chat owners figure
        /// </summary>
        public string ReturnOwnersFigure()
        {
            string Figure = "";

            using (UserCache Member = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(this.ChatOwner))
            {

                if (Member == null)
                    return null;

                Figure = Member.Look;
            }

            return Figure;
        }

        /// <summary>
        /// Deletes the chat HTML DIV User Interface
        /// </summary>
        /// <param name="User">Target user</param>
        public void DecomposeChatDIV(GameClient User)
        {
            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "leavechat" },
                { "chatname", this.ChatName }
            }));
        }

        /// <summary>
        /// Broadcasts a new chat message entry
        /// </summary>
        /// <param name="User">Broadcasting user source</param>
        /// <param name="Params"></param>
        public void BroadCastNewChat(GameClient User, Dictionary<object, object> Params)
        {

            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "onsendchat" },
                { "chatname", this.ChatName },
                { "chatmessage", Convert.ToString(Params["chatmessage"]) },
            }));

            this.BroadCastChatData(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "onchat" },
                { "chatname", this.ChatName },
                { "chatusername", Convert.ToString(Params["chatusername"]) },
                { "chatmessage", Convert.ToString(Params["chatmessage"]) },
                { "chatfigure", User.GetHabbo().Look }
            }));

            #region Text stuff
            foreach (GameClient ChatUser in this.ChatUsers.Keys)
            {
                if (ChatUser == null)
                    continue;

                if (ChatUser == User)
                    continue;

                if (ChatUser.GetRoleplay() == null)
                    continue;

                if (ChatUser.GetRoomUser() == null)
                    continue;

                //ChatUser.SendWhisper("You have received a new text message from the '" + this.ChatName + "' WhatsHolo Group Chat on your " + RoleplayManager.GetPhoneName(ChatUser) + "!", 1);
                //ChatUser.SendMessage(new RoomNotificationComposer("text_message", "message", "Nova mensagem no grupo do WhatsApp '" + this.ChatName + "'!"));
            }
            #endregion
        }

        /// <summary>
        /// Broadcasts a new chat HTML entry
        /// </summary>
        /// <param name="User">Broadcasting user source<</param>
        /// <param name="Params">Data to broadcast</param>
        public void BroadCastChatHTML(GameClient User, Dictionary<object, object> Params)
        {

            if (User == null)
                return;

            if (User.LoggingOut)
                return;

            if (User.GetRoleplay().WebSocketConnection == null)
            {
                User.SendWhisper("Sua conexão do websocket está atualmente offline. Entre em contato com um membro da equipe se esse problema persistir", 1);
                return;
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "specialembed" },
                { "chatname", this.ChatName },
                { "chatmessage", Convert.ToString(Params["chatmessage"]) },

                { "chatembedtype", Params["chatEmbedType"] },
                { "chatembedlink", Params["chatEmbedLink"] },
                { "chatembedextra", Params["chatEmbedExtra"] },
                { "chatembedaction", "send" }
            }));
            this.BroadCastChatData(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "specialembed" },
                { "chatname", this.ChatName },
                { "chatmessage", Convert.ToString(Params["chatmessage"]) },
               
                { "chatembedtype", Params["chatEmbedType"] },
                { "chatembedlink", Params["chatEmbedLink"] },
                { "chatembedextra", Params["chatEmbedExtra"] },
                { "chatembedaction", "receive" },

                { "chatusername", Convert.ToString(Params["chatusername"]) },
                { "chatfigure", User.GetHabbo().Look },
            }));
        }

        /// <summary>
        ///Broadcasts a new chat DATA
        /// </summary>
        /// <param name="User">Broadcasting user source<</param>
        /// <param name="Data">Data to broadcast</param>
        /// <param name="SendToMe">Send to the broadcaster</param>
        public void BroadCastChatData(GameClient User, string Data, bool SendToMe = false)
        {

            foreach (GameClient ChatUser in this.ChatUsers.Keys)
            {
                if (ChatUser == null)
                    continue;

                if (ChatUser.LoggingOut)
                    continue;

                if (ChatUser.GetRoleplay() == null)
                    continue;

                if (ChatUser.GetRoleplay().WebSocketConnection == null)
                    continue;

                if (!SendToMe)
                {
                    if (ChatUser == User)
                        continue;
                }

                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(ChatUser, Data);       
            }
        }

        /// <summary>
        /// BroadCast a chat warning message to all users in the chat
        /// </summary>
        /// <param name="User">Broadcasting user</param>
        /// <param name="Msg">Message to send to all of chat users</param>
        public void BroadCastChatWarning(GameClient User, string Msg)
        {
            this.BroadCastChatData(User, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "action", "newwarnchat" },
                { "chatname", this.ChatName },
                { "chatmessage", Msg }
            }), true);
        }

        /// <summary>
        /// Set the gang of the chat joinable by gang members ONLY
        /// </summary>
        /// <param name="GangId">Desired GangID</param>
        /// <returns>Returns true or false</returns>
        public bool SetChatGang(int GangId)
        {
            if (this.ChatValues.ContainsKey("gang"))
            {
                this.ChatValues["gang"] = GangId;
                return true;
            }
            else
            {
                this.ChatValues.Add("gang", GangId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the chats lock status
        /// </summary>
        /// <param name="Locked">true = locked, false = unlocked</param>
        public bool SetLockStatus(bool Locked)
        {
            if (!this.ChatValues.ContainsKey("locked"))
            {
                this.ChatValues.Add("locked", Locked);
                return true;
            }
            else
            {
                this.ChatValues["locked"] = Locked;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the password of the chat REQUIRED to join this chat
        /// </summary>
        /// <param name="Password">Desired Password</param>
        /// <returns></returns>
        public bool SetChatPassword(string Password)
        {

            if (this.ChatValues.ContainsKey("password"))
            {
                this.ChatValues["password"] = Password;
                return true;
            }
            else
            {
                this.ChatValues.Add("password", Password);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if there are no users in the chat, and delete the chat instance
        /// </summary>
        /// <returns></returns>
        public bool CheckDelete()
        {
            if (this.FromDB)
                return false;

            if (this.ChatUsers == null)
                return false;

            if (WebSocketChatManager.RunningChatRooms == null)
                return false;

            if (this.ChatUsers.Count <= 0)
            {
                this.SaveNewChat();
            }

            return false;
        }

        public void SaveNewChat()
        {

            DataRow CheckRow = null;

            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT NULL FROM rp_chat_rooms WHERE name = @chatname");
                DB.AddParameter("chatname", this.ChatName);

                CheckRow = DB.getRow();
                if (CheckRow == null)
                {

                    DB.SetQuery("INSERT INTO rp_chat_rooms(owner_id, name, password, locked, gang_id, admins) VALUES('" + this.ChatOwner + "', @chatname, '" + this.ChatValues["password"] + "', '" + PlusEnvironment.BoolToEnum(Convert.ToBoolean(this.ChatValues["locked"])) + "', '" + this.ChatValues["gang"] + "', '" + String.Join(":", this.ChatAdmins) + "')");
                    DB.AddParameter("chatname", this.ChatName);
                    DB.RunQuery();

                    this.FromDB = false;
                }
            }
        }

        /// <summary>
        /// Stop the chat completely 
        /// </summary>
        public void Stop(string StopMsg)
        {
            lock (this.ChatUsers)
            {
                WebSocketChatRoom ChatRoom;
                WebSocketChatManager.RunningChatRooms.TryRemove(this.ChatName, out ChatRoom);

                foreach (GameClient User in this.ChatUsers.Keys)
                {
                    if (User == null)
                        continue;

                    if (User.LoggingOut)
                        continue;

                    if (User.GetRoleplay().WebSocketConnection != null)
                    {
                        if (!User.LoggingOut)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(User, Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<object, object>() { { "event", "chatManager" }, { "action", "disconnectchat" }, { "chatname", this.ChatName } }));
                            if (User.GetRoleplay() != null)
                                this.SendGreyChatAlert(User, StopMsg);
                        }
                    }

                    WebSocketChatManager.Disconnect(User, this.ChatName, true, StopMsg);
                }
            }

            new System.Threading.Thread(() => {
                System.Threading.Thread.Sleep(50);
                this.Dispose();
            }).Start();
        }

        /// <summary>
        /// Dispose of the chats variables & initiate garbage collection
        /// </summary>
        public void Dispose()
        {
            ChatUsers = new ConcurrentDictionary<GameClient, int>();
            ChatValues = null;
            ChatAdmins.Clear();

            WebSocketChatRoom OutChat;

            if (WebSocketChatManager.RunningChatRooms.ContainsKey(this.ChatName))
                WebSocketChatManager.RunningChatRooms.TryRemove(this.ChatName, out OutChat);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Returns the chats type, : password, locked, available
        /// </summary>
        /// <returns></returns>
        public string GetChatType()
        {
            if (this.ChatValues.Count > 0)
            {
                if (Convert.ToInt32(this.ChatValues["gang"]) > 0)
                    return "gang";

                if (!string.IsNullOrEmpty(Convert.ToString(this.ChatValues["password"])))
                    return "password";

                if (Convert.ToBoolean(this.ChatValues["locked"]))
                    return "locked";
            }

            return "available";
        }

        /// <summary>
        /// Increment the flood time & check for spam & mute
        /// </summary>
        /// <param name="User">Checking User</param>
        /// <param name="MuteTime">Outputs the time to mute the user</param>
        /// <returns></returns>
        public bool IncrementAndCheckFlood(GameClient User, out int MuteTime)
        {

            MuteTime = 0;

            if (User == null)
                return false;

            if (User.LoggingOut)
                return false;

            if (User.GetRoleplay() == null)
                return false;

            User.GetRoleplay().socketChatSpamCount++;

            if (User.GetRoleplay().socketChatSpamTicks == -1)
                User.GetRoleplay().socketChatSpamTicks = 8;

            else if (User.GetRoleplay().socketChatSpamCount >= 8)
            {
                
                if (User.GetHabbo().GetPermissions().HasRight("events_staff"))
                    MuteTime = 3;
                else if (User.GetHabbo().GetPermissions().HasRight("gold_vip"))
                    MuteTime = 7;
                else if (User.GetHabbo().GetPermissions().HasRight("silver_vip"))
                    MuteTime = 10;
                else
                    MuteTime = 15;

                User.GetRoleplay().socketChatFloodTime = MuteTime;
                User.GetRoleplay().socketChatSpamCount = 0;

                return true;
            }

            return false;
        }
    }
}
