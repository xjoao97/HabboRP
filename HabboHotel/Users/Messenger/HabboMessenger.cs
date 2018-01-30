using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing;
using Plus.Utilities;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users.Effects;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Users.Messenger
{
    public class HabboMessenger
    {
        public bool AppearOffline;
        private readonly int _userId;

        public Dictionary<int, MessengerBuddy> _friends;
        private Dictionary<int, MessengerRequest> _requests;

        public HabboMessenger(int UserId)
        {
            this._userId = UserId;
            
            this._requests = new Dictionary<int, MessengerRequest>();
            this._friends = new Dictionary<int, MessengerBuddy>();
        }


        public void Init(Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests)
        {
            this._requests = new Dictionary<int, MessengerRequest>(requests);
            this._friends = new Dictionary<int, MessengerBuddy>(friends);
        }

        public bool TryGetRequest(int senderID, out MessengerRequest Request)
        {
            return this._requests.TryGetValue(senderID, out Request);
        }

        public bool TryGetFriend(int UserId, out MessengerBuddy Buddy)
        {
            return this._friends.TryGetValue(UserId, out Buddy);
        }

        public void ProcessOfflineMessages()
        {
            DataTable GetMessages = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `messenger_offline_messages` WHERE `to_id` = @id;");
                dbClient.AddParameter("id", this._userId);
                GetMessages = dbClient.getTable();

                if (GetMessages != null)
                {
                    GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(this._userId);
                    if (Client == null)
                        return;

                    foreach (DataRow Row in GetMessages.Rows)
                    {
                        Client.SendMessage(new NewConsoleMessageComposer(Convert.ToInt32(Row["from_id"]), Convert.ToString(Row["message"]), Math.Abs(Convert.ToInt32(PlusEnvironment.GetUnixTimestamp()) - Convert.ToInt32(Row["timestamp"]))));
                    }

                    dbClient.SetQuery("DELETE FROM `messenger_offline_messages` WHERE `to_id` = @id");
                    dbClient.AddParameter("id", this._userId);
                    dbClient.RunQuery();
                }
            }
        }

        public void Destroy()
        {
            IEnumerable<GameClient> onlineUsers = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);

            foreach (GameClient client in onlineUsers)
            {
                if (client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                    continue;

                client.GetHabbo().GetMessenger().UpdateFriend(_userId, null, true);
            }
        }

        public void OnStatusChanged(bool notification)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().GetMessenger() == null)
                return;

            if (_friends == null)
                return;

            IEnumerable<GameClient> onlineUsers = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);
            if (onlineUsers.Count() == 0)
                return;

            foreach (GameClient client in onlineUsers.ToList())
            {
                try
                {
                    if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                        continue;

                    client.GetHabbo().GetMessenger().UpdateFriend(_userId, client, true);

                    if (this == null || client == null || client.GetHabbo() == null)
                        continue;

                    UpdateFriend(client.GetHabbo().Id, client, notification);
                }
                catch
                {
                    continue;
                }
            }
        }

        public void UpdateFriend(int userid, GameClient client, bool notification)
        {
            if (_friends.ContainsKey(userid))
            {
                _friends[userid].UpdateUser(client);

                if (notification)
                {
                    GameClient Userclient = GetClient();
                    if (Userclient != null)
                        Userclient.SendMessage(SerializeUpdate(_friends[userid]));
                }
            }
        }

        public void HandleAllRequests()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM messenger_requests WHERE from_id = " + _userId + " OR to_id = " + _userId);
            }

            ClearRequests();
        }

        public void HandleRequest(int sender)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM messenger_requests WHERE (from_id = " + _userId + " AND to_id = " +       sender + ") OR (to_id = " + _userId + " AND from_id = " + sender + ")");
            }

            _requests.Remove(sender);
        }

        public void CreateFriendship(int friendID)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO messenger_friendships (user_one_id,user_two_id) VALUES (" + _userId + "," + friendID + ")");
            }

            OnNewFriendship(friendID);

            GameClient User = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            if (User != null && User.GetHabbo().GetMessenger() != null)
            {
                User.GetHabbo().GetMessenger().OnNewFriendship(_userId);
            }
        }

        public void DestroyFriendship(int friendID)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM messenger_friendships WHERE (user_one_id = " + _userId +     " AND user_two_id = " + friendID + ") OR (user_two_id = " + _userId +  " AND user_one_id = " + friendID + ")");

            }

            OnDestroyFriendship(friendID);

            GameClient User = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            if (User != null && User.GetHabbo().GetMessenger() != null)
                User.GetHabbo().GetMessenger().OnDestroyFriendship(_userId);
        }

        public void OnNewFriendship(int friendID)
        {
            GameClient friend = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);

            MessengerBuddy newFriend;
            if (friend == null || friend.GetHabbo() == null)
            {
                DataRow dRow;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT id,username,motto,look,last_online,hide_inroom,hide_online FROM users WHERE `id` = @friendid LIMIT 1");
                    dbClient.AddParameter("friendid", friendID);
                    dRow = dbClient.getRow();
                }

                newFriend = new MessengerBuddy(friendID, Convert.ToString(dRow["username"]), Convert.ToString(dRow["look"]), Convert.ToString(dRow["motto"]), Convert.ToInt32(dRow["last_online"]),
                    PlusEnvironment.EnumToBool(dRow["hide_online"].ToString()), PlusEnvironment.EnumToBool(dRow["hide_inroom"].ToString()), false);
            }
            else
            {
                Habbo user = friend.GetHabbo();


                newFriend = new MessengerBuddy(friendID, user.Username, user.Look, user.Motto, 0, user.AppearOffline, user.AllowPublicRoomStatus, false);
                newFriend.UpdateUser(friend);
            }

            if (!_friends.ContainsKey(friendID))
                _friends.Add(friendID, newFriend);

            GetClient().SendMessage(SerializeUpdate(newFriend));
        }

        public bool RequestExists(int requestID)
        {
            if (_requests.ContainsKey(requestID))
                return true;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_one_id FROM messenger_friendships WHERE user_one_id = @myID AND user_two_id = @friendID");
                dbClient.AddParameter("myID", Convert.ToInt32(_userId));
                dbClient.AddParameter("friendID", Convert.ToInt32(requestID));
                return dbClient.findsResult();
            }
        }

        public bool FriendshipExists(int friendID)
        {
            return _friends.ContainsKey(friendID);
        }

        public void OnDestroyFriendship(int Friend)
        {
            if (_friends.ContainsKey(Friend))
                _friends.Remove(Friend);

            #region Bot Friendship Remover
            if (GetClient() != null && GetClient().GetRoleplay() != null && GetClient().GetRoleplay().BotFriendShips != null)
            {
                int BotId = Friend - RoleplayBotManager.BotFriendMultiplyer;

                if (GetClient().GetRoleplay().FriendsWithBot(BotId))
                    GetClient().GetRoleplay().RemoveBotAsFriend(BotId);
            }
            #endregion

            GetClient().SendMessage(new FriendListUpdateComposer(Friend));
        }

        public bool RequestBuddy(string UserQuery)
        {
            int userID;
            bool hasFQDisabled;

            #region Bot Friendship
            if (RoleplayBotManager.GetDeployedBotByName(UserQuery) != null)
            {
                RoomUser BotUser = RoleplayBotManager.GetDeployedBotByName(UserQuery);

                if (BotUser.GetBotRoleplay() == null)
                    return false;

                if (GetClient().GetRoleplay().BotFriendShips.ContainsKey(BotUser.GetBotRoleplay().Id))
                {
                    GetClient().SendWhisper("Você já é amigo deste NPC!", 1);
                    return false;
                }

                if (!BotUser.GetBotRoleplay().AddableBot)
                {
                    GetClient().SendWhisper("Você não pode adicionar este bot!", 1);
                    return false;
                }

                BotUser.Chat("OK " + GetClient().GetHabbo().Username + ", eu adicionei você à lista de contatos do meu telefone!", true);
                GetClient().GetRoleplay().AddBotAsFriend(BotUser.GetBotRoleplay().Id);

                return true;
            }
            #endregion

            if (GetClient().GetRoleplay().PhoneType <= 0)
            {
                GetClient().SendWhisper("Você não tem um telefone para fazer isso! Compre um na Loja de Celulares [Quarto ID: 25]", 1);
                return false;
            }

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(UserQuery);
            if (client == null)
            {
                DataRow Row = null;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id`,`block_newfriends` FROM `users` WHERE `username` = @query LIMIT 1");
                    dbClient.AddParameter("query", UserQuery.ToLower());
                    Row = dbClient.getRow();
                }

                if (Row == null)
                    return false;

                userID = Convert.ToInt32(Row["id"]);
                hasFQDisabled = PlusEnvironment.EnumToBool(Row["block_newfriends"].ToString());
            }
            else
            {
                userID = client.GetHabbo().Id;
                hasFQDisabled = client.GetHabbo().AllowFriendRequests;
            }

            if (hasFQDisabled)
            {
                GetClient().SendMessage(new MessengerErrorComposer(39, 3));
                return false;
            }

            int ToId = userID;
            if (RequestExists(ToId))
                return true;

            int PhoneType = 0;
            if (client == null || client.GetHabbo() == null || client.GetRoleplay() == null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `phone` FROM `rp_stats` WHERE `id` = @userid LIMIT 1");
                    dbClient.AddParameter("userid", userID);
                    PhoneType = dbClient.getInteger();
                }
            }
            else
                PhoneType = client.GetRoleplay().PhoneType;

            if (PhoneType <= 0)
            {
                GetClient().SendWhisper("Desculpe, " + UserQuery + " não tem um telefone!", 1);
                return false;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `messenger_requests` (`from_id`,`to_id`) VALUES ('" + _userId + "','" + ToId + "')");
            }

            GameClient ToUser = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(ToId);
            if (ToUser == null || ToUser.GetHabbo() == null)
                return true;

            MessengerRequest Request = new MessengerRequest(ToId, _userId, PlusEnvironment.GetGame().GetClientManager().GetNameById(_userId));

            ToUser.GetHabbo().GetMessenger().OnNewRequest(_userId);

            using (UserCache ThisUser = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_userId))
            {
                if (ThisUser != null)
                    ToUser.SendMessage(new NewBuddyRequestComposer(ThisUser));
            }

            _requests.Add(ToId, Request);
            return true;
        }

        public void OnNewRequest(int friendID)
        {
            if (!_requests.ContainsKey(friendID))
                _requests.Add(friendID, new MessengerRequest(_userId, friendID, PlusEnvironment.GetGame().GetClientManager().GetNameById(friendID)));
        }

        public void SendInstantMessage(int ToId, string Message, bool CanAfford, int TextCost)
        {
            if (ToId == 0)
                return;

            if (GetClient() == null)
                return;

            if (GetClient().GetHabbo() == null)
                return;

            #region Bot Friendship Messager
            if (ToId > 1000000)
            {
                if (GetClient().GetRoleplay() != null)
                {
                    if (GetClient().GetRoleplay().FriendsWithBot(ToId))
                    {
                        if (CanAfford)
                            GetClient().GetRoleplay().MessageBot(ToId, Message);
                        else
                            GetClient().SendMessage(new NewConsoleMessageComposer(ToId, "Não consigo receber sua mensagem no momento, compre créditos de celular para me enviar um texto!"));
                    }
                }
                return;
            }
            #endregion

            if (!FriendshipExists(ToId))
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.YOUR_NOT_FRIENDS, ToId));
                return;
            }

            if (GetClient().GetHabbo().MessengerSpamCount >= 12)
            {
                GetClient().GetHabbo().MessengerSpamTime = PlusEnvironment.GetUnixTimestamp() + 60;
                GetClient().GetHabbo().MessengerSpamCount = 0;
                GetClient().SendNotification("Você não pode enviar uma mensagem, floodou o chat.\n\nVocê pode enviar uma mensagem em 60 segundos.");
                return;
            }
            else if (GetClient().GetHabbo().MessengerSpamTime > PlusEnvironment.GetUnixTimestamp())
            {
                double Time = GetClient().GetHabbo().MessengerSpamTime - PlusEnvironment.GetUnixTimestamp();
                GetClient().SendNotification("Você não pode enviar uma mensagem, floodou o chat.\n\nVocê pode enviar uma mensagem em " + Convert.ToInt32(Time) + " segundos.");
                return;
            }

            GetClient().GetHabbo().MessengerSpamCount++;

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(ToId);
            if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().GetMessenger() == null)
            {
                if (CanAfford)
                {
                    string UserName = "Username";
                    using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", ToId);
                        UserName = dbClient.getString();
                    }

                    GetClient().SendMessage(new RoomNotificationComposer("text_message", "message", "Você enviou uma mensagem de texto offline para " + UserName + "!"));
                    if (GetClient().GetHabbo().Translating)
                    {
                        string LG1 = GetClient().GetHabbo().FromLanguage.ToLower();
                        string LG2 = GetClient().GetHabbo().ToLanguage.ToLower();

                        NotifyStaffMembers(GetClient().GetHabbo(), UserName, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", false);
                    }
                    else
                        NotifyStaffMembers(GetClient().GetHabbo(), UserName, Message, false);

                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("INSERT INTO `messenger_offline_messages` (`to_id`, `from_id`, `message`, `timestamp`) VALUES (@tid, @fid, @msg, UNIX_TIMESTAMP())");
                        dbClient.AddParameter("tid", ToId);
                        dbClient.AddParameter("fid", GetClient().GetHabbo().Id);
                        dbClient.AddParameter("msg", Message);
                        dbClient.RunQuery();
                    }

                    LogPM(_userId, ToId, Message);

                    return;
                }
                else
                    GetClient().SendMessage(new NewConsoleMessageComposer(ToId, "Não consigo receber sua mensagem no momento, compre créditos de celular para me enviar um texto!"));
            }

            if (Client != null && Client.LoggingOut)
            {
                GetClient().SendMessage(new NewConsoleMessageComposer(ToId, "Não consigo receber sua mensagem agora como meu telefone está morto!"));
                return;
            }

            if (Client == null)
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.FRIEND_BUSY, ToId));
                return;
            }

            if (Client.GetHabbo() == null)
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.FRIEND_BUSY, ToId));
                return;
            }

            if (!Client.GetHabbo().AllowConsoleMessages || Client.GetHabbo().MutedUsers.Contains(GetClient().GetHabbo().Id))
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.FRIEND_BUSY, ToId));
                return;
            }

            if (GetClient().GetHabbo().TimeMuted > 0)
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.YOUR_MUTED, ToId));
                return;
            }

            if (Client.GetHabbo().TimeMuted > 0)
            {
                GetClient().SendMessage(new InstantMessageErrorComposer(MessengerMessageErrors.FRIEND_MUTED, ToId));
            }

            if (String.IsNullOrEmpty(Message))
                return;

            if (CanAfford)
            {
                GetClient().GetHabbo().Duckets -= TextCost;
                GetClient().GetHabbo().UpdateDucketsBalance();

                GetClient().SendMessage(new RoomNotificationComposer("text_message", "message", "Você enviou uma mensagem para " + Client.GetHabbo().Username + "!"));

                if (GetClient().GetRoomUser() != null)
                {
                    if (GetClient().GetRoomUser().CurrentEffect != 65)
                        GetClient().GetRoomUser().ApplyEffect(EffectsList.CellPhone);
                    GetClient().GetRoleplay().TextTimer = 5;
                }

                Client.SendMessage(new RoomNotificationComposer("text_message", "message", "Você recebeu uma nova mensagem de " + GetClient().GetHabbo().Username + "!"));

                if (Client.GetRoomUser() != null)
                {
                    if (Client.GetRoomUser().CurrentEffect != 65)
                        Client.GetRoomUser().ApplyEffect(EffectsList.CellPhone);
                    Client.GetRoleplay().TextTimer = 5;
                }

                if (GetClient().GetHabbo().Translating)
                {
                    string LG1 = GetClient().GetHabbo().FromLanguage.ToLower();
                    string LG2 = GetClient().GetHabbo().ToLanguage.ToLower();

                    Client.SendMessage(new NewConsoleMessageComposer(_userId, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]"));
                    NotifyStaffMembers(GetClient().GetHabbo(), Client.GetHabbo().Username, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]");
                }
                else
                {
                    Client.SendMessage(new NewConsoleMessageComposer(_userId, Message));
                    NotifyStaffMembers(GetClient().GetHabbo(), Client.GetHabbo().Username, Message);
                }
                LogPM(_userId, ToId, Message);
            }
            else
            {
                GetClient().SendMessage(new NewConsoleMessageComposer(ToId, "Não consigo receber sua mensagem no momento, você não tem créditos no celular para me enviar um texto!"));
            }
        }

        public void NotifyStaffMembers(Habbo Sender, string Username, string Message, bool CheckReceiver = true)
        {
            if (Sender.CurrentRoom != null)
            {
                List<RoomUser> ToNotify = Sender.CurrentRoom.GetRoomUserManager().GetRoomUserBySpecialRights();
                if (ToNotify.Count > 0)
                {
                    foreach (RoomUser user in ToNotify)
                    {
                        if (user != null && user.GetUsername() != Username && user.HabboId != Sender.Id)
                        {
                            if (user.GetClient() != null && user.GetClient().GetHabbo() != null && !user.GetClient().GetHabbo().IgnorePublicWhispers)
                            {
                                if (Sender != null && Sender.GetClient() != null && Sender.GetClient().GetRoomUser() != null)
                                    user.GetClient().SendMessage(new WhisperComposer(Sender.GetClient().GetRoomUser().VirtualId, "[Texto para " + Username + "] " + Message, 0, Sender.GetClient().GetRoomUser().LastBubble));
                            }
                        }
                    }
                }
            }

            if (CheckReceiver)
            {
                var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                if (Client == null || Client.GetRoomUser() == null || Client.GetHabbo() == null)
                    return;

                if (Client.GetHabbo().CurrentRoom == null)
                    return;

                if (Client.GetHabbo().CurrentRoomId != Sender.CurrentRoomId)
                {
                    List<RoomUser> ToNotify2 = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserBySpecialRights();
                    if (ToNotify2.Count > 0)
                    {
                        foreach (RoomUser user in ToNotify2)
                        {
                            if (user != null && user.GetUsername() != Username && user.HabboId != Sender.Id)
                            {
                                if (user.GetClient() != null && user.GetClient().GetHabbo() != null && !user.GetClient().GetHabbo().IgnorePublicWhispers)
                                {
                                    if (Client != null && Client.GetRoomUser() != null)
                                        user.GetClient().SendMessage(new WhisperComposer(Client.GetRoomUser().VirtualId, "[Text from " + Sender.Username + "] " + Message, 0, Client.GetRoomUser().LastBubble));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LogPM(int From_Id, int ToId, string Message)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO chatlogs_console VALUES (NULL, " + From_Id + ", " + ToId + ", @message, @timestamp)");
                dbClient.AddParameter("message", Message);
                dbClient.AddParameter("timestamp", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public ServerPacket SerializeUpdate(MessengerBuddy friend)
        {
            ServerPacket Packet = new ServerPacket(ServerPacketHeader.FriendListUpdateMessageComposer);
            Packet.WriteInteger(0); // category count
            Packet.WriteInteger(1); // number of updates
            Packet.WriteInteger(0); // don't know

            friend.Serialize(Packet, GetClient());
            return Packet;
        }

        public void BroadcastAchievement(int UserId, MessengerEventTypes Type, string Data)
        {
            IEnumerable<GameClient> MyFriends = PlusEnvironment.GetGame().GetClientManager().GetClientsById(this._friends.Keys);

            foreach (GameClient Client in MyFriends.ToList())
            {
                if (Client.GetHabbo() != null && Client.GetHabbo().GetMessenger() != null)
                {
                    Client.SendMessage(new FriendNotificationComposer(UserId, Type, Data));
                    Client.GetHabbo().GetMessenger().OnStatusChanged(true);
                }
            }
        }

        public void ClearRequests()
        {
          this.  _requests.Clear();
        }

        private GameClient GetClient()
        {
            return PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(this._userId);
        }

        public ICollection<MessengerRequest> GetRequests()
        {
            return this._requests.Values;
        }

        public ICollection<MessengerBuddy> GetFriends()
        {
            return this._friends.Values;
        }
    }
}