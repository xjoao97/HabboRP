using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using log4net;
using Plus.Core;
using System.Text;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;

using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Inventory.Badges;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Rooms.Session;

namespace Plus.Messages.Net
{

    public class MusConnection
    {
        private Socket _socket;
        private byte[] _buffer = new byte[1024];

        private static readonly ILog log = LogManager.GetLogger("Plus.Messages.Net.MusConnection");

        public MusConnection(Socket Socket)
        {
            this._socket = Socket;

            try
            {
                this._socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnEvent_RecieveData, _socket);
            }
            catch { this.tryClose(); }
        }

        public void tryClose()
        {
            try
            {
                this._socket.Shutdown(SocketShutdown.Both);
                this._socket.Close();
                this._socket.Dispose();
            }
            catch
            {
            }

            this._socket = null;
            this._buffer = null;
        }

        public void OnEvent_RecieveData(IAsyncResult iAr)
        {
            try
            {
                int bytes = 0;

                try
                {
                    bytes = _socket.EndReceive(iAr);
                }
                catch
                {
                    tryClose();
                    return;
                }

                String data = Encoding.Default.GetString(_buffer, 0, bytes);

                if (data.Length > 0)
                    processCommand(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            tryClose();
        }

        public void processCommand(String data)
        {
            GameClient Client = null;

            String header = data.Split(Convert.ToChar(1))[0];
            String param = data.Split(Convert.ToChar(1))[1];

            string[] Params = param.ToString().Split(':');

            switch (header.ToLower())
            {
                #region User Related
                #region :reload_credits <UserID>
                case "reload_credits":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        int Credits = 0;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `credits` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", UserId);
                            Credits = dbClient.getInteger();
                        }

                        Client.GetHabbo().Credits = Credits;
                        Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                        break;
                    }
                #endregion
                #region :reload_pixels <UserID>
                case "reload_pixels":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        int Pixels = 0;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `activity_points` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", UserId);
                            Pixels = dbClient.getInteger();
                        }

                        Client.GetHabbo().Duckets = Pixels;
                        Client.SendMessage(new HabboActivityPointNotificationComposer(Client.GetHabbo().Duckets, Pixels));
                        break;
                    }
                #endregion
                #region :reload_diamonds <UserID>
                case "reload_diamonds":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        int Diamonds = 0;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `vip_points` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", UserId);
                            Diamonds = dbClient.getInteger();
                        }

                        Client.GetHabbo().Diamonds = Diamonds;
                        Client.SendMessage(new HabboActivityPointNotificationComposer(Diamonds, 0, 5));
                        break;
                    }
                #endregion
                #region :reload_eventp <UserID>
                case "reload_eventp":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        int EventPoints = 0;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `event_points` FROM `users` WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("id", UserId);
                            EventPoints = dbClient.getInteger();
                        }

                        Client.GetHabbo().EventPoints = EventPoints;
                        Client.SendMessage(new HabboActivityPointNotificationComposer(EventPoints, 0, 103));
                        break;
                    }
                #endregion
                #region :reload_user_rank userID
                case "reload_user_rank":
                    {
                        int UserId = Convert.ToInt32(Params[0]);

                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `rank` FROM `users` WHERE `id` = @userID LIMIT 1");
                            dbClient.AddParameter("userID", UserId);
                            Client.GetHabbo().Rank = dbClient.getInteger();
                            Client.GetHabbo().GetPermissions().Init(Client.GetHabbo());
                        }
                        break;
                    }
                #endregion
                #region :reload_user_vip userID
                case "reload_user_vip":
                    {
                        int UserId = Convert.ToInt32(Params[0]);

                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `rank_vip` FROM `users` WHERE `id` = @userID LIMIT 1");
                            dbClient.AddParameter("userID", UserId);
                            Client.GetHabbo().VIPRank = dbClient.getInteger();
                            Client.GetHabbo().GetPermissions().Init(Client.GetHabbo());
                        }
                        break;
                    }
                #endregion
                #region :reload_motto userID
                case "reload_motto":
                    {
                        int UserId = Convert.ToInt32(Params[0]);

                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `motto` FROM `users` WHERE `id` = @userID LIMIT 1");
                            dbClient.AddParameter("userID", UserId);
                            Client.GetHabbo().Motto = dbClient.getString();
                        }

                        if (Client.GetHabbo().InRoom)
                        {
                            Room Room = Client.GetHabbo().CurrentRoom;
                            if (Room == null)
                                return;

                            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                            if (User == null || User.GetClient() == null)
                                return;

                            Room.SendMessage(new UserChangeComposer(User, false));
                        }
                        break;
                    }
                #endregion
                #region :alert_user <userid> <message>
                case "alert":
                case "alert_user":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        string alertMessage = Convert.ToString(Params[1]);

                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        Client.SendMessage(new BroadcastMessageAlertComposer(alertMessage));
                        break;
                    }
                #endregion
                #region :reload_badges <UserID>
                case "update_badges":
                case "reload_badges":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

                        if (Client != null)
                        {
                            if (Client.GetHabbo() != null)
                            {
                                Client.SendMessage(new BadgesComposer(Client));
                            }
                        }
                        break;
                    }
                #endregion
                #region :givebadge <UserID> <badge>
                case "givebadge":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        string badgeCode = Convert.ToString(Params[1]);
                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

                        if (Client != null)
                        {
                            if (Client.GetHabbo() != null)
                            {
                                Client.GetHabbo().GetBadgeComponent().GiveBadge(badgeCode, true, Client);
                            }
                        }
                        break;
                    }
                #endregion
                #region :disconnect <username>
                case "disconnect":
                    {
                        try
                        {
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(Params[0]));
                            if (TargetClient != null && TargetClient.GetConnection() != null)
                                TargetClient.GetConnection().Dispose();
                        }
                        catch
                        {
                            log.Error("Error disconnecting user using MUS");
                        }
                        return;
                    }
                #endregion
                #region :reload_last_change userID
                case "reload_last_change":
                    {
                        int UserId = Convert.ToInt32(Params[0]);

                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `last_change` FROM `users` WHERE `id` = @userID LIMIT 1");
                            dbClient.AddParameter("userID", UserId);
                            Client.GetHabbo().LastNameChange = dbClient.getInteger();
                        }
                        break;
                    }
                #endregion
                #region :goto <UserID> <RoomID>
                case "goto":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        int RoomId = Convert.ToInt32(Params[1]);

                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        if (!int.TryParse(Params[1], out RoomId))
                            break;
                        else
                        {
                            Room _room = HabboRoleplay.Misc.RoleplayManager.GenerateRoom(RoomId);
                            if (_room == null)
                                Client.SendNotification("Failed to find the requested room!");
                            else
                            {
                                if (!Client.GetHabbo().InRoom)
                                    Client.SendMessage(new RoomForwardComposer(_room.Id));
                                else
                                    Client.GetHabbo().PrepareRoom(_room.Id, "");
                            }
                        }
                    }
                    break;
                #endregion
                #region :reload_user_color userID
                case "reload_user_color":
                    {
                        int UserId = Convert.ToInt32(Params[0]);

                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT `colour` FROM `users` WHERE `id` = @userID LIMIT 1");
                            dbClient.AddParameter("userID", UserId);
                            Client.GetHabbo().Colour = dbClient.getString();
                        }
                        break;
                    }
                #endregion
                #endregion

                #region Fastfood
                #region :progress_achievement
                case "progress_achievement":
                    {
                        int UserId = Convert.ToInt32(Params[0]);
                        Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                        if (Client == null || Client.GetHabbo() == null)
                            break;

                        string Achievement = Convert.ToString(Params[1]);
                        int Progress = Convert.ToInt32(Params[2]);

                        PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, Achievement, Progress);
                        break;
                    }
                #endregion
                #endregion

                #region Settings related
                #region :reload_filter/:update_filter
                case "update_filter":
                case "reload_filter":
                case "recache_filter":
                case "refresh_filter":
                    {
                        PlusEnvironment.GetGame().GetChatManager().GetFilter().Init();
                        break;
                    }
                #endregion
                #region :reload_catalog/:reload_catalog
                case "update_catalog":
                case "reload_catalog":
                case "recache_catalog":
                case "refresh_catalog":
                case "update_catalogue":
                case "reload_catalogue":
                case "recache_catalogue":
                case "refresh_catalogue":
                    {
                        PlusEnvironment.GetGame().GetCatalog().Init(PlusEnvironment.GetGame().GetItemManager());
                        PlusEnvironment.GetGame().GetClientManager().SendMessage(new CatalogUpdatedComposer());
                        break;
                    }
                #endregion
                #region :reload_items/:update_items
                case "update_items":
                case "reload_items":
                case "recache_items":
                case "refresh_items":
                    {
                        PlusEnvironment.GetGame().GetItemManager().Init();
                        break;
                    }
                #endregion
                #region :reload_navigator/:update_navigator
                case "update_navigator":
                case "reload_navigator":
                case "recache_navigator":
                case "refresh_navigator":
                    {
                        PlusEnvironment.GetGame().GetNavigator().Init();
                        break;
                    }
                #endregion
                #region :reload_ranks/:update_ranks
                case "update_ranks":
                case "reload_ranks":
                case "recache_ranks":
                case "refresh_ranks":
                    {
                        PlusEnvironment.GetGame().GetPermissionManager().Init();

                        foreach (GameClient C in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            if (C == null || C.GetHabbo() == null || C.GetHabbo().GetPermissions() == null)
                                continue;

                            C.GetHabbo().GetPermissions().Init(Client.GetHabbo());
                        }
                        break;
                    }
                #endregion
                #region :reload_settings/:update_settings
                case "update_settings":
                case "reload_settings":
                case "recache_settings":
                case "refresh_settings":
                    {
                        PlusEnvironment.ConfigData = new ConfigData();
                        break;
                    }
                #endregion
                #region :reload_quests/:update_quests
                case "reload_quests":
                case "update_quests":
                    {
                        PlusEnvironment.GetGame().GetQuestManager().Init();
                        break;
                    }
                #endregion
                #region :reload_vouchers/:update_vouchers
                case "reload_vouchers":
                case "update_vouchers":
                    {
                        PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().Init();
                        break;
                    }
                #endregion
                #region :reload_bans/:update_bans
                case "update_bans":
                case "reload_bans":
                    {
                        PlusEnvironment.GetGame().GetModerationManager().ReCacheBans();
                        break;
                    }
                #endregion
                #endregion

                #region Misc
                #region :hotel_alert
                case "hotel_alert":
                case "alert_hotel":
                    {
                        string Message = Convert.ToString(Params[0]);

                        PlusEnvironment.GetGame().GetClientManager().SendMessage(new BroadcastMessageAlertComposer(Message));
                        break;
                    }
                #endregion
                #region :whisper_hotel_alert
                case "whisper_hotel_alert":
                case "whisper_alert_hotel":
                    {
                        string Message = Convert.ToString(Params[0]);

                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
                        {
                            foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                if (client == null || client.GetHabbo() == null)
                                    continue;

                                client.SendWhisper("[HOTEL Alert] " + Message, 33);
                            }
                        }
                        break;
                    }
                #endregion
                #endregion

                default:
                    {
                        log.Error("Pacote MUS não reconhecido: '" + header + "'");
                        return;
                    }
            }

            log.Info("Successfully Parsed MUS command: '" + header + "'");
        }
    }
}