using System;
using System.Data;
using System.Threading;

using Plus.Net;
using Plus.Core;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Communication.Interfaces;
using Plus.HabboHotel.Users.UserDataManagement;

using ConnectionManager;

using Plus.Communication.Packets.Outgoing.Sound;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.BuildersClub;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Campaigns;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

using Plus.Communication.Encryption.Crypto.Prng;
using Plus.HabboHotel.Users.Messenger.FriendBar;
using Plus.HabboHotel.Moderation;
using Plus.HabboRoleplay.Misc;

using Plus.Database.Interfaces;
using Plus.Utilities;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Permissions;

using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Events;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;

namespace Plus.HabboHotel.GameClients
{
    public class GameClient
    {
        private readonly int _id;
        private Habbo _habbo;
        public RoleplayUser _roleplay;
        public string MachineId;
        private bool _disconnected;
        public ARC4 RC4Client = null;
        private GamePacketParser _packetParser;
        private ConnectionInformation _connection;
        public bool LoggingOut = false;
        public int PingCount { get; set; }
        public string AuthTicket { get; private set; }

        public GameClient(int ClientId, ConnectionInformation pConnection)
        {
            this._id = ClientId;
            this._connection = pConnection;
            this._packetParser = new GamePacketParser(this);

            this.PingCount = 0;
        }

        private void SwitchParserRequest()
        {
            _packetParser.SetConnection(_connection);
            _packetParser.onNewPacket += parser_onNewPacket;
            byte[] data = (_connection.parser as InitialPacketParser).currentData;
            _connection.parser.Dispose();
            _connection.parser = _packetParser;
            _connection.parser.handlePacketData(data);
        }

        private void parser_onNewPacket(ClientPacket Message)
        {
            try
            {
                PlusEnvironment.GetGame().GetPacketManager().TryExecutePacket(this, Message);
            }
            catch (Exception e)
            {
                Logging.LogPacketException(Message.ToString(), e.ToString());
            }
        }

        private void PolicyRequest()
        {
            _connection.SendData(PlusEnvironment.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
                   "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                   "<cross-domain-policy>\r\n" +
                   "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                   "</cross-domain-policy>\x0"));
        }


        public void StartConnection()
        {
            if (_connection == null)
                return;

            this.PingCount = 0;

            (_connection.parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (_connection.parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
            _connection.startPacketProcessing();
        }

        public bool TryAuthenticate(string AuthTicket)
        {
            try
            {
                byte errorCode = 0;
                UserData userData = UserDataFactory.GetUserData(AuthTicket, out errorCode);
                if (errorCode == 1 || errorCode == 2)
                {
                    Disconnect(true);
                    return false;
                }

                #region Ban Checking
                //Let's have a quick search for a ban before we successfully authenticate..
                ModerationBan BanRecord = null;
                if (!string.IsNullOrEmpty(MachineId))
                {
                    if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(MachineId, out BanRecord))
                    {
                        if (PlusEnvironment.GetGame().GetModerationManager().MachineBanCheck(MachineId))
                        {
                            Disconnect(true);
                            return false;
                        }
                    }
                }

                if (userData.user != null)
                {
                    //Now let us check for a username ban record..
                    BanRecord = null;
                    if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(userData.user.Username, out BanRecord))
                    {
                        if (PlusEnvironment.GetGame().GetModerationManager().UsernameBanCheck(userData.user.Username))
                        {
                            Disconnect(true);
                            return false;
                        }
                    }
                }
                #endregion

                #region Roleplay Data
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * FROM `rp_stats` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                    DataRow UserRPRow = dbClient.getRow();

                    dbClient.SetQuery("SELECT * FROM `rp_stats_cooldowns` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                    DataRow UserRPCooldowns = dbClient.getRow();

                    if (UserRPCooldowns == null)
                    {
                        dbClient.RunQuery("INSERT INTO `rp_stats_cooldowns` (`id`) VALUES ('" + userData.userID + "')");
                        dbClient.SetQuery("SELECT * FROM `rp_stats_cooldowns` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                        UserRPCooldowns = dbClient.getRow();
                    }

                    dbClient.SetQuery("SELECT * FROM `rp_stats_farming` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                    DataRow UserRPFarming = dbClient.getRow();

                    if (UserRPFarming == null)
                    {
                        dbClient.RunQuery("INSERT INTO `rp_stats_farming` (`id`) VALUES ('" + userData.userID + "')");
                        dbClient.SetQuery("SELECT * FROM `rp_stats_farming` WHERE `id` = '" + userData.userID + "' LIMIT 1");
                        UserRPFarming = dbClient.getRow();
                    }

                    _roleplay = new RoleplayUser(this, UserRPRow, UserRPCooldowns, UserRPFarming);
                }
                #endregion

                PlusEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.userID, userData.user.Username);
                _habbo = userData.user;
                
                if (_habbo != null)
                {
                    userData.user.Init(this, userData);
                    
                    SendMessage(new AuthenticationOKComposer());
                    SendMessage(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                    SendMessage(new NavigatorSettingsComposer(_habbo.HomeRoom));
                    SendMessage(new FavouritesComposer(userData.user.FavoriteRooms));
                    SendMessage(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingAllParts));
                    SendMessage(new UserRightsComposer(this, _habbo.Rank));
                    SendMessage(new AvailabilityStatusComposer());
                    SendMessage(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                    SendMessage(new BuildersClubMembershipComposer());
                    SendMessage(new CfhTopicsInitComposer());
                    SendMessage(new BadgeDefinitionsComposer(PlusEnvironment.GetGame().GetAchievementManager()._achievements));
                    SendMessage(new SoundSettingsComposer(_habbo.ClientVolume, _habbo.ChatPreference, _habbo.AllowMessengerInvites, _habbo.FocusPreference, FriendBarStateUtility.GetInt(_habbo.FriendbarState)));  
                    //SendMessage(new TalentTrackLevelComposer());

                    if (!string.IsNullOrEmpty(MachineId))
                    {
                        if (this._habbo.MachineId != MachineId)
                        {
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `machine_id` = @MachineId WHERE `id` = @id LIMIT 1");
                                dbClient.AddParameter("MachineId", MachineId);
                                dbClient.AddParameter("id", _habbo.Id);
                                dbClient.RunQuery();
                            }
                        }

                        _habbo.MachineId = MachineId;
                    }

                    PermissionGroup PermissionGroup = null;
                    if (PlusEnvironment.GetGame().GetPermissionManager().TryGetGroup(_habbo.Rank, out PermissionGroup))
                    {
                        if (!String.IsNullOrEmpty(PermissionGroup.Badge))
                            if (!_habbo.GetBadgeComponent().HasBadge(PermissionGroup.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(PermissionGroup.Badge, true, this);
                    }

                    SubscriptionData SubData = null;
                    if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(this._habbo.VIPRank, out SubData))
                    {
                        if (!String.IsNullOrEmpty(SubData.Badge))
                        {
                            if (!_habbo.GetBadgeComponent().HasBadge(SubData.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(SubData.Badge, true, this);
                        }
                    }

                    if (!PlusEnvironment.GetGame().GetCacheManager().ContainsUser(_habbo.Id))
                        PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_habbo.Id);

                    _habbo.InitProcess();
          
                    if (userData.user.GetPermissions().HasRight("mod_tickets"))
                    {
                        SendMessage(new ModeratorInitComposer(
                          PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                          PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                          PlusEnvironment.GetGame().GetModerationManager().UserActionPresets,
                          PlusEnvironment.GetGame().GetModerationTool().GetTickets));
                    }

                    if (!string.IsNullOrWhiteSpace(PlusEnvironment.GetDBConfig().DBData["welcome_message"]))
                        SendMessage(new MOTDNotificationComposer(PlusEnvironment.GetDBConfig().DBData["welcome_message"].Replace("\\r\\n", "\n")));

                    PlusEnvironment.GetGame().GetRewardManager().CheckRewards(this);
                    this.AuthTicket = AuthTicket;
                    EventManager.TriggerEvent("OnLogin", this);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Erro durante o login do usuário: " + e);
            }
            return false;
        }

        public void SendWhisper(string Message, int Colour = 0)
        {
            if (this == null || GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            RoomUser User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Id);
            if (User == null)
                return;

            SendMessage(new WhisperComposer(User.VirtualId, Message, 0, (Colour == 0 ? User.LastBubble : Colour)));
        }

        public void Shout(string Message, int Colour = 4)
        {
            RoleplayManager.Shout(this, Message, Colour);
        }

        public void SendNotification(string Message)
        {
            SendMessage(new RoomNotificationComposer("Notificação", Message, "", "ok", "event:"));
        }

        public void SendMessage(IServerPacket Message)
        {
            byte[] bytes = Message.GetBytes();

            if (Message == null)
                return;

            if (GetConnection() == null)
                return;

            GetConnection().SendData(bytes);
        }

        public int ConnectionID
        {
            get { return _id; }
        }

        public ConnectionInformation GetConnection()
        {
            return _connection;
        }

        public Habbo GetHabbo()
        {
            return _habbo;
        }
        public RoleplayUser GetRoleplay()
        {
            return _roleplay;
        }

        public RoomUser GetRoomUser()
        {
            RoomUser RUser = null;
            try
            {
                if (this == null || GetHabbo() == null || GetHabbo().CurrentRoom == null)
                    return null;

                RUser = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Id);
            }
            catch
            {
                return RUser;
            }

            return RUser;
        }

        public void Disconnect(bool ForcedDisconnect)
        {

            if (LoggingOut)
                return;

            LoggingOut = true;

            try
            {
                #region WebSocket 
                PlusEnvironment.GetGame().GetWebEventManager().CloseSocketByGameClient(((this.GetHabbo() == null) ? 0 : this.GetHabbo().Id));
                if (GetRoleplay() != null)
                {
                    foreach (WebSocketChatRoom ChatRoom in GetRoleplay().ChatRooms.Values)
                    {
                        if (ChatRoom == null) { continue; }
                        WebSocketChatManager.Disconnect(this, ChatRoom.ChatName, false, null);
                    }
                }
                #endregion

                if (GetRoomUser() != null && !ForcedDisconnect)
                {
                    RoleplayManager.Chat(this, GetHabbo().Username + " irá sair em 10 segundos.", 1);
                    GetRoomUser().ApplyEffect(95);

                    if (GetHabbo() != null && GetHabbo().CurrentRoom != null)
                        GetHabbo().CurrentRoom.SendMessage(new SleepComposer(GetRoomUser(), true));

                    GetRoomUser().CanWalk = false;
                    GetRoomUser().ClearMovement(true);
                }

                if (GetRoleplay() != null)
                {
                    if (GetRoleplay().UserDataHandler != null)
                    {
                        GetRoleplay().UserDataHandler.SaveFarmingData();
                        GetRoleplay().UserDataHandler.SaveCooldownData();
                        GetRoleplay().UserDataHandler.SaveData();
                        GetRoleplay().UserDataHandler = null;
                    }
                }

                if (GetHabbo() != null)
                {
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        dbClient.RunQuery(GetHabbo().GetQueryString);
                }

                if (!ForcedDisconnect)
                {
                    new Thread(() =>
                    {
                        Thread.Sleep(8000);

                        if (GetRoleplay() != null)
                        {
                            GetRoleplay().UserDataHandler = new UserDataHandler(this, GetRoleplay());
                            GetRoleplay().UserDataHandler.SaveFarmingData();
                            GetRoleplay().UserDataHandler.SaveCooldownData();
                            GetRoleplay().UserDataHandler.SaveData();
                            GetRoleplay().UserDataHandler = null;
                        }

                        if (GetHabbo() != null)
                        {
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                dbClient.RunQuery(GetHabbo().GetQueryString);
                        }
                        
                        EventManager.TriggerEvent("OnDisconnect", this);

                        Thread.Sleep(1000);

                        if (GetRoomUser() != null)
                            GetRoomUser().ApplyEffect(108);

                        Thread.Sleep(1000);

                        if (GetHabbo() != null)
                        {
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.RunQuery(GetHabbo().GetQueryString);
                            }

                            GetHabbo().OnDisconnect();
                        }

                        if (!_disconnected)
                        {
                            if (_connection != null)
                                _connection.Dispose();
                            _disconnected = true;
                        }
                    }).Start();
                }
                else
                {
                    EventManager.TriggerEvent("OnDisconnect", this);

                    if (GetHabbo() != null)
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery(GetHabbo().GetQueryString);
                        }

                        GetHabbo().OnDisconnect();
                    }                    

                    if (!_disconnected)
                    {
                        if (_connection != null)
                            _connection.Dispose();
                        _disconnected = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        public void Dispose()
        {
            if (GetHabbo() != null)
                GetHabbo().OnDisconnect();

            this.MachineId = string.Empty;
            this._disconnected = true;
            this._roleplay = null;
            this._habbo = null;
            this._connection = null;
            this.RC4Client = null;
            this._packetParser = null;
        }
    }
}