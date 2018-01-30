using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Plus.Core;
using Plus.HabboHotel.GameClients;
using System.Collections.Concurrent;
using Plus.Database.Interfaces;
using log4net;
using System.Threading;
using Plus.HabboRoleplay.Turfs;
using Plus.HabboRoleplay.Houses;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Farming;
using Plus.HabboRoleplay.Gambling;
using Plus.HabboRoleplay.Bots.Manager;

namespace Plus.HabboHotel.Rooms
{
    public class RoomManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Rooms.RoomManager");

        private Dictionary<string, RoomModel> _roomModels;

        public ConcurrentDictionary<int, Room> _rooms;
        private ConcurrentDictionary<int, RoomData> _loadedRoomData;


        private DateTime _cycleLastExecution;
        private DateTime _purgeLastExecution;


        public RoomManager()
        {
            this._roomModels = new Dictionary<string, RoomModel>();

            this._rooms = new ConcurrentDictionary<int, Room>();
            this._loadedRoomData = new ConcurrentDictionary<int, RoomData>();

            this.LoadModels();

            this._purgeLastExecution = DateTime.Now.AddHours(3);

            //log.Info("Room Manager -> LOADED");
        }

        public void OnCycle()
        {
            try
            {
                TimeSpan sinceLastTime = DateTime.Now - _cycleLastExecution;
                if (sinceLastTime.TotalMilliseconds >= 400)
                {
                    _cycleLastExecution = DateTime.Now;
                    foreach (Room Room in this._rooms.Values.ToList())
                    {
                        if (Room.isCrashed)
                            continue;

                        if (Room.ProcessTask == null || Room.ProcessTask.IsCompleted)
                        {
                            Room.ProcessTask = new Task(Room.ProcessRoom);
                            Room.ProcessTask.Start();
                            Room.IsLagging = 0;
                        }
                        else
                        {
                            Room.IsLagging++;
                            if (Room.IsLagging >= 30)
                            {
                                List<RoomUser> UsersToReturn = Room.GetRoomUserManager().GetRoomUsers().ToList();

                                Room.isCrashed = true;
                                UnloadRoom(Room);
                                Logging.WriteLine("[Gerenciador de Quartos] Quarto travado (A tarefa não foi concluída dentro de 30 segundos): " + Room.RoomId);

                                lock (UsersToReturn)
                                {
                                    foreach (RoomUser User in UsersToReturn)
                                    {
                                        if (User == null || User.GetClient() == null)
                                            continue;

                                        User.GetClient().SendMessage(new Communication.Packets.Outgoing.Notifications.MOTDNotificationComposer("O quarto em que você estava estava travou!\n\nUse o navegador ou clique no botão Início (na parte inferior esquerda) para voltar a uma sala!"));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Edite com o Gerenciador de Quartos: " + e);
            }
        }

        public int LoadedRoomDataCount
        {
            get { return this._loadedRoomData.Count; }
        }

        public int Count
        {
            get { return this._rooms.Count; }
        }

        public void LoadModels()
        {
            if (this._roomModels.Count > 0)
                _roomModels.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '0'");
                DataTable Data = dbClient.getTable();

                if (Data == null)
                    return;

                foreach (DataRow Row in Data.Rows)
                {
                    string Modelname = Convert.ToString(Row["id"]);
                    string staticFurniture = Convert.ToString(Row["public_items"]);

                    _roomModels.Add(Modelname, new RoomModel(Convert.ToInt32(Row["door_x"]), Convert.ToInt32(Row["door_y"]), (Double)Row["door_z"], Convert.ToInt32(Row["door_dir"]),
                        Convert.ToString(Row["heightmap"]), Convert.ToString(Row["public_items"]), PlusEnvironment.EnumToBool(Row["club_only"].ToString()), Convert.ToString(Row["poolmap"]), Convert.ToInt32(Row["wall_height"])));
                }
            }
        }

        public void LoadModel(string Id)
        {
            DataRow Row = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '1' AND `id` = '" + Id + "' LIMIT 1");
                Row = dbClient.getRow();

                if (Row == null)
                    return;

                string Modelname = Convert.ToString(Row["id"]);
                if (!this._roomModels.ContainsKey(Id))
                {
                    this._roomModels.Add(Modelname, new RoomModel(Convert.ToInt32(Row["door_x"]), Convert.ToInt32(Row["door_y"]), Convert.ToDouble(Row["door_z"]), Convert.ToInt32(Row["door_dir"]),
                      Convert.ToString(Row["heightmap"]), Convert.ToString(Row["public_items"]), PlusEnvironment.EnumToBool(Row["club_only"].ToString()), Convert.ToString(Row["poolmap"]), Convert.ToInt32(Row["wall_height"])));
                }
            }
        }

        public void ReloadModel(string Id)
        {
            if (!this._roomModels.ContainsKey(Id))
            {
                this.LoadModel(Id);
                return;
            }

            this._roomModels.Remove(Id);
            this.LoadModel(Id);
        }

        public bool TryGetModel(string Id, out RoomModel Model)
        {
            return this._roomModels.TryGetValue(Id, out Model);
        }

        public void UnloadRoom(Room Room, bool RemoveData = false)
        {
            if (Room == null)
                return;

            #region Roleplay Checks

            #region Turf
            Turf Turf = TurfManager.GetTurf(Room.RoomId);

            if (Turf != null)
            {
                Turf.Flag = null;
                Turf.FlagSpawned = false;
            }
            #endregion

            #region Texas Hold Em
            List<TexasHoldEm> Games = TexasHoldEmManager.GetGamesByRoomId(Room.Id);
            if (Games.Count > 0)
            {
                foreach (TexasHoldEm Game in Games)
                {
                    if (Game != null)
                    {
                        #region PotSquare Check
                        Game.PotSquare.Furni = null;
                        #endregion

                        #region JoinGate Check
                        Game.JoinGate.Furni = null;
                        #endregion

                        #region Player1 Check
                        foreach (TexasHoldEmItem Item in Game.Player1.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion

                        #region Player2 Check
                        foreach (TexasHoldEmItem Item in Game.Player2.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion

                        #region Player3 Check
                        foreach (TexasHoldEmItem Item in Game.Player3.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion

                        #region Banker Check
                        foreach (TexasHoldEmItem Item in Game.Banker.Values)
                        {
                            Item.Furni = null;
                        }
                        #endregion
                    }
                }
            }
            #endregion

            #region Farming
            List<FarmingSpace> FarmingSpaces = FarmingManager.GetFarmingSpacesByRoomId(Room.Id);
            if (FarmingSpaces.Count > 0)
            {
                foreach (FarmingSpace Space in FarmingSpaces)
                {
                    if (Space != null)
                    {
                        Space.Item = null;
                        Space.Spawned = false;
                    }
                }
            }
            #endregion

            #region Houses
            List<House> Houses = PlusEnvironment.GetGame().GetHouseManager().GetHousesBySignRoomId(Room.Id);
            if (Houses.Count > 0)
            {
                foreach (House House in Houses)
                {
                    if (House.Sign != null)
                    {
                        House.Sign.Item = null;
                        House.Sign.Spawned = false;
                    }
                }
            }
            #endregion

            #region Bots
            RoleplayBotManager.EjectRoomsDeployedBots(Room);
            #endregion

            #endregion

            new Thread(() => {
                Thread.Sleep(200);
                Room room = null;
                if (this._rooms.TryRemove(Room.RoomId, out room))
                {
                    Room.Dispose();

                    if (RemoveData)
                    {
                        RoomData Data = null;
                        this._loadedRoomData.TryRemove(Room.Id, out Data);
                    }
                }
            }).Start();
        }

        public List<RoomData> SearchGroupRooms(string Query)
        {
            IEnumerable<RoomData> InstanceMatches =
                (from RoomInstance in this._loadedRoomData
                 where /*RoomInstance.Value.UsersNow >= 0 &&*/
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE &&
                 RoomInstance.Value.Group != null &&
                 (RoomInstance.Value.OwnerName.StartsWith(Query) ||
                 RoomInstance.Value.Tags.Contains(Query) ||
                 RoomInstance.Value.Name.Contains(Query))
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> SearchTaggedRooms(string Query)
        {
            IEnumerable<RoomData> InstanceMatches =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow >= 0 &&
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE &&
                 (RoomInstance.Value.Tags.Contains(Query))
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> GetPopularRooms(int category, int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow > 0 &&
                 (category == -1 || RoomInstance.Value.Category == category) &&
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE
                 orderby RoomInstance.Value.Score descending
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRecommendedRooms(int Amount = 50, int CurrentRoomId = 0)
        {
            IEnumerable<RoomData> Rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow >= 0 &&
                 RoomInstance.Value.Score >= 0 &&
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE &&
                 RoomInstance.Value.Id != CurrentRoomId
                 orderby RoomInstance.Value.Score descending
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(Amount);
            return Rooms.ToList();
        }

        public List<RoomData> GetPopularRatedRooms(int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.Access != RoomAccess.INVISIBLE
                 orderby RoomInstance.Value.Score descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRoomsByCategory(int Category, int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.Category == Category &&
                 /*RoomInstance.Value.UsersNow > 0 &&*/
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE
                 orderby RoomInstance.Value.UsersNow descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetOnGoingRoomPromotions(int Mode, int Amount = 50)
        {
            IEnumerable<RoomData> Rooms = null;

            if (Mode == 17)
            {
                Rooms =
                    (from RoomInstance in this._loadedRoomData
                     where (RoomInstance.Value.HasActivePromotion) &&
                     RoomInstance.Value.Access != RoomAccess.INVISIBLE
                     orderby RoomInstance.Value.Promotion.TimestampStarted descending
                     select RoomInstance.Value).Take(Amount);
            }
            else
            {
                Rooms =
                    (from RoomInstance in this._loadedRoomData
                     where (RoomInstance.Value.HasActivePromotion) &&
                     RoomInstance.Value.Access != RoomAccess.INVISIBLE
                     orderby RoomInstance.Value.UsersNow descending
                     select RoomInstance.Value).Take(Amount);
            }

            return Rooms.ToList();
        }


        public List<RoomData> GetPromotedRooms(int CategoryId, int Amount = 50)
        {
            IEnumerable<RoomData> Rooms = null;

            Rooms =
                (from RoomInstance in this._loadedRoomData
                 where (RoomInstance.Value.HasActivePromotion) &&
                 RoomInstance.Value.Promotion.CategoryId == CategoryId &&
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE
                 orderby RoomInstance.Value.Promotion.TimestampStarted descending
                 select RoomInstance.Value).Take(Amount);

            return Rooms.ToList();
        }

        public List<KeyValuePair<string, int>> GetPopularRoomTags()
        {
            IEnumerable<List<string>> Tags =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.UsersNow >= 0 &&
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE
                 orderby RoomInstance.Value.UsersNow descending
                 orderby RoomInstance.Value.Score descending
                 select RoomInstance.Value.Tags).Take(50);

            Dictionary<string, int> TagValues = new Dictionary<string, int>();

            foreach (List<string> TagList in Tags)
            {
                foreach (string Tag in TagList)
                {
                    if (!TagValues.ContainsKey(Tag))
                    {
                        TagValues.Add(Tag, 1);
                    }
                    else
                    {
                        TagValues[Tag]++;
                    }
                }
            }

            List<KeyValuePair<string, int>> SortedTags = new List<KeyValuePair<string, int>>(TagValues);
            SortedTags.Sort((FirstPair, NextPair) =>
            {
                return FirstPair.Value.CompareTo(NextPair.Value);
            });

            SortedTags.Reverse();
            return SortedTags;
        }

        public List<RoomData> GetGroupRooms(int Amount = 50)
        {
            IEnumerable<RoomData> rooms =
                (from RoomInstance in this._loadedRoomData
                 where RoomInstance.Value.Group != null &&
                 RoomInstance.Value.Access != RoomAccess.INVISIBLE
                 orderby RoomInstance.Value.Score descending
                 select RoomInstance.Value).Take(Amount);
            return rooms.ToList();
        }

        public Room TryGetRandomLoadedRoom()
        {
            IEnumerable<Room> room =
                (from RoomInstance in this._rooms
                where (RoomInstance.Value.RoomData.UsersNow > 0 &&
                RoomInstance.Value.RoomData.Access == RoomAccess.OPEN &&
                RoomInstance.Value.RoomData.UsersNow < RoomInstance.Value.RoomData.UsersMax)
                orderby RoomInstance.Value.RoomData.UsersNow descending
                select RoomInstance.Value).Take(1);

            if (room.Count() > 0)
                return room.First();
            else
                return null;
        }

        public RoomModel GetModel(string Model)
        {
            if (_roomModels.ContainsKey(Model))
                return (RoomModel)_roomModels[Model];

            return null;
        }

        public void UpdateRoom(Room Room)
        {
            if (_loadedRoomData.ContainsKey(Room.Id))
                _loadedRoomData.TryUpdate(Room.Id, Room.RoomData, _loadedRoomData[Room.Id]);

            if (_rooms.ContainsKey(Room.Id))
                _rooms.TryUpdate(Room.Id, Room, _rooms[Room.Id]);
        }

        public RoomData GenerateRoomData(int RoomId)
        {
            if (_loadedRoomData.ContainsKey(RoomId))
                return (RoomData)_loadedRoomData[RoomId];

            RoomData Data = new RoomData();

            Room Room;

            if (TryGetRoom(RoomId, out Room))
                return Room.RoomData;

            DataRow Row = null;
            DataRow RPRow = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rooms` WHERE `id` = " + RoomId + " LIMIT 1");
                Row = dbClient.getRow();

                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `id` = " + RoomId + " LIMIT 1");
                RPRow = dbClient.getRow();
            }

            if (Row == null || RPRow == null)
                return null;

            Data.Fill(Row);
            Data.FillRP(RPRow);

            if (!_loadedRoomData.ContainsKey(RoomId))
                _loadedRoomData.TryAdd(RoomId, Data);

            return Data;
        }

        public RoomData FetchRoomData(int RoomId, DataRow dRow, DataRow dRowRP)
        {
            if (_loadedRoomData.ContainsKey(RoomId))
                return (RoomData)_loadedRoomData[RoomId];
            else
            {
                RoomData data = new RoomData();

                data.Fill(dRow);
                data.FillRP(dRowRP);

                if (!_loadedRoomData.ContainsKey(RoomId))
                    _loadedRoomData.TryAdd(RoomId, data);
                return data;
            }
        }

        public Room LoadRoom(int Id, bool BotCheck)
        {
            Room Room = null;

            if (TryGetRoom(Id, out Room))
            {
                return Room;
            }

            RoomData Data = GenerateRoomData(Id);
            if (Data == null)
                return null;

            Room = new Room(Data);

            if (!_rooms.ContainsKey(Room.RoomId))
            {
                _rooms.TryAdd(Room.RoomId, Room);
                new Thread(() => {
                    Thread.Sleep(2000);
                    RoleplayBotManager.DeployCachedBots(Room);
                }).Start();
            }

            return Room;
        }
        
        public bool TryGetRoom(int RoomId, out Room Room)
        {
            return this._rooms.TryGetValue(RoomId, out Room);
        }

        public RoomData CreateRoom(GameClient Session, string Name, string Description, string Model, int Category, int MaxVisitors, int TradeSettings)
        {
            if (!_roomModels.ContainsKey(Model))
            {
                Session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("room_model_missing"));
                return null;
            }

            if (Name.Length < 3)
            {
                Session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("room_name_length_short"));
                return null;
            }

            int RoomId = 0;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO `rp_rooms` (`id`) VALUES (0)");
                dbClient.SetQuery("INSERT INTO `rooms` (`roomtype`,`caption`,`description`,`owner`,`model_name`,`category`,`users_max`,`trade_settings`) VALUES ('private',@caption,@description,@UserId,@model,@category,@usersmax,@tradesettings)");
                dbClient.AddParameter("caption", Name);
                dbClient.AddParameter("description", Description);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.AddParameter("model", Model);
                dbClient.AddParameter("category", Category);
                dbClient.AddParameter("usersmax", MaxVisitors);
                dbClient.AddParameter("tradesettings", TradeSettings);

                RoomId = Convert.ToInt32(dbClient.InsertQuery());
            }

            RoomData newRoomData = GenerateRoomData(RoomId);
            Session.GetHabbo().UsersRooms.Add(newRoomData);
            return newRoomData;
        }

        public ICollection<Room> GetRooms()
        {
            return this._rooms.Values;
        }

        public void Dispose()
        {
            int length = _rooms.Count;
            int i = 0;
            foreach (Room Room in this._rooms.Values.ToList())
            {
                if (Room == null)
                    continue;

                PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room);
                Console.Clear();
                log.Info("<<- Servidor desligado ->> Salvando itens dos quartos: " + String.Format("{0:0.##}", ((double)i / length) * 100) + "%");
                i++;
            }
            log.Info("Feito!");
        }
    }
}