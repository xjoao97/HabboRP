using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Bots.Types;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Items;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Core;
using Plus.HabboHotel.Pathfinding;
using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboRoleplay.Cooldowns;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using System.Collections.Concurrent;
using Plus.HabboRoleplay.Timers;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using System.Data;
using Plus.HabboRoleplay.Bots.Manager.TimerHandlers;
using static Plus.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;
using Plus.HabboRoleplay.Bots.Manager.TimerHandlers.Types;

namespace Plus.HabboRoleplay.Bots
{
    public class RoleplayBot : IDisposable
    {

        #region Fundamentals
        public int Id { get; set; }
        public string Name { get; set; }
        public string Figure { get; set; }
        public string Gender { get; set; }
        public string Motto { get; set; }
        public int VirtualId { get; set; }
        public int MaxHealth { get; set; }
        public int CurHealth { get; set; }
        public int Strength { get; set; }
        public int Level { get; set; }
        public int SpawnId { get; set; }
        public int OriginalId { get; set; }
        public bool Deployed { get; set; }
        public bool Dead { get; set; }
        public bool Jailed { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int oX { get; set; }
        public int oY { get; set; }
        public double Z { get; set; }
        public double oZ { get; set; }
        public int SpawnRot { get; set; }
        public RoomBot RoomBotInstance { get; set; }
        public Habbo HabboInstance { get; set; }
        public ConcurrentDictionary<string, RoleplayBotResponse> Responses { get; set; }
        public List<RandomSpeech> RandomSpeech { get; set; }
        public int RandomSpeechTimer { get; set; }
        public BotTimerManager TimerManager { get; set; }
        public BotCooldownManager CooldownManager { get; set; }
        #endregion

        #region Instances
        public Room GenerateSpawnRoom
        {
            get { return (SpawnId <= 0) ? null : RoleplayManager.GenerateRoom(this.SpawnId, false); }
        }
        public RoomUser DRoomUser
        {
            // RoomUser of the DEPLOYED bots instance
            get { if (GenerateSpawnRoom == null || !Deployed) return null; return this.GetDeployedInstance(); }
        }
        public Room DRoom
        {
            // Room of the DEPLOYED bots instance
            get { if (GenerateSpawnRoom == null) return null; if (DRoomUser == null) return null; return DRoomUser.GetRoom(); }
        }
        #endregion

        #region State and Interaction
        public GameClient UserFollowing { get; set; }
        public GameClient UserAttacking { get; set; }
        public bool Attacking { get; set; }
        public int DefaultAttackPosition { get; set; }
        public bool Roaming { get; set; }
        public bool Following { get; set; }
        public bool RoamBot { get; set; }
        public bool RoamCityBot { get; set; }
        public bool AddableBot { get; set; }
        public bool Invisible { get; set; }
        public int Corporation { get; set; }
        public string WorkUniform { get; set; }
        public string ItemWalkingTo { get; set; }
        public bool WalkingToItem { get; set; }
        public bool CanBeAttacked { get; set; }
        public ConcurrentDictionary<string, BotRoleplayTimer> ActiveTimers { get; set; }
        #endregion

        #region Games
        public bool Boss { get; set; }
        public Games.RoleplayTeam Team { get; set; }

        #endregion

        #region Teleporting
        public Item TeleporterEntering { get; set; }
        public Item TeleporterExiting { get; set; }
        public Item LastTeleport { get; set; }
        public bool Teleporting { get; set; }
        public bool Teleported { get; set; }
        #endregion

        #region Cooldowns / Waitings
        public int FollowCooldown { get; set; }
        public int AttackCooldown { get; set; }
        public int RoamCooldown { get; set; }
        public int LookCooldown { get; set; }

        public int RoamInterval { get; set; }
        public int AttackInterval { get; set; }
        public int FollowInterval { get; set; }

        public int RoomStayInterval { get; set; }
        public int RoomStayTime { get; set; }
        #endregion

        #region Misc
        public RoleplayBotAIType AIType { get; set; }
        public string AITypeString { get; set; }
        public ConcurrentDictionary<Handlers, IBotHandler> ActiveHandlers = new ConcurrentDictionary<Handlers, IBotHandler>();
        public int OriginalMaxOdds { get; protected set; }
        public int OriginalMinOdds { get; protected set; }
        public int MaxOdds { get; set; }
        public int MinOdds { get; set; }
        #endregion

        #region Pet Vars
        public int OwnerId { get; set; }
        public string PetData { get; set; }
        public Pet PetInstance { get; set; }
        public bool IsPet { get { return (this.PetData.Length > 1); } }
        public GameClient Owner { get { if (PlusEnvironment.GetGame() == null || PlusEnvironment.GetGame().GetClientManager() == null) return null; return ((this.OwnerId <= 0) ? null : PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(this.OwnerId)); } }
        #endregion

        /// <summary>
        /// Initialises RoleplayBot
        /// </summary>
        public RoleplayBot(int Id, int OwnerId, string Name, string Gender, string Figure, string Motto, int MaxHealth, int CurHealth, int Strength, int Level, int SpawnId, int X, int Y, double Z, int SpawnRot, string AITypeString, RoleplayBotAIType AIType, int RoamInterval, int AttackInterval, int FollowInterval, int RoomStayInterval, bool RoamBot, bool RoamCityBot, bool AddableBot, int Corporation, string StopWorkItem, string WorkUniform, bool CanBeAttacked, int DefaultAttackPosition, string Odds, int RandomSpeechTimer, string PetData)
        {
            this.Id = Id;
            this.OwnerId = OwnerId;
            this.Name = Name;
            this.Gender = Gender;
            this.Figure = Figure;
            this.Motto = Motto;
            this.MaxHealth = MaxHealth;
            this.CurHealth = CurHealth;
            this.Strength = Strength;
            this.Level = Level;
            this.SpawnId = SpawnId;
            this.VirtualId = -1;
            this.Deployed = false;
            this.Dead = false;
            this.Jailed = false;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.oX = X;
            this.oY = Y;
            this.oZ = Z;

            this.AITypeString = AITypeString;
            this.AIType = AIType;

            this.SpawnRot = SpawnRot;
            this.RandomSpeechTimer = RandomSpeechTimer;
            this.OriginalId = SpawnId;

            this.WalkingToItem = false;
            this.ItemWalkingTo = StopWorkItem;
            this.Corporation = Corporation;
            this.WorkUniform = WorkUniform;

            List<RandomSpeech> Null = new List<RandomSpeech>();
            this.RoomBotInstance = new RoomBot(Id, SpawnId, "", "", Name, "", Figure, X, Y, Z, SpawnRot, 0, 0, 0, 0, ref Null, Gender, 0, 0, false, 0, false, 0);

            this.RoamBot = RoamBot;
            this.RoamCityBot = RoamCityBot;
            this.AddableBot = AddableBot;
            this.CanBeAttacked = CanBeAttacked;

            this.UserFollowing = null;
            this.UserAttacking = null;
            this.Attacking = false;
            this.Roaming = RoamBot;
            this.Following = false;
            this.DefaultAttackPosition = DefaultAttackPosition;

            this.FollowCooldown = new CryptoRandom().Next(0, FollowInterval);
            this.AttackCooldown = new CryptoRandom().Next(0, AttackInterval);
            this.RoamCooldown = new CryptoRandom().Next(0, RoamInterval);
            this.RoomStayTime = new CryptoRandom().Next(0, RoomStayInterval);
            this.LookCooldown = new CryptoRandom().Next(0, LookCooldown);

            this.RoamInterval = RoamInterval;
            this.AttackInterval = AttackInterval;
            this.FollowInterval = FollowInterval;
            this.RoomStayInterval = RoomStayInterval;

            this.TeleporterEntering = null;
            this.TeleporterExiting = null;
            this.LastTeleport = null;
            this.Teleporting = false;
            this.Teleported = false;

            this.Boss = false;
            this.Team = null;

            this.HabboInstance = new Habbo(Id + RoleplayBotManager.BotFriendMultiplyer, Name, 1, Motto, Figure, Gender, 0, 0, SpawnId, false, 0, false, false, 0, 0, "", "", false, false, false, false, false, 0, 0, false, 0, 0, false, 0, false, false, 0, true, string.Empty);
            this.Invisible = false;

            this.ActiveTimers = new ConcurrentDictionary<string, BotRoleplayTimer>();
            this.Responses = new ConcurrentDictionary<string, RoleplayBotResponse>();
            this.RandomSpeech = new List<RandomSpeech>();

            this.OriginalMaxOdds = Convert.ToInt32(Odds.Split(',')[1]);
            this.OriginalMinOdds = Convert.ToInt32(Odds.Split(',')[0]);

            this.MaxOdds = OriginalMaxOdds;
            this.MinOdds = OriginalMinOdds;

            this.PetData = PetData;
            string[] PetParts = this.PetData.Replace(" ", "").Split('|');

            if (this.IsPet)
                this.PetInstance = new Pet(this.Id, this.OwnerId, this.SpawnId, this.Name, Convert.ToInt32(PetParts[0]), PetParts[1], PetParts[2], 0, 0, 0, 0, 0, this.X, this.Y, this.Z, 0, 0, 0, 0, null);
            else this.PetInstance = null;
        }

        /// <summary>
        /// Follow cooldown
        /// </summary>
        /// <returns></returns>
        public bool FollowCoolingDown()
        {
            if (FollowCooldown > 0)
            {
                FollowCooldown--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attack cooldown
        /// </summary>
        /// <returns></returns>
        public bool AttackCoolingDown()
        {
            if (AttackCooldown > 0)
            {
                AttackCooldown--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Roam cooldown
        /// </summary>
        /// <returns></returns>
        public bool RoamCoolingDown()
        {
            if (RoamCooldown > 0)
            {
                RoamCooldown--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Follows a user
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        public void StartFollow(RoomUser Self, GameClient Following)
        {
            RoomUser User = Following.GetRoomUser();
            if (User == null) return;

            int NewX = User.X;
            int NewY = User.Y;

            #region Rotation
            if (User.RotBody == 4)
            {
                NewY = User.Y + 1;
            }
            else if (User.RotBody == 0)
            {
                NewY = User.Y - 1;
            }
            else if (User.RotBody == 6)
            {
                NewX = User.X - 1;
            }
            else if (User.RotBody == 2)
            {
                NewX = User.X + 1;
            }
            else if (User.RotBody == 3)
            {
                NewX = User.X + 1;
                NewY = User.Y + 1;
            }
            else if (User.RotBody == 1)
            {
                NewX = User.X + 1;
                NewY = User.Y - 1;
            }
            else if (User.RotBody == 7)
            {
                NewX = User.X - 1;
                NewY = User.Y - 1;
            }
            else if (User.RotBody == 5)
            {
                NewX = User.X - 1;
                NewY = User.Y + 1;
            }
            #endregion

            // Self.Chat("moving TO " + NewX + "|" + NewY, true);
            Self.MoveTo(NewX, NewY);
        }

        /// <summary>
        /// Roams a room
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        /// <param name="Room">Bot's room</param>
        public void MoveRandomly()
        {
            if (this.DRoom == null)
                return;
            if (this.DRoom.GetGameMap() == null)
                return;
            if (this.DRoom.GetGameMap().Model == null)
                return;
            if (this.DRoomUser == null)
                return;

            int randomX = new CryptoRandom().Next(0, this.DRoom.GetGameMap().Model.MapSizeX);
            int randomY = new CryptoRandom().Next(0, this.DRoom.GetGameMap().Model.MapSizeY);

            if (this.DRoomUser.CanWalk)
                this.DRoomUser.MoveTo(randomX, randomY);
        }

        /// <summary>
        /// Enters a teleport/arrow
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        /// <param name="Room">Bot's room</param>
        /// <param name="Params">Teleport data</param>
        public void StartTeleporting(RoomUser Self, Room Room, object[] Params)
        {
            object[] Bits = Params;

            int ItemX = Convert.ToInt32(Bits[0]);
            int ItemY = Convert.ToInt32(Bits[1]);
            int LastRoomId = Convert.ToInt32(Bits[2]);
            int NextRoomId = Convert.ToInt32(Bits[3]);
            int LastTeleId = Convert.ToInt32(Bits[4]);
            int NextTeleId = Convert.ToInt32(Bits[5]);


            TeleporterEntering = Room.GetRoomItemHandler().GetItem(LastTeleId);
            if (TeleporterEntering == null)
                return;

            int LinkedTele = ItemTeleporterFinder.GetLinkedTele(TeleporterEntering.Id, Room);
            int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);
            Room NewRoom = HabboRoleplay.Misc.RoleplayManager.GenerateRoom(TeleRoomId);

            if (NewRoom == null)
            {
                TeleporterExiting = null;
                return;
            }

            if (NewRoom.GetRoomItemHandler() == null)
            {
                TeleporterExiting = null;
                return;
            }

            if (NewRoom.GetRoomItemHandler().GetItem(LinkedTele) == null)
            {
                TeleporterExiting = null;
                return;
            }

            TeleporterExiting = NewRoom.GetRoomItemHandler().GetItem(LinkedTele);
            Teleporting = true;
        }

        /// <summary>
        /// Handles the court trial case
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        /// <param name="Room">Bot's room</param>
        public void HandleJuryCase(RoomUser Self, Room Room)
        {
            if (Self == null || Room == null)
                return;

            RoleplayBot Bot = Self.GetBotRoleplay();
            if (Bot != null && Bot.TimerManager != null && Bot.TimerManager.ActiveTimers != null && !Bot.TimerManager.ActiveTimers.ContainsKey("jury"))
            {
                Bot.TimerManager.CreateTimer("jury", this, 1000, true);
            }
            return;
        }

        /// <summary>
        /// Handles teleporting
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        /// <param name="Room">Bot's room</param>
        public void HandleTeleporting(RoomUser Self, Room Room)
        {
            if (Self == null || Room == null)
                return;

            RoleplayBot Bot = Self.GetBotRoleplay();
            if (!Bot.Teleported)
            {
                if (Bot.TeleporterEntering == null)
                {
                    Bot.Teleporting = false;
                    return;
                }

                if (Self.Coordinate == Bot.TeleporterEntering.Coordinate)
                {
                    // Lets teleport the bot!
                    Self.GetBotRoleplayAI().GetRoom().SendMessage(new UserRemoveComposer(Self.VirtualId));
                    Bot.Teleported = true;
                    Bot.X = Bot.TeleporterExiting.GetX;
                    Bot.Y = Bot.TeleporterExiting.GetY;
                    Bot.SpawnId = Bot.TeleporterExiting.RoomId;
                    Bot.LastTeleport = Bot.TeleporterExiting;
                    Room TeleRoom = RoleplayManager.GenerateRoom(Bot.TeleporterExiting.RoomId);
                    RoleplayBotManager.TransportDeployedBot(Self, TeleRoom.Id, true);

                    if (Bot != null && Bot.TimerManager != null && Bot.TimerManager.ActiveTimers != null && Bot.TimerManager.ActiveTimers.ContainsKey("teleport"))
                    {
                        Bot.TimerManager.ActiveTimers["teleport"].EndTimer();

                        if (Bot.UserAttacking != null && Bot.UserAttacking.GetHabbo() != null)
                            Bot.TimerManager.CreateTimer("attack", this, 10, true, Bot.UserAttacking.GetHabbo().Id);
                        else
                            Bot.MoveRandomly();
                    }
                }
                else
                {
                    // Lets make the bot walk to the teleport!
                    Self.MoveTo(Bot.TeleporterEntering.GetX, Bot.TeleporterEntering.GetY);
                }
            }
            return;
        }

        public RoomUser GetDeployedInstance()
        {
            return RoleplayBotManager.GetDeployedBotById(this.Id);
        }

        /// <summary>
        /// Handles roaming
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        /// <param name="Room">Bot's room</param>
        public void HandleRoaming()
        {
            if (this.DRoomUser == null)
                return;
            if (this.RoamCoolingDown())
            {
                if (!DRoomUser.IsWalking)
                    this.HandleLookAround();
            }
            else
            {
                this.MoveRandomly();
                this.RoamCooldown = this.RoamInterval;
                return;
            }
        }

        /// <summary>
        /// Handles bot looking around room
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        /// <param name="Room">Bot's room</param>
        public void HandleLookAround()
        {
            this.LookCooldown = new CryptoRandom().Next(0, 15);

            int Rand = new CryptoRandom().Next(1, 5);

            if (this.LookCooldown == Rand)
            {
                this.DRoomUser.RotBody = new CryptoRandom().Next(0, 7);
                this.DRoomUser.RotHead = this.DRoomUser.RotBody;
                this.DRoomUser.UpdateNeeded = true;
            }
        }

        /// <summary>
        /// Gets a random teleport
        /// </summary>
        /// <param name="Self">Bot's roomuser instance</param>
        /// <param name="Room">Bot's room</param>
        public Item GetRandomTeleport()
        {
            ConcurrentDictionary<Item, int> Teleports = new ConcurrentDictionary<Item, int>();
            Item RandTele = null;

            if (this.DRoomUser == null)
                return null;

            Room Room = this.DRoom;

            if (Room == null)
                return null;
            if (Room.GetRoomItemHandler() == null)
                return null;
            if (Room.GetRoomItemHandler().GetFloor == null)
                return null;

            lock (Room.GetRoomItemHandler().GetFloor)
            {
                #region Grab available teleports
                foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                {
                    if (item == null)
                        continue;
                    if (item.GetBaseItem() == null)
                        continue;

                    if (item.GetBaseItem().InteractionType == InteractionType.ARROW)
                    {
                        if (!Teleports.ContainsKey(item))
                        {
                            Teleports.TryAdd(item, item.Id);
                        }
                    }
                }
                #endregion

                #region Remove Last used teleport
                lock (Teleports)
                {
                    if (Teleports.Count > 1 && LastTeleport != null)
                    {
                        foreach (Item Teleport in Teleports.Keys)
                        {
                            if (Teleport == LastTeleport)
                            {
                                int Id;
                                Teleports.TryRemove(Teleport, out Id);
                            }
                        }
                    }
                }
                #endregion

                #region Grab random teleport
                CryptoRandom Random = new CryptoRandom();
                if (Teleports.Count >= 1)
                {

                    List<Item> Teles = Enumerable.ToList(Teleports.Keys);

                    if (Teleports.Count == 1)
                    {
                        RandTele = Teles[0];
                    }
                    else
                    {
                        RandTele = Teles[Random.Next(0, Teleports.Count)];
                    }

                }
                #endregion
            }

            if (RandTele == null)
                return null;

            return RandTele;
        }

        /// <summary>
        /// Gets the linked teleport for a target teleport
        /// </summary>
        /// <param name="EnteringTeleport"></param>
        /// <returns></returns>
        public Item GetLinkedTeleport(Item EnteringTeleport)
        {

            int LinkedTele = ItemTeleporterFinder.GetLinkedTele(EnteringTeleport.Id, this.DRoom);
            int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, this.DRoom);

            if (TeleRoomId <= 0)
                return null;

            Room NewRoom = RoleplayManager.GenerateRoom(TeleRoomId);

            if (NewRoom == null)
                return null;
            if (NewRoom.GetRoomItemHandler() == null)
                return null;

            Item Teleport = NewRoom.GetRoomItemHandler().GetItem(LinkedTele);

            return Teleport;
        }

        /// <summary>
        /// Gets a target item
        /// </summary>
        /// <param name="ItemName"></param>
        /// <param name="Self"></param>
        /// <param name="Room"></param>
        /// <returns></returns>
        public Item GetItem(string ItemName, RoomUser Self, Room Room)
        {
            ConcurrentDictionary<Item, int> Items = new ConcurrentDictionary<Item, int>();
            Item Item = null;

            if (Room.GetRoomItemHandler().GetFloor == null)
                return null;

            lock (Room.GetRoomItemHandler().GetFloor)
            {

                #region Grab available <item>
                foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                {
                    if (item == null)
                        continue;
                    if (item.GetBaseItem() == null)
                        continue;

                    if (item.GetBaseItem().ItemName.ToLower() == ItemName.ToLower())
                    {
                        if (!Items.ContainsKey(item))
                        {
                            Items.TryAdd(item, item.Id);
                        }
                    }
                }
                #endregion

                #region Grab Random of item
                CryptoRandom Random = new CryptoRandom();
                if (Items.Count >= 1)
                {

                    List<Item> ItemsList = Enumerable.ToList(Items.Keys);

                    if (Items.Count == 1)
                    {
                        Item = ItemsList[0];
                    }
                    else
                    {
                        Item = ItemsList[Random.Next(0, Items.Count)];
                    }
                    
                }
                else
                {
                    //Console.WriteLine("Couldnt find item '" + ItemName.ToLower() + "' ;; GetItem() void");
                }
                #endregion

            }

            return Item;
        }

        /// <summary>
        /// Causes a selected bot to message a selected user
        /// </summary>
        /// <param name="FriendId">ID of user bot is messaging</param>
        /// <param name="Message">Message the bot is sending</param>
        /// <returns></returns>
        public bool MessageFriend(int FriendId, string Message)
        {
            GameClient Friend = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(FriendId);

            if (Friend == null)
                return false;

            if (Friend.GetRoleplay() == null)
                return false;

            if (Friend.GetRoleplay().BotFriendShips == null)
                return false;

            if (!Friend.GetRoleplay().FriendsWithBot(Id + RoleplayBotManager.BotFriendMultiplyer))
                return false;

            Friend.SendMessage(new NewConsoleMessageComposer(Id + RoleplayBotManager.BotFriendMultiplyer, Message));
            return true;
        }

        /// <summary>
        /// Gets the stop work item
        /// </summary>
        /// <param name="Room"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        public bool GetStopWorkItem(Room Room, out Item Item)
        {
            Item = null;
            var Items = Room.GetRoomItemHandler().GetFloor;
            bool HasStopworkItem = Items.ToList().Where(x => x.GetBaseItem().ItemName == this.ItemWalkingTo).ToList().Count() > 0;

            if (HasStopworkItem)
            {
                if (this.ItemWalkingTo != "none")
                {
                    Item = Items.FirstOrDefault(x => x.GetBaseItem().ItemName == this.ItemWalkingTo);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Initiates the bots death
        /// </summary>
        public void InitiateDeath()
        {
            this.Dead = true;
            this.UserAttacking = null;
            this.TimerManager.EndAllTimers();
            this.CurHealth = this.MaxHealth;

            RoomUser Bot = RoleplayBotManager.GetDeployedBotById(this.Id);

            if (Bot == null)
                return;

            int HospitalRoomId = Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2"));
            Room Hospital = RoleplayManager.GenerateRoom(HospitalRoomId);

            RoleplayBotManager.TransportDeployedBot(Bot, HospitalRoomId, true);
            this.TimerManager.CreateTimer("botdeath", this, 1000, true);

        }

        /// <summary>
        /// Gets the bots outfit
        /// </summary>
        /// <returns></returns>
        public string[] GetOutFit()
        {
            string[] Outfit = new string[2];

            Outfit[0] = this.Figure;
            Outfit[1] = this.Motto;

            RoomUser User = RoleplayBotManager.GetDeployedBotById(this.Id);

            if (this.Dead)
            {
                if (this.Gender.ToLower() == "m")
                    Outfit[0] = RoleplayManager.SplitFigure(this.Figure, "lg-280-83.ch-215-83");

                if (this.Gender.ToLower() == "f")
                    Outfit[0] = RoleplayManager.SplitFigure(this.Figure, "lg-710-83.ch-635-83");

                Outfit[1] = "[MORTO] Paciente do Hospital";
            }
            else if (this.Jailed)
            {
                Random Random = new Random();
                int PrisonNumber = Random.Next(10000, 100000);

                if (Gender.ToLower() == "m")
                    Outfit[0] = RoleplayManager.SplitFigure(this.Figure, "lg-280-1323.sh-3016-92.ch-220-1323");

                if (Gender.ToLower() == "f")
                    Outfit[0] = RoleplayManager.SplitFigure(this.Figure, "lg-710-1323.sh-3016-92.ch-3067-1323");

                Outfit[1] = "[PRESO] ID do Criminoso [#" + PrisonNumber + "]";
            }
            else if (User != null && User.GetBotRoleplayAI() != null && User.GetBotRoleplayAI().OnDuty)
            {
                if (this.WorkUniform != "Nenhum")
                    Outfit[0] = RoleplayManager.SplitFigure(this.Figure, this.WorkUniform);

                Outfit[1] = "[TRABALHANDO] [" + GroupManager.GetJob(this.Corporation).Name + "]";
            }

            return Outfit;
        }

        /// <summary>
        /// Gets the bots cooldown
        /// </summary>
        /// <param name="cooldown"></param>
        /// <returns></returns>
        public bool TryGetCooldown(string cooldown)
        {
            if (this.CooldownManager != null && this.CooldownManager.ActiveCooldowns != null)
            {
                if (this.CooldownManager.ActiveCooldowns.ContainsKey(cooldown.ToLower()))
                {
                    var CoolDown = this.CooldownManager.ActiveCooldowns[cooldown.ToLower()];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Disposes of the roleplay bots instance
        /// </summary>
        public void Dispose()
        {

            if (DRoomUser != null)
            {
                if (DRoomUser.GetBotRoleplayAI() != null)
                    DRoomUser.GetBotRoleplayAI().StopActivities();

                RoleplayBotManager.EjectDeployedBot(this.DRoomUser, this.DRoom, false);
            }

            Id = 0;
            Name = null;
            Figure = null;
            Gender = null;
            Motto = null;
            VirtualId = 0;
            MaxHealth = 0;
            CurHealth = 0;
            Strength = 0;
            Level = 0;
            SpawnId = 0;
            OriginalId = 0;
            Deployed = false;
            Dead = false;
            Jailed = false;
            X = 0;
            Y = 0;
            oX = 0;
            oY = 0;
            Z = 0;
            oZ = 0;
            SpawnRot = 0;
            RoomBotInstance = null;
            HabboInstance = null;
            Responses = null;
            UserFollowing = null;
            UserAttacking = null;
            Attacking = false;
            Roaming = false;
            Following = false;
            RoamBot = false;
            RoamCityBot = false;
            AddableBot = false;
            Invisible = false;
            TeleporterEntering = null;
            TeleporterExiting = null;
            LastTeleport = null;
            Teleporting = false;
            Teleported = false;
            FollowCooldown = 0;
            AttackCooldown = 0;
            RoamCooldown = 0;
            LookCooldown = 0;

            RoamInterval = 0;
            AttackInterval = 0;
            FollowInterval = 0;

            RoomStayInterval = 0;
            RoomStayTime = 0;
            AIType = 0;

            TimerManager.EndAllTimers();
            TimerManager = null;

            CooldownManager.EndAllCooldowns();
            CooldownManager = null;

            this.RoomBotInstance.Dispose();
        }

        public void StopAllHandlers()
        {
            foreach(IBotHandler Handler in this.ActiveHandlers.Values)
            {
                if (Handler == null) continue;
                Handler.AbortHandler();
            }
        }

        public void StopHandler(Handlers Handler)
        {
            if (!this.ActiveHandlers.ContainsKey(Handler))
                return;

            this.ActiveHandlers[Handler].AbortHandler();
        }

        public bool TryGetHandler(Handlers Handler, out IBotHandler HandlerOut)
        {
            HandlerOut = null;

            if (this.ActiveHandlers == null) return false;
            if (!this.ActiveHandlers.ContainsKey(Handler)) return false;

            HandlerOut = this.ActiveHandlers[Handler];

            return true;
        }

        public void StartHandler(Handlers Handler, out IBotHandler HandlerOut, params object[] Params)
        {

            HandlerOut = null;

            #region Null checks
            if (this == null)
                return;
            if (this.DRoomUser == null)
                return;

            Room Room = this.DRoom;

            if (Room == null)
                return;
            #endregion

            if (this.ActiveHandlers.ContainsKey(Handler))
            {
                IBotHandler I;
                this.ActiveHandlers.TryRemove(Handler, out I);
            }
            
            
            switch (Handler)
            {

                case Handlers.TELEPORT:
                    HandlerOut = new TeleportHandler(this, (Item)Params[0]);
                     this.Teleporting = true;
                    break;
                case Handlers.ATTACK:
                    HandlerOut = new AttackHandler(this, (GameClient)Params[0]);                  
                    this.Attacking = true;
                    break;
                case Handlers.DELIVERY:
                    HandlerOut = new DeliveryHandler(this);
                    break;
                case Handlers.FOODSERVE:
                    HandlerOut = new FoodServeHandler(this, (GameClient)Params[0], (Food.Food)Params[1], (Point)Params[2], (Point)Params[3]);
                    break;
                default:

                    break;
            }

            this.ActiveHandlers.TryAdd(Handler, HandlerOut);

        }
    }
}
