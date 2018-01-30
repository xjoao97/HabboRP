using System;
using System.Linq;
using System.Collections.Concurrent;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Timers;
using Plus.Database.Interfaces;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms.AI;
using System.Data;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Types;
using System.Drawing;
using Plus.Utilities;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Cooldowns;
using System.Threading;
using log4net;
using Plus.Core;
using Plus.HabboRoleplay.Bots.PetBots;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboRoleplay.Bots.Manager
{
    public class RoleplayBotManager
    {

        private static object SyncLock = new object();
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Roleplayer.Web.WebEventManager");
        public static ConcurrentDictionary<int, RoleplayBot> CachedRoleplayBots = new ConcurrentDictionary<int, RoleplayBot>();
        public static ConcurrentDictionary<int, RoomUser> DeployedRoleplayBots = new ConcurrentDictionary<int, RoomUser>();
        public static BotRoleplayTimer MainTimer = null;
        public static int BotFriendMultiplyer = 1000000;

        public static void Initialize(bool Refresh)
        {
            lock (SyncLock)
            {

                #region Refresh 
                if (Refresh)
                {
                    foreach (KeyValuePair<int, RoomUser> RoleplayBot in RoleplayBotManager.DeployedRoleplayBots)
                    {
                        if (RoleplayBot.Value == null) continue;
                        RoleplayBotManager.EjectDeployedBot(RoleplayBot.Value, RoleplayBot.Value.GetBotRoleplayAI().GetRoom());
                    }
                }
                #endregion

                #region Start Cycler
                if (RoleplayBotManager.MainTimer == null)
                    RoleplayBotManager.MainTimer = new RoleplayBotCycler();
                #endregion

                #region Cache & Deploy Bots
                new Thread(() =>
                {
                    Thread.Sleep(2000);
                    RoleplayBotManager.FetchCachedBots();
                    Thread.Sleep(100);
                    RoleplayBotManager.DeployCachedBots();
                }).Start();
                #endregion

            }
        }

        public static void FetchCachedBots()
        {
            lock (SyncLock)
            {
                RoleplayBotManager.CachedRoleplayBots = new ConcurrentDictionary<int, RoleplayBot>();
                DataTable RoleplayBotsTable;

                using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB.SetQuery("SELECT * FROM `rp_bots` WHERE `spawn_id` > '0'");
                    RoleplayBotsTable = DB.getTable();

                    foreach (DataRow RoleplayBot in RoleplayBotsTable.Rows)
                    {
                        int SpawnId = Convert.ToInt32(RoleplayBot["spawn_id"]);
                        RoleplayBotManager.CachedRoleplayBots.TryAdd(Convert.ToInt32(RoleplayBot["id"]),
                            new RoleplayBot(Convert.ToInt32(RoleplayBot["id"]), Convert.ToInt32(RoleplayBot["owner_id"]), Convert.ToString(RoleplayBot["name"]), Convert.ToString(RoleplayBot["gender"]), Convert.ToString(RoleplayBot["figure"]), Convert.ToString(RoleplayBot["motto"]), Convert.ToInt32(RoleplayBot["max_health"]), Convert.ToInt32(RoleplayBot["cur_health"]), Convert.ToInt32(RoleplayBot["str"]), Convert.ToInt32(RoleplayBot["level"]), (SpawnId == 0 ? 1 : SpawnId), Convert.ToInt32(RoleplayBot["spawn_x"]), Convert.ToInt32(RoleplayBot["spawn_y"]), Convert.ToInt32(RoleplayBot["spawn_z"]), Convert.ToInt32(RoleplayBot["spawn_rot"]), Convert.ToString(RoleplayBot["ai_type"]), RoleplayBotManager.GetRoleplayBotAIType(Convert.ToString(RoleplayBot["ai_type"])), Convert.ToInt32(RoleplayBot["roam_interval"]), Convert.ToInt32(RoleplayBot["attack_interval"]), Convert.ToInt32(RoleplayBot["follow_interval"]), Convert.ToInt32(RoleplayBot["stay_interval"]), PlusEnvironment.EnumToBool(Convert.ToString(RoleplayBot["roam_bot"])), PlusEnvironment.EnumToBool(Convert.ToString(RoleplayBot["roam_city_bot"])), PlusEnvironment.EnumToBool(Convert.ToString(RoleplayBot["addable_bot"])), Convert.ToInt32(RoleplayBot["corporation_id"]), Convert.ToString(RoleplayBot["stopwork_item"]), Convert.ToString(RoleplayBot["work_uniform"]), PlusEnvironment.EnumToBool(Convert.ToString(RoleplayBot["can_be_attacked"])), Convert.ToInt32(RoleplayBot["attack_pos"]), Convert.ToString(RoleplayBot["action_odds"]), Convert.ToInt32(RoleplayBot["speech_timer"]), Convert.ToString(RoleplayBot["pet_data"])));
                    }
                }

                new Thread(() =>
                {
                    Thread.Sleep(3000);
                    RoleplayBotManager.FetchCachedSpeeches();
                }).Start();
            }
        }

        public static void FetchCachedSpeeches()
        {
            foreach (RoleplayBot RoleplayBot in RoleplayBotManager.CachedRoleplayBots.Values)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT * from `rp_bots_responses` WHERE `bot_id` = '" + RoleplayBot.Id + "'");
                    DataTable BotResponseTable = dbClient.getTable();

                    if (BotResponseTable != null)
                    {
                        RoleplayBot.Responses.Clear();

                        foreach (DataRow Row in BotResponseTable.Rows)
                        {
                            string Message = Row["message"].ToString();
                            string Response = Row["response"].ToString();
                            int Bubble = Convert.ToInt32(Row["bubble"]);
                            string Type = Row["type"].ToString();

                            if (!RoleplayBot.Responses.ContainsKey(Message.ToLower()))
                                RoleplayBot.Responses.TryAdd(Message.ToLower(), new RoleplayBotResponse(Message.ToLower(), Response, Bubble, Type));
                        }

                        if (RoleplayBotManager.DeployedRoleplayBots.ContainsKey(RoleplayBot.Id))
                            RoleplayBotManager.DeployedRoleplayBots[RoleplayBot.Id].GetBotRoleplay().Responses = RoleplayBot.Responses;
                    }

                    dbClient.SetQuery("SELECT * from `rp_bots_speech` WHERE `bot_id` = '" + RoleplayBot.Id + "'");
                    DataTable BotSpeechTable = dbClient.getTable();

                    if (BotSpeechTable != null)
                    {
                        RoleplayBot.RandomSpeech.Clear();

                        foreach (DataRow Speech in BotSpeechTable.Rows)
                        {
                            string Text = Speech["text"].ToString();
                            int Id = RoleplayBot.Id;

                            var RandomSpeech = new RandomSpeech(Text, Id);

                            if (!RoleplayBot.RandomSpeech.Contains(RandomSpeech))
                                RoleplayBot.RandomSpeech.Add(RandomSpeech);
                        }

                        if (RoleplayBotManager.DeployedRoleplayBots.ContainsKey(RoleplayBot.Id))
                            RoleplayBotManager.DeployedRoleplayBots[RoleplayBot.Id].GetBotRoleplay().RandomSpeech = RoleplayBot.RandomSpeech;
                    }
                }
            }
        }

        public static void DeployCachedBots()
        {
            foreach (KeyValuePair<int, RoleplayBot> RoleplayBot in RoleplayBotManager.CachedRoleplayBots)
            {
                RoleplayBotManager.DeployBotByID(RoleplayBot.Key, "default");
                // Console.WriteLine("Deployed bot: " + RoleplayBot.Value.Name + " to room " + RoleplayBot.Value.SpawnId);
            }
        }

        public static void DeployCachedBots(Room Room)
        {
            foreach (RoleplayBot RoleplayBot in RoleplayBotManager.CachedRoleplayBots.Values.Where(CachedBot => CachedBot != null).Where(CachedBot => CachedBot.SpawnId == Room.Id))
            {
                /// Don't deploy delivery bot
                if (RoleplayBot.AIType == RoleplayBotAIType.DELIVERY)
                    continue;

                RoleplayBotManager.DeployBotByID(RoleplayBot.Id, "default");
            }
        }

        public static RoomUser DeployBotByID(int RoleplayBotsID, string SpawnType = "default", int RoomID = 0)
        {
            lock (SyncLock)
            {
                try
                {
                    if (!RoleplayBotManager.CachedRoleplayBots.ContainsKey(RoleplayBotsID))
                        return null;

                    #region If the bot already exists simply transfer it
                    RoleplayBot DeployingBot = RoleplayBotManager.CachedRoleplayBots[RoleplayBotsID];

                    if (DeployingBot.DRoomUser != null)
                    {
                        if (DeployingBot.DRoom != null)
                            if (DeployingBot.DRoom.Id != RoomID)
                                RoleplayBotManager.TransportDeployedBot(DeployingBot.DRoomUser, (RoomID == 0 ? DeployingBot.SpawnId : RoomID));
                        return null;
                    }
                    #endregion

                    Room RoleplayBotsRoom;

                    if (RoomID == 0)
                    {
                        if (RoleplayManager.GenerateRoom(DeployingBot.SpawnId, false) == null)
                            return null;
                        else RoleplayBotsRoom = RoleplayManager.GenerateRoom(DeployingBot.SpawnId, false);
                    }
                    else
                    {
                        if (RoleplayManager.GenerateRoom(RoomID, false) == null)
                            return null;
                        else RoleplayBotsRoom = RoleplayManager.GenerateRoom(RoomID, false);

                        DeployingBot.SpawnId = RoleplayBotsRoom.RoomId;
                    }

                    #region Misc variables
                    RoomUserManager RoomManager = RoleplayBotsRoom.GetRoomUserManager();
                    RoomUser BotsRoomUserInstance = new RoomUser(0, RoleplayBotsRoom.RoomId, RoomManager.primaryPrivateUserID++, RoleplayBotsRoom);
                    List<RandomSpeech> BotsSpeech = new List<RandomSpeech>();
                    int BotsPersonalID = RoomManager.secondaryPrivateUserID++;
                    BotsRoomUserInstance.InternalRoomID = BotsPersonalID;
                    RoomManager._users.TryAdd(BotsPersonalID, BotsRoomUserInstance);
                    DynamicRoomModel RoleplayBotsModel = RoleplayBotsRoom.GetGameMap().Model;
                    #endregion

                    #region Spawning & Positioning
                    if (DeployingBot.Dead)
                    {
                        RoleplayManager.SpawnBeds(null, "hosptl_bed", BotsRoomUserInstance);
                    }
                    else
                    {

                        #region Spawn at origin
                        if (SpawnType == "default")
                        {
                            if ((DeployingBot.X > 0 && DeployingBot.Y > 0) && DeployingBot.X < RoleplayBotsModel.MapSizeX && DeployingBot.Y < RoleplayBotsModel.MapSizeY)
                            {
                                BotsRoomUserInstance.SetPos(DeployingBot.X, DeployingBot.Y, DeployingBot.Z);
                                BotsRoomUserInstance.SetRot(DeployingBot.SpawnRot, false);
                            }
                            else
                            {
                                DeployingBot.X = RoleplayBotsModel.DoorX;
                                DeployingBot.Y = RoleplayBotsModel.DoorY;

                                BotsRoomUserInstance.SetPos(RoleplayBotsModel.DoorX, RoleplayBotsModel.DoorY, RoleplayBotsModel.DoorZ);
                                BotsRoomUserInstance.SetRot(RoleplayBotsModel.DoorOrientation, false);
                            }
                        }
                        #endregion

                        #region Spawn at work item
                        if (SpawnType == "workitem")
                        {
                            Item Item;
                            if (DeployingBot.GetStopWorkItem(RoleplayBotsRoom, out Item))
                            {
                                var Point = new Point(Item.GetX, Item.GetY);

                                BotsRoomUserInstance.X = Point.X;
                                BotsRoomUserInstance.Y = Point.Y;

                                BotsRoomUserInstance.SetPos(Point.X, Point.Y, RoleplayBotsRoom.GetGameMap().GetHeightForSquare(Point));
                                BotsRoomUserInstance.SetRot(Item.Rotation, false);
                            }
                            else
                            {
                                BotsRoomUserInstance.X = RoleplayBotsModel.DoorX;
                                BotsRoomUserInstance.Y = RoleplayBotsModel.DoorY;

                                BotsRoomUserInstance.SetPos(RoleplayBotsModel.DoorX, RoleplayBotsModel.DoorY, RoleplayBotsModel.DoorZ);
                                BotsRoomUserInstance.SetRot(RoleplayBotsModel.DoorOrientation, false);
                            }
                        }
                        #endregion

                        #region Spawn at owner
                        
                        if (SpawnType == "owner" && DeployingBot.Owner != null)
                        {
                            RoomUser OwnerInstance = DeployingBot.Owner.GetRoomUser();
                            int X = OwnerInstance.SquareInFront.X;
                            int Y = OwnerInstance.SquareInFront.Y;
                            double Z = OwnerInstance.Z;

                            if ( (OwnerInstance != null) && (X > 0 && Y > 0) && X < RoleplayBotsModel.MapSizeX && Y < RoleplayBotsModel.MapSizeY)
                            {
                                BotsRoomUserInstance.SetPos(X, Y, Z);
                                BotsRoomUserInstance.SetRot(DeployingBot.SpawnRot, false);
                            }
                            else
                            {

                                X = OwnerInstance.X;
                                Y = OwnerInstance.Y;
                                DeployingBot.X = X;
                                DeployingBot.Y = Y;

                                BotsRoomUserInstance.SetPos(X, Y, OwnerInstance.Z);
                                BotsRoomUserInstance.SetRot(DeployingBot.SpawnRot, false);
                            }
                        }
                        #endregion
                    }
                    #endregion

                    #region Generate Roleplay Bot's data, timers

                    BotsRoomUserInstance.BotData = new RoomBot(DeployingBot.Id, DeployingBot.SpawnId, DeployingBot.AITypeString, "stand", DeployingBot.Name, "Motto", DeployingBot.Figure, DeployingBot.X, DeployingBot.Y, DeployingBot.Z, DeployingBot.SpawnRot, DeployingBot.X, DeployingBot.Y, DeployingBot.X, DeployingBot.Y, ref BotsSpeech, DeployingBot.Gender, 0, 0, false, 0, false, 0);
                    BotsRoomUserInstance.RPBotAI = RoleplayBotManager.GetRoleplayBotAI(DeployingBot.AIType, BotsRoomUserInstance.VirtualId);
                    BotsRoomUserInstance.BotAI = BotsRoomUserInstance.BotData.GenerateBotAI(BotsRoomUserInstance.VirtualId);

                    if (DeployingBot.IsPet)
                    {
                        BotsRoomUserInstance.PetData = DeployingBot.PetInstance;
                        BotsRoomUserInstance.PetData.VirtualId = BotsRoomUserInstance.VirtualId;
                    }

                    BotsRoomUserInstance.RPBotAI.Init(DeployingBot.Id, DeployingBot, BotsRoomUserInstance.VirtualId, DeployingBot.SpawnId, BotsRoomUserInstance, RoleplayBotsRoom);

                    BotsRoomUserInstance.BotAI.Init(DeployingBot.Id, BotsRoomUserInstance.VirtualId, DeployingBot.SpawnId, BotsRoomUserInstance, RoleplayBotsRoom);

                    if (DeployingBot.TimerManager == null)
                        DeployingBot.TimerManager = new BotTimerManager(DeployingBot);
                    else
                        DeployingBot.TimerManager.CachedBot = DeployingBot;

                    if (DeployingBot.CooldownManager == null)
                        DeployingBot.CooldownManager = new BotCooldownManager(DeployingBot);
                    else
                        DeployingBot.CooldownManager.CachedBot = DeployingBot;

                    RoomManager.UpdateUserStatus(BotsRoomUserInstance, false);
                    BotsRoomUserInstance.UpdateNeeded = true;

                    #region Set general variables
                    DeployingBot.Teleporting = false;
                    DeployingBot.TeleporterEntering = null;
                    DeployingBot.TeleporterExiting = null;
                    DeployingBot.Teleported = false;
                    DeployingBot.Deployed = true;
                    DeployingBot.RoomStayTime = new CryptoRandom().Next(0, DeployingBot.RoomStayInterval);
                    DeployingBot.FollowCooldown = new CryptoRandom().Next(0, DeployingBot.FollowInterval);
                    DeployingBot.AttackCooldown = new CryptoRandom().Next(0, DeployingBot.AttackInterval);
                    DeployingBot.RoamCooldown = new CryptoRandom().Next(0, DeployingBot.RoamInterval);
                    DeployingBot.RoomStayTime = new CryptoRandom().Next(0, DeployingBot.RoomStayInterval);
                    DeployingBot.LookCooldown = new CryptoRandom().Next(0, DeployingBot.LookCooldown);
                    #endregion

                    #endregion

                    #region Compose Roleplay Bot's visibility & insert into deployed bots

                    DeployingBot.Invisible = false;

                    if (!DeployingBot.Invisible)
                        RoleplayBotsRoom.SendMessage(new UsersComposer(BotsRoomUserInstance));


                    if (RoomManager._bots.ContainsKey(DeployingBot.Id))
                        RoomManager._bots[DeployingBot.Id] = BotsRoomUserInstance;
                    else
                        RoomManager._bots.TryAdd(DeployingBot.Id, BotsRoomUserInstance);

                    RoleplayBotsRoom.SendMessage(new DanceComposer(BotsRoomUserInstance, 0));

                    #endregion

                    #region Attach RoleplayBot Instance to RoomUser & deliver its respective data
                    BotsRoomUserInstance.RPBotData = DeployingBot;
                    if (!RoleplayBotManager.DeployedRoleplayBots.ContainsKey(DeployingBot.Id))
                        RoleplayBotManager.DeployedRoleplayBots.TryAdd(DeployingBot.Id, BotsRoomUserInstance);
                    else
                    {
                        RoomUser NullBot;
                        RoleplayBotManager.ClearRoleplayBotClones(DeployingBot.Id);
                        RoleplayBotManager.DeployedRoleplayBots.TryAdd(DeployingBot.Id, BotsRoomUserInstance);
                    }


                    #region Start roaming
                    if (DeployingBot.RoamBot)
                        DeployingBot.MoveRandomly();
                    #endregion

                    DeployingBot.DRoomUser.GetBotRoleplayAI().OnDeployed(DeployingBot.Owner);

                    #endregion

                    return BotsRoomUserInstance;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return null;
        }

        public static RoomUser DeployBotByAI(RoleplayBotAIType BotAI, string SpawnType = "default", int RoomID = 0)
        {
            lock (SyncLock)
            {
                try
                {
                    RoleplayBot Bot = RoleplayBotManager.GetCachedBotByAI(BotAI);

                    if (Bot == null)
                        return null;

                    return RoleplayBotManager.DeployBotByID(Bot.Id);
                }
                catch (Exception ex)
                {
                    RoleplayBotManager.OnError(ex.Message);
                }
            }

            return null;
        }

        public static RoleplayBotAIType GetRoleplayBotAIType(string StringType)
        {
            switch (StringType.ToLower())
            {
                case "quest":
				case "tarefa":
                    return RoleplayBotAIType.QUEST;
                case "thug":
				case "vidaloka":
                    return RoleplayBotAIType.THUG;
                case "jury":
                case "court":
				case "juiz":
                    return RoleplayBotAIType.JURY;
                case "hosp":
                case "hospital":
                    return RoleplayBotAIType.HOSP;
                case "gun":
                case "gunstore":
                case "gun_store":
                case "ammu":
                case "ammunation":
				case "armas":
                    return RoleplayBotAIType.GUNSTORE;
                case "phone":
                case "phonestore":
                case "phone_store":
				case "celulares":
                    return RoleplayBotAIType.PHONESTORE;
                case "car":
                case "carstore":
                case "car_store":
				case "carros":
                    return RoleplayBotAIType.CARSTORE;
                case "bank":
                case "bank worker":
				case "banco":
                    return RoleplayBotAIType.BANKWORKER;
                case "food":
                case "food server":
                case "food_server":
				case "restaurante":
				case "comida":
                    return RoleplayBotAIType.FOODSERVER;
                case "drink":
                case "drinks":
                case "drink server":
                case "drinks server":
                case "drink_server":
                case "drinks_server":
				case "bebidas":
				case "servir":
                    return RoleplayBotAIType.DRINKSERVER;
                case "drug":
                case "drugseller":
				case "drogas":
                    return RoleplayBotAIType.DRUGSELLER;
                case "plant":
                case "plantseller":
				case "plantas":
				case "planta":
                    return RoleplayBotAIType.PLANTSELLER;
                case "business":
                case "job":
                case "mayor":
				case "emprego":
				case "vagas":
				case "empregos":
                    return RoleplayBotAIType.BUSINESS;
                case "supermarket":
				case "mercado":
                    return RoleplayBotAIType.SUPERMARKET;
                case "delivery":
                case "delivery bot":
                case "delivery worker":
				case "entregador":
                    return RoleplayBotAIType.DELIVERY;
                case "mw":
                case "mafiawars":
				case "gm":
                    return RoleplayBotAIType.MAFIAWARS;
                default:
                    return RoleplayBotAIType.BLANK;
            }
        }

        public static RoleplayBotAI GetRoleplayBotAI(RoleplayBotAIType Type, int VirtualId)
        {
            switch (Type)
            {
                case RoleplayBotAIType.THUG:
                    return new ThugBot(VirtualId);
                case RoleplayBotAIType.QUEST:
                    return new QuestBot(VirtualId);
                case RoleplayBotAIType.JURY:
                    return new JuryBot(VirtualId);
                case RoleplayBotAIType.HOSP:
                    return new HospitalBot(VirtualId);
                case RoleplayBotAIType.GUNSTORE:
                    return new GunStoreBot(VirtualId);
                case RoleplayBotAIType.PHONESTORE:
                    return new PhoneStoreBot(VirtualId);
                case RoleplayBotAIType.CARSTORE:
                    return new CarStoreBot(VirtualId);
                case RoleplayBotAIType.SUPERMARKET:
                    return new SupermarketBot(VirtualId);
                case RoleplayBotAIType.BANKWORKER:
                    return new BankWorkerBot(VirtualId);
                case RoleplayBotAIType.DRINKSERVER:
                    return new DrinkServerBot(VirtualId);
                case RoleplayBotAIType.FOODSERVER:
                    return new FoodServerBot(VirtualId);
                case RoleplayBotAIType.BUSINESS:
                    return new BusinessBot(VirtualId);
                case RoleplayBotAIType.DELIVERY:
                    return new DeliveryBot(VirtualId);
                case RoleplayBotAIType.DRUGSELLER:
                    return new DrugSellerBot(VirtualId);
                case RoleplayBotAIType.PLANTSELLER:
                    return new PlantSellerBot(VirtualId);
                case RoleplayBotAIType.MAFIAWARS:
                    return new MafiaWarsBot(VirtualId);
                case RoleplayBotAIType.PET:
                    return new PetBot(VirtualId);
                default:
                    return new BlankBot(VirtualId);
            }
        }

        public static RoomUser GetDeployedBotById(int BotsID)
        {

            RoomUser DeployedBot = null;

            if (RoleplayBotManager.DeployedRoleplayBots == null) return null;
            if (RoleplayBotManager.DeployedRoleplayBots.Count <= 0) return null;

            foreach (RoomUser DeployedBots in RoleplayBotManager.DeployedRoleplayBots.Values)
            {
                if (DeployedBots == null) continue;
                if (DeployedBots.GetBotRoleplay() == null) continue;
                if (DeployedBots.GetBotRoleplay().Id != BotsID) continue;

                DeployedBot = DeployedBots;
            }

            return DeployedBot;
        }

        public static RoomUser GetDeployedBotByName(string BotsName)
        {
            RoomUser DeployedBot = null;

            foreach (RoomUser DeployedBots in RoleplayBotManager.DeployedRoleplayBots.Values)
            {
                if (DeployedBots == null) continue;
                if (DeployedBots.GetBotRoleplay() == null) continue;
                if (DeployedBots.GetBotRoleplay().Name.ToLower() != BotsName.ToLower()) continue;

                DeployedBot = DeployedBots;
            }

            return DeployedBot;
        }

        public static RoleplayBot GetCachedBotById(int BotsID)
        {
            RoleplayBot CachedBot = null;

            foreach (RoleplayBot CachedBots in RoleplayBotManager.CachedRoleplayBots.Values)
            {
                if (CachedBots == null) continue;
                if (CachedBots.Id != BotsID) continue;

                CachedBot = CachedBots;
            }

            return CachedBot;
        }

        public static RoleplayBot GetCachedBotByAI(RoleplayBotAIType BotAI)
        {
            RoleplayBot CachedBot = null;

            foreach (RoleplayBot CachedBots in RoleplayBotManager.CachedRoleplayBots.Values)
            {
                if (CachedBots == null) continue;
                if (CachedBots.AIType != BotAI) continue;

                CachedBot = CachedBots;
            }

            return CachedBot;
        }

        public static void ClearRoleplayBotClones(int BotsID)
        {
            if (RoleplayBotManager.GetDeployedBotById(BotsID) == null) return;

            RoleplayBotManager.EjectDeployedBot(RoleplayBotManager.GetDeployedBotById(BotsID), RoleplayBotManager.GetDeployedBotById(BotsID).GetRoom());
        }

        public static bool EjectDeployedBot(RoomUser RoleplayBot, Room Room, bool SaveDataToCache = false)
        {

            #region Conditions & null checks
            if (RoleplayBot == null) return false;
            if (RoleplayBot.GetBotRoleplay() == null) return false;
            if (RoleplayBot.GetBotRoleplayAI() == null) return false;
            if (Room == null) return false;
            if (Room.GetGameMap() == null) return false;
            if (!RoleplayBotManager.DeployedRoleplayBots.ContainsKey(RoleplayBot.GetBotRoleplay().Id)) return false;
            #endregion

            #region Additional null checks
            if (RoleplayBotManager.DeployedRoleplayBots[RoleplayBot.GetBotRoleplay().Id] == null) return false;
            if (RoleplayBotManager.DeployedRoleplayBots[RoleplayBot.GetBotRoleplay().Id].GetBotRoleplay() == null) return false;
            #endregion

            #region End any ongoing actions / timers 

            #endregion

            #region Save to cache
            if (SaveDataToCache)
                RoleplayBotManager.SaveDeployedBotsData(RoleplayBot);
            #endregion


            RoleplayBotManager.DeployedRoleplayBots.TryRemove(RoleplayBot.GetBotRoleplay().Id, out RoleplayBot);
            Room.GetRoomUserManager().RemoveBot(RoleplayBot.VirtualId, false);
            RoleplayBot.GetBotRoleplay().Invisible = true;

            return true;

        }

        public static bool EjectRoomsDeployedBots(Room Room)
        {
            try
            {
                foreach (RoomUser RoleplayBot in RoleplayBotManager.DeployedRoleplayBots.Values)
                {
                    if (RoleplayBot == null) continue;
                    if (RoleplayBot.GetRoom() == null) continue;

                    if (RoleplayBot.GetRoom() == Room)
                        RoleplayBotManager.EjectDeployedBot(RoleplayBot, RoleplayBot.GetRoom());

                    return true;
                }
            }
            catch (Exception ex)
            {
                RoleplayBotManager.OnError(ex.Message);
            }

            return false;
        }

        public static bool TransportDeployedBot(RoomUser RoleplayBot, int NewRoomID, bool SaveDataToCache = true)
        {
            if (RoleplayBot == null) return false;
            if (RoleplayBot.GetBotRoleplay() == null) return false;

            int BotsID = RoleplayBot.GetBotRoleplay().Id;

            try
            {

                Room NewRoom;
                if (RoleplayManager.GenerateRoom(NewRoomID, false) == null)
                    return false;
                else NewRoom = RoleplayManager.GenerateRoom(NewRoomID, false);


                RoleplayBotManager.EjectDeployedBot(RoleplayBot, RoleplayBot.GetRoom(), SaveDataToCache);
                RoleplayBotManager.DeployBotByID(BotsID, "default", NewRoom.Id);


                //Console.WriteLine("Transfered bot " + RoleplayBotManager.GetDeployedBotById(BotsID).GetBotRoleplay().Name + " to [" + NewRoom.Id + "] " + NewRoom.Name);
                return true;
            }
            catch (Exception ex)
            {
                RoleplayBotManager.OnError(ex.Message);
                return false;
            }
        }

        private static void SaveDeployedBotsData(RoomUser RoleplayBot)
        {
            if (RoleplayBotManager.CachedRoleplayBots.ContainsKey(RoleplayBot.GetBotRoleplay().Id))
                RoleplayBotManager.CachedRoleplayBots[RoleplayBot.GetBotRoleplay().Id] = RoleplayBot.GetBotRoleplay();
            else RoleplayBotManager.CachedRoleplayBots.TryAdd(RoleplayBot.GetBotRoleplay().Id, RoleplayBot.GetBotRoleplay());
        }

        public static void OnError(string Error)
        {
            Logging.LogRPBotError(Error);
        }
    }
}

