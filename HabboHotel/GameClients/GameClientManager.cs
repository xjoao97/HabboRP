using System;
using System.Collections.Generic;
using System.Text;
using ConnectionManager;

using Plus.Core;
using Plus.HabboHotel.Users.Messenger;


using System.Linq;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing;

using log4net;
using System.Data;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Database.Interfaces;
using System.Collections;
using Plus.Communication.Packets.Outgoing.Handshake;
using System.Diagnostics;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.GameClients
{
    public class GameClientManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.GameClients.GameClientManager");

        private ConcurrentDictionary<int, GameClient> _clients;
        private ConcurrentDictionary<int, GameClient> _userIDRegister;
        private ConcurrentDictionary<string, GameClient> _usernameRegister;

        private readonly Queue timedOutConnections;

        private readonly Stopwatch clientPingStopwatch;

        public GameClientManager()
        {
            this._clients = new ConcurrentDictionary<int, GameClient>();
            this._userIDRegister = new ConcurrentDictionary<int, GameClient>();
            this._usernameRegister = new ConcurrentDictionary<string, GameClient>();

            timedOutConnections = new Queue();

            clientPingStopwatch = new Stopwatch();
            clientPingStopwatch.Start();
        }

        public void OnCycle()
        {
            TestClientConnections();
            HandleTimeouts();
        }

        public GameClient GetClientByUserID(int userID)
        {
            if (_userIDRegister.ContainsKey(userID))
                return _userIDRegister[userID];
            return null;
        }

        public GameClient GetClientByUsername(string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
                return _usernameRegister[username.ToLower()];
            return null;
        }

        public bool TryGetClient(int ClientId, out GameClient Client)
        {
            return this._clients.TryGetValue(ClientId, out Client);
        }

        public bool UpdateClientUsername(GameClient Client, string OldUsername, string NewUsername)
        {
            if (Client == null || !_usernameRegister.ContainsKey(OldUsername.ToLower()))
                return false;

            _usernameRegister.TryRemove(OldUsername.ToLower(), out Client);
            _usernameRegister.TryAdd(NewUsername.ToLower(), Client);
            return true;
        }

        public string GetNameById(int Id)
        {
            GameClient client = GetClientByUserID(Id);

            if (client != null)
                return client.GetHabbo().Username;

            string username;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                username = dbClient.getString();
            }

            return username;
        }

        public IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users)
        {
            foreach (int id in users)
            {
                GameClient client = GetClientByUserID(id);
                if (client != null)
                    yield return client;
            }
        }

        public void StaffWhisperAlert(string Message, GameClient Session)
        {
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (!client.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    continue;

                client.SendWhisper("[Alerta STAFF] [" + Session.GetHabbo().Username + "] " + Message, 23);
            }
        }

        public void AmbassadorWhisperAlert(string Message, GameClient Session)
        {
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (!client.GetHabbo().GetPermissions().HasRight("ambassador"))
                    continue;

                client.SendWhisper("[Alerta EMBAIXADOR] [" + Session.GetHabbo().Username + "] " + Message, 37);
            }
        }

        public void VIPWhisperAlert(string Message, GameClient Session)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("advertisement_filter_override"))
            {
                string Phrase = "";
                if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message, out Phrase))
                {
                    Session.GetHabbo().AdvertisingStrikes++;

                    if (Session.GetHabbo().AdvertisingStrikes < 2)
                    {
                        Session.SendMessage(new RoomNotificationComposer("Perigo!", "Por favor, evite de anunciar outros sites que não são afiliados ou oferecidos pelo HabboRPG. Você será silenciado se você fizer isso de novo!<br><br>Frase na lista negra: '" + Phrase + "'", "frank10", "ok", "event:"));
                        return;
                    }

                    if (Session.GetHabbo().AdvertisingStrikes >= 2)
                    {
                        Session.GetHabbo().TimeMuted = 3600;

                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `users` SET `time_muted` = '3600' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                        }

                        Session.SendMessage(new RoomNotificationComposer("Você ficou mudo!", "Desculpe, mas você foi automaticamente silenciado por anunciar outros links '" + Phrase + "'.<br><br>A equipe de moderação foi notificada e ações serão tomadas dentro de sua conta!", "frank10", "ok", "event:"));

                        List<string> Messages = new List<string>();
                        Messages.Add(Message);
                        PlusEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, 9, Session.GetHabbo().Id, "[Servidor] O civil já recebeu uma advertência " + Phrase + ".", Messages);
                        return;
                    }

                    return;
                }
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);

            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (!client.GetHabbo().GetPermissions().HasRight("mod_tool") && client.GetHabbo().VIPRank == 0)
                    continue;

                if (client.GetRoleplay().DisableVIPA == true)
                    continue;

                client.SendWhisper("[Alerta VIP] [" + Session.GetHabbo().Username + "] " + Message, 11);
            }
        }

        public void RadioAlert(string Message, GameClient Session)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("advertisement_filter_override"))
            {
                string Phrase = "";
                if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message, out Phrase))
                {
                    Session.GetHabbo().AdvertisingStrikes++;

                    if (Session.GetHabbo().AdvertisingStrikes < 2)
                    {
                        Session.SendMessage(new RoomNotificationComposer("Perigo!", "Por favor, evite de anunciar outros sites que não são afiliados ou oferecidos pelo HabboRPG. Você será silenciado se você fizer isso de novo!<<br><br>Frase da lista negra: '" + Phrase + "'", "frank10", "ok", "event:"));
                        return;
                    }

                    if (Session.GetHabbo().AdvertisingStrikes >= 2)
                    {
                        Session.GetHabbo().TimeMuted = 3600;

                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `users` SET `time_muted` = '3600' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                        }

                        Session.SendMessage(new RoomNotificationComposer("Você ficou mudo!", "Desculpe, mas você foi automaticamente silenciado por anunciar outros links '" + Phrase + "'.<br><br>A equipe de moderação foi notificada e ações serão tomadas dentro de sua conta!", "frank10", "ok", "event:"));

                        List<string> Messages = new List<string>();
                        Messages.Add(Message);
                        PlusEnvironment.GetGame().GetModerationTool().SendNewTicket(Session, 9, Session.GetHabbo().Id, "[Servidor] O civil já recebeu uma advertência " + Phrase + ".", Messages);
                        return;
                    }

                    return;
                }
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);

            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (!Groups.GroupManager.HasJobCommand(client, "radio") && !client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                    continue;

                if (client.GetRoleplay().DisableRadio == true)
                    continue;

                client.SendWhisper("[Alerta RÁDIO] [" + Session.GetHabbo().Username + "] " + Message, 30);
            }
        }

        public void JailAlert(string Message)
        {
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (!Groups.GroupManager.HasJobCommand(client, "radio") && !client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                    continue;

                if (client.GetRoleplay().DisableRadio)
                    continue;

                client.SendWhisper(Message, 30);
                client.SendMessage(new RoomNotificationComposer("police_announcement", "message", Message.Replace("[Alerta RÁDIO] ", "")));
            }
        }

        public void ModAlert(string Message)
        {
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") && !client.GetHabbo().GetPermissions().HasRight("staff_ignore_mod_alert"))
                {
                    try { client.SendWhisper(Message, 23); }
                    catch { }
                }
            }
        }

        public void DoAdvertisingReport(GameClient Reporter, GameClient Target)
        {
            if (Reporter == null || Target == null || Reporter.GetHabbo() == null || Target.GetHabbo() == null)
                return;

            StringBuilder Builder = new StringBuilder();
            Builder.Append("Nova denúncia enviado!\r\r");
            Builder.Append("Relator: " + Reporter.GetHabbo().Username + "\r");
            Builder.Append("Usuário denunciado: " + Target.GetHabbo().Username + "\r\r");
            Builder.Append(Target.GetHabbo().Username + " - 10 últimas mensagens\r\r");

            DataTable GetLogs = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `message` FROM `chatlogs` WHERE `user_id` = '" + Target.GetHabbo().Id + "' ORDER BY `id` DESC LIMIT 10");
                GetLogs = dbClient.getTable();

                if (GetLogs != null)
                {
                    int Number = 11;
                    foreach (DataRow Log in GetLogs.Rows)
                    {
                        Number -= 1;
                        Builder.Append(Number + ": " + Convert.ToString(Log["message"]) + "\r");
                    }
                }
            }

            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetHabbo() == null)
                    continue;

                if (Client.GetHabbo().GetPermissions().HasRight("mod_tool") && !Client.GetHabbo().GetPermissions().HasRight("staff_ignore_advertisement_reports"))
                    Client.SendMessage(new MOTDNotificationComposer(Builder.ToString()));
            }
        }

        public void SendMessage(ServerPacket Packet, string fuse = "")
        {
            foreach (GameClient Client in this._clients.Values.ToList())
            {
                if (Client == null || Client.GetHabbo() == null)
                    continue;

                if (!string.IsNullOrEmpty(fuse))
                {
                    if (!Client.GetHabbo().GetPermissions().HasRight(fuse))
                        continue;
                }

                Client.SendMessage(Packet);
            }
        }

        public void CreateAndStartClient(int clientID, ConnectionInformation connection)
        {
            GameClient Client = new GameClient(clientID, connection);
            if (this._clients.TryAdd(Client.ConnectionID, Client))
                Client.StartConnection();
            else
                connection.Dispose();
        }

        public void DisposeConnection(int clientID)
        {
            GameClient Client = null;
            if (!TryGetClient(clientID, out Client))
                return;

            if (Client != null)
                Client.Disconnect(false);

            this._clients.TryRemove(clientID, out Client);
        }

        public void LogClonesOut(int UserID)
        {
            GameClient client = GetClientByUserID(UserID);
            if (client != null)
            {
                client.Disconnect(true);
            }
        }

        public void RegisterClient(GameClient client, int userID, string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
                _usernameRegister[username.ToLower()] = client;
            else
                _usernameRegister.TryAdd(username.ToLower(), client);

            if (_userIDRegister.ContainsKey(userID))
                _userIDRegister[userID] = client;
            else
                _userIDRegister.TryAdd(userID, client);
        }

        public void UnregisterClient(int userid, string username)
        {
            GameClient Client = null;
            _userIDRegister.TryRemove(userid, out Client);
            _usernameRegister.TryRemove(username.ToLower(), out Client);
        }

        public void CloseAll()
        {
            #region Old Code (Un-necessary as GameClient.cs Disconnect() does it as well)
            /*
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null)
                    continue;

                if (client.GetHabbo() != null)
                {
                    try
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery(client.GetHabbo().GetQueryString);
                        }
                        Console.Clear();
                        log.Info("<<- SERVER SHUTDOWN ->> IVNENTORY IS SAVING");
                    }
                    catch
                    {
                    }
                }
            }

            log.Info("Done saving users inventory!");
            log.Info("Closing server connections...");
            */
            #endregion

            try
            {
                foreach (GameClient client in this.GetClients.ToList())
                {
                    if (client == null)
                        continue;

                    if (client.GetConnection() == null)
                        continue;

                    try
                    {
                        client.Disconnect(true);
                        client.GetConnection().Dispose();
                    }
                    catch { }

                    Console.Clear();
                    log.Info("<<- Servidor Desligado ->> Fechando conexões");

                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }

            if (this._clients.Count > 0)
                this._clients.Clear();

            log.Info("Conexões fechadas!");
        }

        private void TestClientConnections()
        {
            if (clientPingStopwatch.ElapsedMilliseconds >= 30000)
            {
                clientPingStopwatch.Restart();

                try
                {
                    List<GameClient> ToPing = new List<GameClient>();

                    foreach (GameClient client in this._clients.Values.ToList())
                    {
                        if (client.PingCount < 6)
                        {
                            client.PingCount++;

                            ToPing.Add(client);
                        }
                        else
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(client);
                            }
                        }
                    }

                    DateTime start = DateTime.Now;

                    foreach (GameClient Client in ToPing.ToList())
                    {
                        try
                        {
                            Client.SendMessage(new PongComposer());
                        }
                        catch
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(Client);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    log.Error("TestClientConnections Error!", e);
                }
            }
        }

        private void HandleTimeouts()
        {
            if (timedOutConnections.Count > 0)
            {
                lock (timedOutConnections.SyncRoot)
                {
                    while (timedOutConnections.Count > 0)
                    {
                        GameClient client = null;

                        if (timedOutConnections.Count > 0)
                            client = (GameClient)timedOutConnections.Dequeue();

                        if (client != null)
                        {
                            client.Disconnect(false);
                        }
                    }
                }
            }
        }

        public int Count
        {
            get { return this._clients.Count; }
        }

        public ICollection<GameClient> GetClients
        {
            get
            {
                return this._clients.Values;
            }
        }
    }
}