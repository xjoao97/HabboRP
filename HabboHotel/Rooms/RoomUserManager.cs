using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;

using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Global;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms.Games;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Inventory;
using Plus.Communication.Packets.Incoming;

using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;

using Plus.Utilities;

using System.Data;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Handshake;
using System.Text.RegularExpressions;
using Plus.HabboHotel.Rooms.Games.Teams;

using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Events;
using Plus.HabboRoleplay.Games;
using Plus.HabboHotel.Roleplay.Web.Incoming.Interactions;
using Plus.HabboRoleplay.Gambling;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUserManager
    {
        private Room _room;
        public ConcurrentDictionary<int, RoomUser> _users;
        public ConcurrentDictionary<int, RoomUser> _bots;
        private ConcurrentDictionary<int, RoomUser> _pets;

        public int primaryPrivateUserID;
        public int secondaryPrivateUserID;

        public int userCount;
        private int petCount;


        public RoomUserManager(Room room)
        {
            this._room = room;
            this._users = new ConcurrentDictionary<int, RoomUser>();
            this._pets = new ConcurrentDictionary<int, RoomUser>();
            this._bots = new ConcurrentDictionary<int, RoomUser>();

            this.primaryPrivateUserID = 0;
            this.secondaryPrivateUserID = 0;

            this.petCount = 0;
            this.userCount = 0;
        }

        public void Dispose()
        {
            this._users.Clear();
            this._pets.Clear();
            this._bots.Clear();

            this._users = null;
            this._pets = null;
            this._bots = null;
        }

     
        public RoomUser DeployBot(RoomBot Bot, Pet PetData)
        {
            var BotUser = new RoomUser(0, _room.RoomId, primaryPrivateUserID++, _room);
            Bot.VirtualId = primaryPrivateUserID;

            int PersonalID = secondaryPrivateUserID++;
            BotUser.InternalRoomID = PersonalID;
            _users.TryAdd(PersonalID, BotUser);

            DynamicRoomModel Model = _room.GetGameMap().Model;

            if ((Bot.X > 0 && Bot.Y > 0) && Bot.X < Model.MapSizeX && Bot.Y < Model.MapSizeY)
            {
                BotUser.SetPos(Bot.X, Bot.Y, Bot.Z);
                BotUser.SetRot(Bot.Rot, false);
            }
            else
            {
                Bot.X = Model.DoorX;
                Bot.Y = Model.DoorY;

                BotUser.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                BotUser.SetRot(Model.DoorOrientation, false);
            }

            BotUser.BotData = Bot;
            BotUser.BotAI = Bot.GenerateBotAI(BotUser.VirtualId);

            if (BotUser.IsPet)
            {
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);
                BotUser.PetData = PetData;
                BotUser.PetData.VirtualId = BotUser.VirtualId;
            }
            else
                BotUser.BotAI.Init(Bot.BotId, BotUser.VirtualId, _room.RoomId, BotUser, _room);

            UpdateUserStatus(BotUser, false);
            BotUser.UpdateNeeded = true;

            _room.SendMessage(new UsersComposer(BotUser));

            if (BotUser.IsPet)
            {
                if (_pets.ContainsKey(BotUser.PetData.PetId)) //Pet allready placed
                    _pets[BotUser.PetData.PetId] = BotUser;
                else
                    _pets.TryAdd(BotUser.PetData.PetId, BotUser);

                petCount++;
            }
            else if (BotUser.IsBot)
            {
                if (_bots.ContainsKey(BotUser.BotData.BotId))
                    _bots[BotUser.BotData.BotId] = BotUser;
                else
                    _bots.TryAdd(BotUser.BotData.Id, BotUser);
                _room.SendMessage(new DanceComposer(BotUser, BotUser.BotData.DanceId));
            }
            return BotUser;
        }


        public void RemoveBot(int VirtualId, bool Kicked)
        {
            RoomUser User = GetRoomUserByVirtualId(VirtualId);
            if (User == null || !User.IsBot)
                return;

            if (User.IsPet)
            {
                RoomUser PetRemoval = null;

                _pets.TryRemove(User.PetData.PetId, out PetRemoval);
                petCount--;
            }
            else
            {
                RoomUser BotRemoval = null;
                _bots.TryRemove(User.BotData.Id, out BotRemoval);
            }

            User.BotAI.OnSelfLeaveRoom(Kicked);

            _room.SendMessage(new UserRemoveComposer(User.VirtualId));

            RoomUser toRemove;

            if (_users != null)
                _users.TryRemove(User.InternalRoomID, out toRemove);

            onRemove(User);
        }

        public RoomUser GetUserForSquare(int x, int y)
        {
            return _room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();
        }

        public bool AddAvatarToRoom(GameClient Session)
        {
            if (_room == null)
                return false;

            if (Session == null)
                return false;

            if (Session.GetHabbo().CurrentRoom == null)
                return false;

            #region Old Stuff
            RoomUser User = new RoomUser(Session.GetHabbo().Id, _room.RoomId, primaryPrivateUserID++, _room);

            if (User == null || User.GetClient() == null)
                return false;

            User.UserId = Session.GetHabbo().Id;

            Session.GetHabbo().TentId = 0;

            int PersonalID = secondaryPrivateUserID++;
            User.InternalRoomID = PersonalID;


            Session.GetHabbo().CurrentRoomId = _room.RoomId;
            if (!this._users.TryAdd(PersonalID, User))
                return false;
            #endregion

            DynamicRoomModel Model = _room.GetGameMap().Model;
            if (Model == null)
                return false;

            if (!_room.PetMorphsAllowed && Session.GetHabbo().PetId != 0)
                Session.GetHabbo().PetId = 0;

            if (Session.GetRoleplay().InsideTaxi || (!Session.GetHabbo().IsTeleporting && !Session.GetHabbo().IsHopping))
            {
                if (!Model.DoorIsValid())
                {
                    Point Square = _room.GetGameMap().getRandomWalkableSquare();
                    Model.DoorX = Square.X;
                    Model.DoorY = Square.Y;
                    Model.DoorZ = _room.GetGameMap().GetHeightForSquareFromData(Square);
                }

                #region Roleplay last spawn coordination

                if (!Session.GetRoleplay().AntiArrowCheck)
                {
                    object[] Coords = Session.GetRoleplay().LastCoordinates.Split(',');
                    int LastX = Convert.ToInt32(Coords[0]);
                    int LastY = Convert.ToInt32(Coords[1]);
                    double LastZ = Convert.ToDouble(Coords[2]);
                    int LastRot = Convert.ToInt32(Coords[3]);

                    if (_room.GetGameMap().isInMap(LastX, LastY))
                    {
                        if (LastX == 0 && LastY == 0)
                        {
                            User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                            User.SetRot(Model.DoorOrientation, false);
                        }
                        else
                        {
                            User.SetPos(LastX, LastY, LastZ);
                            User.SetRot(LastRot, false);
                            UpdateUserStatus(User, false);
                        }
                    }
                    else
                    {
                        User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                        User.SetRot(Model.DoorOrientation, false);
                    }
                }
                else
                {
                    User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ);
                    User.SetRot(Model.DoorOrientation, false);
                }
                
                #endregion

                if (Session.GetHabbo().IsTeleporting)
                {
                    Session.GetHabbo().IsTeleporting = false;
                    Session.GetHabbo().TeleportingRoomID = 0;
                    Session.GetHabbo().TeleporterId = 0;
                }
            }
            else if (!User.IsBot && (User.GetClient().GetHabbo().IsTeleporting || User.GetClient().GetHabbo().IsHopping))
            {
                Item Item = null;
                if (Session.GetHabbo().IsTeleporting)
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().TeleporterId);
                else if (Session.GetHabbo().IsHopping)
                    Item = _room.GetRoomItemHandler().GetItem(Session.GetHabbo().HopperId);

                if (Item != null)
                {
                    if (Session.GetHabbo().IsTeleporting)
                    {
                        if (Item.GetBaseItem().InteractionType == InteractionType.ARROW)
                        {
                            Point Point = new Point(Item.Coordinate.X, Item.Coordinate.Y);
                            User.SetPos(Point.X, Point.Y, Item.GetRoom().GetGameMap().GetHeightForSquare(Point));
                            User.SetRot(Item.Rotation - 4, false);
                        }
                        else
                        {
                            Item.ExtraData = "2";
                            Item.UpdateState(false, true);
                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                            User.SetRot(Item.Rotation, false);
                            Item.InteractingUser2 = Session.GetHabbo().Id;
                            Item.ExtraData = "0";
                            Item.UpdateState(false, true);
                        }
                    }
                    else if (Session.GetHabbo().IsHopping)
                    {
                        Item.ExtraData = "1";
                        Item.UpdateState(false, true);
                        User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                        User.SetRot(Item.Rotation, false);
                        User.AllowOverride = false;
                        Item.InteractingUser2 = Session.GetHabbo().Id;
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                    }
                }
                else
                {
                    User.SetPos(Model.DoorX, Model.DoorY, Model.DoorZ - 1);
                    User.SetRot(Model.DoorOrientation, false);
                }
            }

            #region Update last spawn user coordination

            if (User != null)
            {
                if (User.GetClient() != null)
                {
                    if (User.GetClient().GetRoleplay() != null)
                    {
                        User.GetClient().GetRoleplay().LastCoordinates = User.X + "," + User.Y + "," + User.Z + "," + User.RotBody;
                    }
                }
            }

            #endregion

            #region Invisible command
            if (User.GetClient() != null)
            {
                if (User.GetClient().GetRoleplay() != null)
                {
                    if (User.GetClient().GetRoleplay().Invisible && !_room.TutorialEnabled)
                    {

                        RoleplayManager.SendDelayedWhisper(User.GetClient(), "Lembrete: Você está invisível!", 1);

                        foreach (RoomUser roomUser in GetUserList().ToList())
                        {
                            if (roomUser == null)
                                continue;
                            if (roomUser.GetClient() == null)
                                continue;
                            if (roomUser.GetClient().GetHabbo() == null)
                                continue;

                            string cansee = "";
                            if (roomUser.GetClient().GetRoleplay().Invisible && roomUser.GetClient().GetHabbo().Username != User.GetClient().GetHabbo().Username)
                            {
                                User.GetClient().SendMessage(new UsersComposer(roomUser));
                                roomUser.GetClient().SendMessage(new UsersComposer(User));
                                cansee += roomUser.GetClient().GetHabbo().Username + ", ";
                                RoleplayManager.SendDelayedWhisper(User.GetClient(), "O usuário invisível " + User.GetClient().GetHabbo().Username + " entrou na sala e pode vê-lo!", 1);
                                continue;
                            }

                            if (roomUser.GetClient().GetHabbo().Username == User.GetClient().GetHabbo().Username)
                            {
                                RoleplayManager.SendDelayedWhisper(User.GetClient(), "Os seguintes usuários invisíveis que também estão na sala podem ver você: " + cansee, 1);
                                continue;
                            }

                            if (!roomUser.GetClient().GetRoleplay().Invisible)
                            roomUser.GetClient().SendMessage(new UserRemoveComposer(User.VirtualId));
                        }
                    }
                    else
                    {
                        if (_room.TutorialEnabled)
                            Session.SendMessage(new UsersComposer(Session.GetRoomUser()));
                        else
                            _room.SendMessage(new UsersComposer(User));
                    }
                }
            }
            #endregion

            if (_room.CheckRights(Session, true))
            {
                User.SetStatus("flatctrl", "useradmin");
                Session.SendMessage(new YouAreOwnerComposer());
                Session.SendMessage(new YouAreControllerComposer(4));
            }
            else if (_room.CheckRights(Session, false) && _room.Group == null)
            {
                User.SetStatus("flatctrl", "1");
                Session.SendMessage(new YouAreControllerComposer(1));
            }
            else if (_room.Group != null && _room.CheckRights(Session, false, true))
            {
                User.SetStatus("flatctrl", "3");
                Session.SendMessage(new YouAreControllerComposer(3));
            }
            else
                Session.SendMessage(new YouAreNotControllerComposer());

            User.UpdateNeeded = true;

            foreach (RoomUser Bot in this._bots.Values.ToList())
            {
                if (Bot == null || Bot.BotAI == null)
                    continue;

                Bot.BotAI.OnUserEnterRoom(User);
            }

            EventManager.TriggerEvent("OnAddedToRoom", Session, _room);

            return true;
        }

        public void RemoveUserFromRoom(GameClient Session, Boolean NotifyClient, Boolean NotifyKick = false)
        {
            try
            {
                if (_room == null)
                    return;

                if (Session == null || Session.GetHabbo() == null)
                    return;

                if (NotifyKick)
                    Session.SendMessage(new GenericErrorComposer(4008));

                if (NotifyClient)
                    Session.SendMessage(new CloseConnectionComposer());

                if (Session.GetHabbo().TentId > 0)
                    Session.GetHabbo().TentId = 0;

                RoomUser User = GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User != null)
                {
                    if (User.RidingHorse)
                    {
                        User.RidingHorse = false;
                        RoomUser UserRiding = GetRoomUserByVirtualId(User.HorseID);
                        if (UserRiding != null)
                        {
                            UserRiding.RidingHorse = false;
                            UserRiding.HorseID = 0;
                        }
                    }

                    if (User.Team != TEAM.NONE)
                    {
                        TeamManager Team = this._room.GetTeamManagerForFreeze();
                        if (Team != null)
                        {
                            Team.OnUserLeave(User);

                            User.Team = TEAM.NONE;

                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                                User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                        }
                    }


                    RemoveRoomUser(User);

                    if (User.CurrentItemEffect != ItemEffectType.NONE)
                    {
                        if (Session.GetHabbo().Effects() != null)
                            Session.GetHabbo().Effects().CurrentEffect = -1;
                    }

                    if (_room != null)
                    {
                        if (_room.HasActiveTrade(Session.GetHabbo().Id))
                            _room.TryStopTrade(Session.GetHabbo().Id);
                    }

                    //Session.GetHabbo().CurrentRoomId = 0;

                    if (Session.GetHabbo().GetMessenger() != null)
                        Session.GetHabbo().GetMessenger().OnStatusChanged(true);

                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `user_roomvisits` SET `exit_timestamp` = '" + PlusEnvironment.GetUnixTimestamp() + "' WHERE `room_id` = '" + _room.RoomId + "' AND `user_id` = '" + Session.GetHabbo().Id + "' ORDER BY `exit_timestamp` DESC LIMIT 1");
                        dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + _room.UsersNow + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
                    }

                    if (User != null)
                        User.Dispose();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        private void onRemove(RoomUser user)
        {
            try
            {
                GameClient session = user.GetClient();
                if (session == null)
                    return;

                List<RoomUser> Bots = new List<RoomUser>();

                try
                {
                    foreach (RoomUser roomUser in GetUserList().ToList())
                    {
                        if (roomUser == null)
                            continue;

                        if (roomUser.IsBot && !roomUser.IsPet)
                        {
                            if (!Bots.Contains(roomUser))
                                Bots.Add(roomUser);
                        }
                    }
                }
                catch { }

                List<RoomUser> PetsToRemove = new List<RoomUser>();
                foreach (RoomUser Bot in Bots.ToList())
                {
                    if (Bot == null || Bot.BotAI == null)
                        continue;

                    Bot.BotAI.OnUserLeaveRoom(session);

                    if (Bot.GetBotRoleplay() != null)
                        if (Bot.GetBotRoleplayAI() != null)
                            Bot.GetBotRoleplayAI().OnUserLeaveRoom(session);

                    if (Bot.IsPet && Bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true))
                    {
                        if (!PetsToRemove.Contains(Bot))
                            PetsToRemove.Add(Bot);
                    }
                }

                foreach (RoomUser toRemove in PetsToRemove.ToList())
                {
                    if (toRemove == null)
                        continue;

                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetInventoryComponent() == null)
                        continue;

                    user.GetClient().GetHabbo().GetInventoryComponent().TryAddPet(toRemove.PetData);
                    RemoveBot(toRemove.VirtualId, false);
                }

                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }
        }

        public void RemoveRoomUser(RoomUser user)
        {
            if (user.SetStep)
                _room.GetGameMap().GameMap[user.SetX, user.SetY] = user.SqState;
            else
                _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;

            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            _room.SendMessage(new UserRemoveComposer(user.VirtualId));

            RoomUser toRemove = null;
            if (this._users.TryRemove(user.InternalRoomID, out toRemove))
            {
                //uhmm, could put the below stuff in but idk.
            }

            user.InternalRoomID = -1;
            onRemove(user);
        }

        public bool TryGetPet(int PetId, out RoomUser Pet)
        {
            return this._pets.TryGetValue(PetId, out Pet);
        }

        public bool TryGetBot(int BotId, out RoomUser Bot)
        {
            return this._bots.TryGetValue(BotId, out Bot);
        }

        public RoomUser GetBotByName(string Name)
        {
            return RoleplayBotManager.GetDeployedBotByName(Name);
        }

        public void UpdateUserCount(int count)
        {
            userCount = count;
            _room.RoomData.UsersNow = count;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + count + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
            }
        }

        public RoomUser GetRoomUserByVirtualId(int VirtualId)
        {
            RoomUser User = null;

            if (_users != null)
            if (!_users.TryGetValue(VirtualId, out User))
                return null;
            return User;
        }

        public RoomUser GetRoomUserByHabbo(int Id)
        {
            if (this == null)
                return null;

            if (this.GetUserList() == null)
                return null;

            RoomUser User = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Id == Id).FirstOrDefault();

            if (User != null)
                return User;

            return null;
        }

        public List<RoomUser> GetRoomUsers()
        {
            if (this.GetUserList() == null)
                return null;
            
            List<RoomUser> Users =  this.GetUserList().Where(x => x != null && !x.IsBot).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public List<RoomUser> GetRoleplayBots()
        {
            List<RoomUser> Users = this.GetUserList().Where(x => x != null && x.IsBot && x.IsRoleplayBot && x.GetBotRoleplay() != null).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public List<RoomUser> GetRoomUserByRank(int minRank)
        {
            List<RoomUser> Users = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Rank >= minRank).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public List<RoomUser> GetRoomUserBySpecialRights()
        {
            List<RoomUser> Users = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().VIPRank >= 2).ToList();

            if (Users != null)
                return Users;

            return null;
        }

        public RoomUser GetRoomUserByHabbo(string pName)
        {
            RoomUser User = this.GetUserList().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetRoleplay() != null && x.GetClient().GetHabbo().Username.Equals(pName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
 
            if (User != null)
            {
                if (User.GetClient().GetRoleplay().Invisible)
                    return null;
                else
                    return User;
            }
            else
                return null;
        }

        public RoomUser GetRoleplayBotByName(string pName)
        {
            RoomUser User = this.GetUserList().Where(x => x != null && x.IsBot && x.IsRoleplayBot && x.GetBotRoleplay() != null && x.GetBotRoleplay().Name.ToLower() == pName.ToLower()).FirstOrDefault();

            if (User != null)
                return User;

            return null;
        }

        public void UpdatePets()
        {
            foreach (Pet Pet in GetPets().ToList())
            {
                if (Pet == null)
                    continue;

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (Pet.DBState == DatabaseUpdateState.NeedsInsert)
                    {
                        dbClient.SetQuery("INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ('" + Pet.PetId + "','" + Pet.OwnerId + "','" + Pet.RoomId + "',@name,'0','0','0')");
                        dbClient.AddParameter("name", Pet.Name);
                        dbClient.RunQuery();

                        dbClient.SetQuery("INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ('" + Pet.Type + "',@race,@color,'0','100','" + Pet.CreationStamp + "','0','0')");
                        dbClient.AddParameter(Pet.PetId + "race", Pet.Race);
                        dbClient.AddParameter(Pet.PetId + "color", Pet.Color);
                        dbClient.RunQuery();
                    }
                    else if (Pet.DBState == DatabaseUpdateState.NeedsUpdate)
                    {
                        //Surely this can be *99 better?
                        RoomUser User = GetRoomUserByVirtualId(Pet.VirtualId);

                        dbClient.RunQuery("UPDATE `bots` SET room_id = " + Pet.RoomId + ", x = " + (User != null ? User.X : 0) + ", Y = " + (User != null ? User.Y : 0) + ", Z = " + (User != null ? User.Z : 0) + " WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                        dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + Pet.experience + "', `energy` = '" + Pet.Energy + "', `nutrition` = '" + Pet.Nutrition + "', `respect` = '" + Pet.Respect + "' WHERE `id` = '" + Pet.PetId + "' LIMIT 1");
                    }

                    Pet.DBState = DatabaseUpdateState.Updated;
                }
            }
        }

        public List<Pet> GetPets()
        {
            List<Pet> Pets = new List<Pet>();
            foreach (RoomUser User in this._pets.Values.ToList())
            {
                if (User == null || !User.IsPet)
                    continue;

                Pets.Add(User.PetData);
            }

            return Pets;
        }

        public void SerializeStatusUpdates()
        {
            List<RoomUser> Users = new List<RoomUser>();
            ICollection<RoomUser> RoomUsers = GetUserList();

            if (RoomUsers == null)
                return;

            foreach (RoomUser User in RoomUsers.ToList())
            {
                if (User == null || !User.UpdateNeeded || Users.Contains(User))
                    continue;

                User.UpdateNeeded = false;
                Users.Add(User);
            }

            if (Users.Count > 0)
                _room.SendMessage(new UserUpdateComposer(Users));
        }

        public void UpdateUserStatusses()
        {
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                UpdateUserStatus(user, false);
            }
        }

        private bool isValid(RoomUser user)
        {
            if (user == null)
                return false;
            if (user.IsBot)
                return true;
            if (user.GetClient() == null)
                return false;
            if (user.GetClient().GetHabbo() == null)
                return false;
            if (user.GetClient().GetHabbo().CurrentRoomId != _room.RoomId)
                return false;
            return true;
        }

        public void OnCycle()
        {
            int userCounter = 0;

            try
            {
                foreach (RoomUser User in GetUserList().ToList())
                {
                    if (User == null)
                        continue;

                    if (!isValid(User))
                    {
                        if (User.GetClient() != null)
                            RemoveUserFromRoom(User.GetClient(), false, false);
                        else
                            RemoveRoomUser(User);
                    }

                    bool updated = false;
                    User.IdleTime++;
                    User.HandleSpamTicks();
                    if (!User.IsBot && !User.IsAsleep && User.IdleTime >= 600)
                    {
                        User.IsAsleep = true;
                        _room.SendMessage(new SleepComposer(User, true));

                        if (!User.GetClient().GetRoleplay().IsJailed && !User.GetClient().GetRoleplay().IsDead && User.GetClient().GetRoleplay().Game == null)
                        {
                            User.GetClient().GetHabbo().Motto = "[AUSENTE] " + User.GetClient().GetRoleplay().Class;
                            User.GetClient().GetHabbo().Poof(false);
                        }
                    }

                    if (User.CarryItemID > 0)
                    {
                        User.CarryTimer--;
                        if (User.CarryTimer <= 0)
                            User.CarryItem(0);
                    }

                    if (_room.GotFreeze())
                        _room.GetFreeze().CycleUser(User);

                    bool InvalidStep = false;

                    if (User.isRolling)
                    {
                        if (User.rollerDelay <= 0)
                        {
                            UpdateUserStatus(User, false);
                            User.isRolling = false;
                        }
                        else
                            User.rollerDelay--;
                    }

                    if (User.SetStep)
                    {
                        if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(User.SetX, User.SetY), (User.GoalX == User.SetX && User.GoalY == User.SetY), User.AllowOverride))
                        {
                            if (!User.RidingHorse)
                                _room.GetGameMap().UpdateUserMovement(new Point(User.Coordinate.X, User.Coordinate.Y), new Point(User.SetX, User.SetY), User);

                            List<Item> items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (Item Item in items.ToList())
                            {
                                Item.UserWalksOffFurni(User);
                            }
                            if (!User.IsBot)
                            {
                                if (!User.RidingHorse)
                                {
                                    User.X = User.SetX;
                                    User.Y = User.SetY;
                                    User.Z = User.SetZ;
                                }
                                else
                                {
                                    RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        User.X = User.SetX;
                                        User.Y = User.SetY;
                                        User.Z = User.SetZ;
                                        Horse.X = User.SetX;
                                        Horse.Y = User.SetY;
                                    }
                                }
                            }
                            else
                            {
                                if (!User.RidingHorse)
                                {
                                    User.X = User.SetX;
                                    User.Y = User.SetY;
                                    User.Z = User.SetZ;
                                }
                            }

                            List<Item> Items = _room.GetGameMap().GetCoordinatedItems(new Point(User.X, User.Y));
                            foreach (Item Item in Items.ToList())
                            {
                                Item.UserWalksOnFurni(User);
                            }

                            if (User != null)
                            {
                                if (User.GetClient() != null)
                                {
                                    if (User.GetClient().GetRoleplay() != null)
                                    {
                                        User.GetClient().GetRoleplay().LastCoordinates = User.X + "," + User.Y + "," + User.Z + "," + User.RotBody;
                                    }
                                }
                            }

                            UpdateUserStatus(User, true);
                        }
                        else
                            InvalidStep = true;
                        User.SetStep = false;
                    }

                    if (User.PathRecalcNeeded)
                    {
                        if (User.Path.Count > 1)
                            User.Path.Clear();

                        User.Path = PathFinder.FindPath(User, this._room.GetGameMap().DiagonalEnabled, this._room.GetGameMap(), new Vector2D(User.X, User.Y), new Vector2D(User.GoalX, User.GoalY));

                        if (User.Path.Count > 1)
                        {
                            User.PathStep = 1;
                            User.IsWalking = true;
                            User.PathRecalcNeeded = false;
                        }
                        else
                        {
                            User.PathRecalcNeeded = false;
                            if (User.Path.Count > 1)
                                User.Path.Clear();
                        }
                    }

                    if (User.IsWalking && !User.Freezed)
                    {
                        if (InvalidStep || (User.PathStep >= User.Path.Count) || (User.GoalX == User.X && User.GoalY == User.Y)) //No path found, or reached goal (:
                        {
                            User.IsWalking = false;
                            User.RemoveStatus("mv");

                            if (User.Statusses.ContainsKey("sign"))
                                User.RemoveStatus("sign");

                            if (User.IsBot && User.BotData.TargetUser > 0)
                            {
                                if (User.CarryItemID > 0)
                                {
                                    RoomUser Target = _room.GetRoomUserManager().GetRoomUserByHabbo(User.BotData.TargetUser);

                                    if (Target != null && Gamemap.TilesTouching(User.X, User.Y, Target.X, Target.Y))
                                    {
                                        User.SetRot(Rotation.Calculate(User.X, User.Y, Target.X, Target.Y), false);
                                        Target.SetRot(Rotation.Calculate(Target.X, Target.Y, User.X, User.Y), false);
                                        Target.CarryItem(User.CarryItemID);
                                    }
                                }

                                User.CarryItem(0);
                                User.BotData.TargetUser = 0;
                            }

                            if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                            {
                                RoomUser mascotaVinculada = GetRoomUserByVirtualId(User.HorseID);
                                if (mascotaVinculada != null)
                                {
                                    mascotaVinculada.IsWalking = false;
                                    mascotaVinculada.RemoveStatus("mv");
                                    mascotaVinculada.UpdateNeeded = true;
                                }
                            }
                        }
                        else
                        {
                            Vector2D NextStep = User.Path[(User.Path.Count - User.PathStep) - 1];
                            User.PathStep++;

                            bool CarFastWalk = false;
                            bool CocaineFastWalk = false;

                            if (User.GetClient() != null && User.GetClient().GetRoleplay() != null)
                            {
                                if (User.GetClient().GetRoleplay().DrivingCar)
                                    CarFastWalk = true;

                                if (User.GetClient().GetRoleplay().HighOffCocaine)
                                    CocaineFastWalk = true;
                            }

                            if ((User.FastWalking || CarFastWalk) && User.PathStep < User.Path.Count)
                            {
                                int s2 = (User.Path.Count - User.PathStep) - 1;
                                NextStep = User.Path[s2];
                                User.PathStep++;
                            }

                            if ((User.SuperFastWalking || CocaineFastWalk) && User.PathStep < User.Path.Count)
                            {
                                int s2 = (User.Path.Count - User.PathStep) - 1;
                                NextStep = User.Path[s2];
                                User.PathStep++;
                                User.PathStep++;
                            }


                            int nextX = NextStep.X;
                            int nextY = NextStep.Y;

                            if (User.GetClient() != null && User.GetClient().GetRoleplay() != null)
                            {
                                if (User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Right || User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Left)
                                    nextX = User.X;
                                if (User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Up || User.GetClient().GetRoleplay().WalkDirection == WalkDirections.Down)
                                    nextY = User.Y;
                            }

                            User.RemoveStatus("mv");

                            if (!User.IsBot && User.GetClient() != null && User.GetClient().GetRoleplay() != null)
                            {
                                if (User.GetClient().GetRoleplay().WalkDirection != WalkDirections.None)
                                {
                                    Point Point = RoleplayManager.GetDirectionDeviation(User);

                                    if (Point != new Point(0, 0))
                                        User.MoveTo(Point);
                                }
                            }

                            if (_room.GetGameMap().IsValidStep2(User, new Vector2D(User.X, User.Y), new Vector2D(nextX, nextY), (User.GoalX == nextX && User.GoalY == nextY), User.AllowOverride))
                            {
                                double nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);

                                if (!User.IsBot)
                                {
                                    if (User.isSitting || User.isLying)
                                    {
                                        User.Z += 0.35;
                                        User.isSitting = false;
                                        User.UpdateNeeded = true;
                                    }

                                    User.Statusses.Remove("lay");
                                    User.Statusses.Remove("sit");
                                }

                                if (!User.IsBot && !User.IsPet && User.GetClient() != null)
                                {
                                    if (User.GetClient().GetHabbo().IsTeleporting)
                                    {
                                        User.GetClient().GetHabbo().IsTeleporting = false;
                                        User.GetClient().GetHabbo().TeleporterId = 0;
                                    }
                                    else if (User.GetClient().GetHabbo().IsHopping)
                                    {
                                        User.GetClient().GetHabbo().IsHopping = false;
                                        User.GetClient().GetHabbo().HopperId = 0;
                                    }
                                }

                                if (!User.IsBot && User.RidingHorse && User.IsPet == false)
                                {
                                    RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                        Horse.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                    User.AddStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));

                                    User.UpdateNeeded = true;
                                    Horse.UpdateNeeded = true;
                                }
                                else
                                    User.AddStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                int newRot = Rotation.Calculate(User.X, User.Y, nextX, nextY, User.moonwalkEnabled);

                                User.RotBody = newRot;
                                User.RotHead = newRot;

                                User.SetStep = true;
                                User.SetX = nextX;
                                User.SetY = nextY;
                                User.SetZ = nextZ;
                                UpdateUserEffect(User, User.SetX, User.SetY);

                                updated = true;

                                if (User.RidingHorse && User.IsPet == false && !User.IsBot)
                                {
                                    RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                    if (Horse != null)
                                    {
                                        Horse.RotBody = newRot;
                                        Horse.RotHead = newRot;

                                        Horse.SetStep = true;
                                        Horse.SetX = nextX;
                                        Horse.SetY = nextY;
                                        Horse.SetZ = nextZ;
                                    }
                                }

                                if (_room.Id == Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")))
                                {
                                    if (!JailbreakManager.FenceBroken)
                                    {
                                        int X = Convert.ToInt32(RoleplayData.GetData("jailbreak", "fencex"));
                                        int Y = Convert.ToInt32(RoleplayData.GetData("jailbreak", "fencey"));

                                        if (User.X == X && User.Y == Y)
                                        {
                                            _room.GetGameMap().GameMap[X, Y] = 0;
                                            User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY];
                                        }
                                        else if (User.X == (X + 1) || User.Y == Y)
                                        {
                                            _room.GetGameMap().GameMap[(X + 1), Y] = 0;
                                            User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY];
                                        }
                                        else
                                        {
                                            _room.GetGameMap().GameMap[User.X, User.Y] = User.SqState; 
                                            User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY];
                                        }
                                    }
                                }
                                else
                                {
                                    _room.GetGameMap().GameMap[User.X, User.Y] = User.SqState;
                                    User.SqState = _room.GetGameMap().GameMap[User.SetX, User.SetY];
                                }

                                if (_room.RoomBlockingEnabled == 0)
                                {
                                    List<RoomUser> Users = _room.GetRoomUserManager().GetRoomUsers().Where(x => x.X == nextX && x.Y == nextY).ToList();
                                    if (Users != null && Users.Count > 0)
                                        _room.GetGameMap().GameMap[nextX, nextY] = 0;
                                    else
                                        _room.GetGameMap().GameMap[nextX, nextY] = 1;
                                }
                                else
                                    _room.GetGameMap().GameMap[nextX, nextY] = 1;
                            }
                        }
                        if (!User.RidingHorse)
                            User.UpdateNeeded = true;
                    }
                    else
                    {
                        if (User.Statusses.ContainsKey("mv"))
                        {
                            User.RemoveStatus("mv");
                            User.UpdateNeeded = true;

                            if (User.RidingHorse)
                            {
                                RoomUser Horse = GetRoomUserByVirtualId(User.HorseID);
                                if (Horse != null)
                                {
                                    Horse.RemoveStatus("mv");
                                    Horse.UpdateNeeded = true;
                                }
                            }
                        }
                    }

                    if (User.RidingHorse)
                        User.ApplyEffect(77);

                    if (!User.IsBot)
                        userCounter++;

                    if (!updated)
                        UpdateUserEffect(User, User.X, User.Y);
                }

                if (userCount != userCounter)
                    UpdateUserCount(userCounter);
            }
            catch (Exception e)
            {
                int rId = 0;
                if (_room != null)
                    rId = _room.Id;

                Logging.LogCriticalException("Quarto afetado - ID: " + rId + " - " + e.ToString());
            }
        }

        public void UpdateUserStatus(RoomUser User, bool cyclegameitems)
        {
            if (User == null)
                return;

            try
            {
                bool isBot = User.IsBot;
                if (isBot)
                    cyclegameitems = false;

                if (PlusEnvironment.GetUnixTimestamp() > PlusEnvironment.GetUnixTimestamp() + User.SignTime)
                {
                    if (User.Statusses.ContainsKey("sign"))
                    {
                        User.Statusses.Remove("sign");
                        User.UpdateNeeded = true;
                    }
                }

                if ((User.Statusses.ContainsKey("lay") && !User.isLying) || (User.Statusses.ContainsKey("sit") && !User.isSitting))
                {
                    if (User.Statusses.ContainsKey("lay"))
                        User.Statusses.Remove("lay");
                    if (User.Statusses.ContainsKey("sit"))
                        User.Statusses.Remove("sit");
                    User.UpdateNeeded = true;
                }
                else if (User.isLying || User.isSitting)
                    return;

                double newZ;
                List<Item> ItemsOnSquare = _room.GetGameMap().GetAllRoomItemForSquare(User.X, User.Y);
                if (ItemsOnSquare != null || ItemsOnSquare.Count != 0)
                {
                    if (User.RidingHorse && User.IsPet == false)
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList()) + 1;
                    else
                        newZ = _room.GetGameMap().SqAbsoluteHeight(User.X, User.Y, ItemsOnSquare.ToList());
                }
                else
                    newZ = 1;

                if (newZ != User.Z)
                {
                    User.Z = newZ;
                    User.UpdateNeeded = true;
                }

                DynamicRoomModel Model = _room.GetGameMap().Model;
                if (Model.SqState[User.X, User.Y] == SquareState.SEAT)
                {
                    if (!User.Statusses.ContainsKey("sit"))
                        User.Statusses.Add("sit", "1.0");
                    User.Z = Model.SqFloorHeight[User.X, User.Y];
                    User.RotHead = Model.SqSeatRot[User.X, User.Y];
                    User.RotBody = Model.SqSeatRot[User.X, User.Y];

                    User.UpdateNeeded = true;
                }

                if (ItemsOnSquare.Count == 0)
                    User.LastItem = null;

                foreach (Item Item in ItemsOnSquare.ToList())
                {
                    if (Item == null)
                        continue;

                    if (Item.GetBaseItem().IsSeat)
                    {
                        if (!User.Statusses.ContainsKey("sit"))
                        {
                            if (!User.Statusses.ContainsKey("sit"))
                                User.Statusses.Add("sit", TextHandling.GetString(Item.GetBaseItem().Height));
                        }

                        User.Z = Item.GetZ;
                        User.RotHead = Item.Rotation;
                        User.RotBody = Item.Rotation;
                        User.UpdateNeeded = true;
                    }

                    switch (Item.GetBaseItem().InteractionType)
                    {
                        #region Roleplay

                        #region Shower
                        case InteractionType.SHOWER:
                            {
                                if (User.Coordinate.X == Item.GetX && User.Coordinate.Y == Item.GetY)
                                {
                                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetRoleplay() == null)
                                        continue;

                                    Room Room;

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                        return;

                                    if (User.GetClient().GetRoleplay().Hygiene >= 100)
                                    {
                                        User.GetClient().SendWhisper("Sua Higiene já está no máximo de 100!", 1);
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.InteractingUser != 0)
                                    {
                                        User.GetClient().SendWhisper("Este chuveiro já está sendo usado por outra pessoa! Vá para outro!", 1);
                                        User.MoveTo(Item.SquareInFront.X, Item.SquareInFront.Y);
                                        return;
                                    }

                                    if (Item.ExtraData == "0" || Item.ExtraData == "")
                                    {
                                        Item.ExtraData = "1";
                                        Item.UpdateState(false, true);
                                        Item.RequestUpdate(1, true);
                                    }
                                    if (Item.ExtraData == "1")
                                    {
                                        if (!User.GetClient().GetRoleplay().InShower)
                                        {
                                            User.ClearMovement(true);
                                            Item.InteractingUser = User.GetClient().GetHabbo().Id;
                                            User.GetClient().GetRoleplay().InShower = true;
                                            RoleplayManager.Shout(User.GetClient(), "*Começa a tomar banho*", 4);
                                            User.GetClient().GetRoleplay().TimerManager.CreateTimer("shower", 1000, false, Item.Id);
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Whisper Tile
                        case InteractionType.WHISPER_TILE:
                            {
                                if (!User.IsBot)
                                {
                                    if (User.Coordinate.X == Item.GetX && User.Coordinate.Y == Item.GetY)
                                    {
                                        if (Item.WhisperTileData == null)
                                        {
                                            User.GetClient().SendWhisper("Opa, os dados de sussurro deste item está quebrado!", 1);
                                            break;
                                        }
                                        else
                                        {
                                            if (Item.WhisperTileData.Message == null)
                                                Item.WhisperTileData.Message = "";

                                            if (Item.WhisperTileData.Message == "")
                                            {
                                                //User.GetClient().SendWhisper("Opa, looks like this items whisper data has not been set yet!", 1);
                                                break;
                                            }

                                            if (User.GetClient() != null && Item.WhisperTileData != null && Item.WhisperTileData.Message != null)
                                                User.GetClient().SendWhisper(Item.WhisperTileData.Message, 34);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        #endregion

                        #endregion

                        #region Beds & Tents
                        case InteractionType.BED:
                        case InteractionType.BEDEFFECT:
                        case InteractionType.TENT_SMALL:
                            {
                                if (!User.Statusses.ContainsKey("lay"))
                                    User.Statusses.Add("lay", TextHandling.GetString(Item.GetBaseItem().Height) + " null");

                                if (Item.GetBaseItem().InteractionType == InteractionType.BEDEFFECT)
                                {
                                    if (User != null && !User.IsBot)
                                    {
                                        if (Item == null || Item.GetBaseItem() == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Effects() == null)
                                            return;

                                        if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                            return;

                                        User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                                        Item.ExtraData = "1";
                                        Item.UpdateState(false, true);
                                        Item.RequestUpdate(2, true);
                                    }
                                }

                                User.RotHead = Item.Rotation;
                                User.RotBody = Item.Rotation;
                                User.UpdateNeeded = true;
                                break;
                            }
                        #endregion

                        #region Banzai Gates
                        case InteractionType.banzaigategreen:
                        case InteractionType.banzaigateblue:
                        case InteractionType.banzaigatered:
                        case InteractionType.banzaigateyellow:
                            {
                                #region Colour Wars
                                if (Item.RoomId == Convert.ToInt32(RoleplayData.GetData("colourwars", "lobbyid")))
                                {
                                    if (User.IsBot)
                                        break;

                                    if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.ColourWars))
                                    {
                                        User.GetClient().SendWhisper("Não há um evento Guerra de Cores no momento!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    if (User.GetClient().GetRoleplay().Game != null)
                                    {
                                        User.GetClient().SendWhisper("Você já está dentro de um evento!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    if (User.GetClient().GetRoleplay().Team != null)
                                    {
                                        User.GetClient().SendWhisper("Você já está dentro de uma equipe!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    string Team = "";
                                    if (Item.team == TEAM.BLUE)
                                        Team = "Azul";
                                    else if (Item.team == TEAM.GREEN)
                                        Team = "Verde";
                                    else if (Item.team == TEAM.RED)
                                        Team = "Rosa";
                                    else if (Item.team == TEAM.YELLOW)
                                        Team = "Amarelo";

                                    if (Team != "")
                                    {
                                        string Result = RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.ColourWars), User.GetClient(), Team);
                                        if (Result == "TEAMFULL")
                                        {
                                            User.GetClient().SendWhisper("Este time está cheio!", 1);
                                            User.MoveTo(Item.SquareInFront);
                                            break;
                                        }
                                        else if (Result != "OK")
                                            break;

                                        if (User.GetClient().GetRoleplay().IsWorking)
                                            User.GetClient().GetRoleplay().IsWorking = false;

                                        if (Result == "OK")
                                        {
                                            User.ClearMovement(true);

                                            foreach (var item in Item.GetRoom().GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == Item.GetBaseItem().InteractionType).ToList())
                                            {
                                                item.ExtraData = User.GetClient().GetRoleplay().Team.Members.Count.ToString();
                                                item.UpdateState();

                                                if (item.GetRoom().GetGameMap().GameMap[Item.GetX, Item.GetY] == 0)
                                                {
                                                    foreach (RoomUser sser in Item.GetRoom().GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                                    {
                                                        sser.SqState = 1;
                                                    }
                                                    item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                                                }
                                            }
                                        }

                                        if (User.GetClient().GetRoleplay().EquippedWeapon != null)
                                            User.GetClient().GetRoleplay().EquippedWeapon = null;

                                        if (User.GetClient().GetRoleplay().InsideTaxi)
                                            User.GetClient().GetRoleplay().InsideTaxi = false;
                                    }
                                    break;
                                }
                                #endregion

                                #region Team Brawl
                                else if (Item.RoomId == Convert.ToInt32(RoleplayData.GetData("teambrawl", "lobbyid")))
                                {
                                    if (User.IsBot)
                                        break;

                                    if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.TeamBrawl))
                                    {
                                        User.GetClient().SendWhisper("Não há um evento de Brigas de Times acontecendo agora!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    if (User.GetClient().GetRoleplay().Game != null)
                                    {
                                        User.GetClient().SendWhisper("Você já está dentro de um evento!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    if (User.GetClient().GetRoleplay().Team != null)
                                    {
                                        User.GetClient().SendWhisper("Você já está dentro de uma equipe!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    string Team = "";
                                    if (Item.team == TEAM.BLUE)
                                        Team = "Azul";
                                    else if (Item.team == TEAM.GREEN)
                                        Team = "Verde";
                                    else if (Item.team == TEAM.RED)
                                        Team = "Rosa";
                                    else if (Item.team == TEAM.YELLOW)
                                        Team = "Amarelo";

                                    if (Team != "")
                                    {
                                        string Result = RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.TeamBrawl), User.GetClient(), Team);
                                        if (Result == "TEAMFULL")
                                        {
                                            User.GetClient().SendWhisper("Este time está cheio", 1);
                                            User.MoveTo(Item.SquareInFront);
                                            break;
                                        }
                                        else if (Result != "OK")
                                            break;

                                        if (User.GetClient().GetRoleplay().IsWorking)
                                            User.GetClient().GetRoleplay().IsWorking = false;

                                        if (Result == "OK")
                                        {
                                            User.ClearMovement(true);

                                            foreach (var item in Item.GetRoom().GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == Item.GetBaseItem().InteractionType).ToList())
                                            {
                                                item.ExtraData = User.GetClient().GetRoleplay().Team.Members.Count.ToString();
                                                item.UpdateState();

                                                if (item.GetRoom().GetGameMap().GameMap[Item.GetX, Item.GetY] == 0)
                                                {
                                                    foreach (RoomUser sser in Item.GetRoom().GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                                    {
                                                        sser.SqState = 1;
                                                    }
                                                    item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                                                }
                                            }
                                        }

                                        if (User.GetClient().GetRoleplay().EquippedWeapon != null)
                                            User.GetClient().GetRoleplay().EquippedWeapon = null;

                                        if (User.GetClient().GetRoleplay().InsideTaxi)
                                            User.GetClient().GetRoleplay().InsideTaxi = false;
                                    }
                                    break;
                                }
                                #endregion

                                #region Mafia Wars
                                else if (Item.RoomId == Convert.ToInt32(RoleplayData.GetData("mafiawars", "lobbyid")))
                                {
                                    if (User.IsBot)
                                        break;

                                    if (!RoleplayGameManager.RunningGames.ContainsKey(GameMode.MafiaWars))
                                    {
                                        User.GetClient().SendWhisper("Não há um evento Guerra de Máfias no momento!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    if (User.GetClient().GetRoleplay().Game != null)
                                    {
                                        User.GetClient().SendWhisper("Você já está dentro de um evento!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    if (User.GetClient().GetRoleplay().Team != null)
                                    {
                                        User.GetClient().SendWhisper("Você já está dentro de uma equipe!", 1);
                                        User.MoveTo(Item.SquareInFront);
                                        break;
                                    }

                                    string Team = "";
                                    if (Item.team == TEAM.BLUE)
                                        Team = "Azul";
                                    else if (Item.team == TEAM.GREEN)
                                        Team = "Verde";

                                    if (Team != "")
                                    {
                                        string Result = RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.MafiaWars), User.GetClient(), Team);
                                        if (Result == "TEAMFULL")
                                        {
                                            User.GetClient().SendWhisper("Este time está cheio", 1);
                                            User.MoveTo(Item.SquareInFront);
                                            break;
                                        }
                                        else if (Result != "OK")
                                            break;

                                        if (User.GetClient().GetRoleplay().IsWorking)
                                            User.GetClient().GetRoleplay().IsWorking = false;

                                        if (Result == "OK")
                                        {
                                            User.ClearMovement(true);

                                            foreach (var item in Item.GetRoom().GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == Item.GetBaseItem().InteractionType).ToList())
                                            {
                                                item.ExtraData = User.GetClient().GetRoleplay().Team.Members.Count.ToString();
                                                item.UpdateState();

                                                if (item.GetRoom().GetGameMap().GameMap[Item.GetX, Item.GetY] == 0)
                                                {
                                                    foreach (RoomUser sser in Item.GetRoom().GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                                    {
                                                        sser.SqState = 1;
                                                    }
                                                    item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                                                }
                                            }
                                        }

                                        if (User.GetClient().GetRoleplay().EquippedWeapon != null)
                                            User.GetClient().GetRoleplay().EquippedWeapon = null;

                                        if (User.GetClient().GetRoleplay().InsideTaxi)
                                            User.GetClient().GetRoleplay().InsideTaxi = false;
                                    }
                                    break;
                                }
                                #endregion

                                #region Non-Roleplay
                                else
                                {
                                    if (cyclegameitems)
                                    {
                                        int effectID = Convert.ToInt32(Item.team + 32);
                                        TeamManager t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();

                                        if (User.Team == TEAM.NONE)
                                        {
                                            if (t.CanEnterOnTeam(Item.team))
                                            {
                                                if (User.Team != TEAM.NONE)
                                                    t.OnUserLeave(User);
                                                User.Team = Item.team;

                                                t.AddUser(User);

                                                if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                                    User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                            }
                                        }
                                        else if (User.Team != TEAM.NONE && User.Team != Item.team)
                                        {
                                            t.OnUserLeave(User);
                                            User.Team = TEAM.NONE;
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        }
                                        else
                                        {
                                            //usersOnTeam--;
                                            t.OnUserLeave(User);
                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                            User.Team = TEAM.NONE;
                                        }
                                        //Item.ExtraData = usersOnTeam.ToString();
                                        //Item.UpdateState(false, true);                                
                                    }
                                    break;
                                }
                                #endregion
                            }
                        #endregion

                        #region Freeze Gates
                        case InteractionType.FREEZE_YELLOW_GATE:
                        case InteractionType.FREEZE_RED_GATE:
                        case InteractionType.FREEZE_GREEN_GATE:
                        case InteractionType.FREEZE_BLUE_GATE:
                            {
                                if (User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetRoleplay() == null)
                                    break;

                                #region Texas Hold 'Em
                                if (TexasHoldEmManager.GameList.Count > 0)
                                {
                                    if (TexasHoldEmManager.GameList.Values.Where(x => x.JoinGate != null && x.JoinGate.Furni != null && x.JoinGate.Furni == Item).ToList().Count > 0)
                                    {
                                        TexasHoldEm Game = TexasHoldEmManager.GameList.Values.Where(x => x.JoinGate != null && x.JoinGate.Furni != null && x.JoinGate.Furni == Item).ToList().FirstOrDefault();

                                        if (Game.GameStarted)
                                            User.GetClient().SendWhisper("Desculpe, mas já existe um evento de Texas Hold", 1);
                                        else
                                            Game.AddPlayerToGame(User.GetClient().GetHabbo().Id);
                                        break;
                                    }
                                }
                                #endregion

                                #region Non-Roleplay
                                if (cyclegameitems)
                                {
                                    int effectID = Convert.ToInt32(Item.team + 39);
                                    TeamManager t = User.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();

                                    if (User.Team == TEAM.NONE)
                                    {
                                        if (t.CanEnterOnTeam(Item.team))
                                        {
                                            if (User.Team != TEAM.NONE)
                                                t.OnUserLeave(User);
                                            User.Team = Item.team;
                                            t.AddUser(User);

                                            if (User.GetClient().GetHabbo().Effects().CurrentEffect != effectID)
                                                User.GetClient().GetHabbo().Effects().ApplyEffect(effectID);
                                        }
                                    }
                                    else if (User.Team != TEAM.NONE && User.Team != Item.team)
                                    {
                                        t.OnUserLeave(User);
                                        User.Team = TEAM.NONE;
                                        User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    }
                                    else
                                    {
                                        //usersOnTeam--;
                                        t.OnUserLeave(User);
                                        if (User.GetClient().GetHabbo().Effects().CurrentEffect == effectID)
                                            User.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                        User.Team = TEAM.NONE;
                                    }
                                    //Item.ExtraData = usersOnTeam.ToString();
                                    //Item.UpdateState(false, true);                                
                                }
                                #endregion

                                break;
                            }
                        #endregion

                        #region Banzai Teles
                        case InteractionType.banzaitele:
                            {
                                if (User.Statusses.ContainsKey("mv"))
                                    _room.GetGameItemHandler().onTeleportRoomUserEnter(User, Item);
                                break;
                            }
                        #endregion

                        #region Football Gate

                        #endregion

                        #region Pressure Pads
                        case InteractionType.PRESSURE_PAD:
                            {
                                if (User == null)
                                    return;

                                if (!User.IsBot)
                                {
                                    if (Item == null)
                                        return;

                                    Item.ExtraData = "1";
                                    Item.UpdateState(false, true);
                                    Item.RequestUpdate(1, true);
                                }
                                break;
                            }
                        #endregion

                        #region Effects
                        case InteractionType.EFFECT:
                            {
                                if (User == null)
                                    return;

                                if (!User.IsBot)
                                {
                                    if (Item == null || Item.GetBaseItem() == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Effects() == null)
                                        return;

                                    if (Item.GetBaseItem().EffectId == 0 && User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                        return;

                                    User.GetClient().GetHabbo().Effects().ApplyEffect(Item.GetBaseItem().EffectId);
                                    Item.ExtraData = "1";
                                    Item.UpdateState(false, true);
                                    Item.RequestUpdate(2, true);
                                }
                                break;
                            }
                        #endregion

                        #region Arrows
                        case InteractionType.ARROW:
                            {
                                if (User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                                {
                                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().IsTeleporting)
                                        continue;

                                    Room Room;

                                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(User.GetClient().GetHabbo().CurrentRoomId, out Room))
                                        break;

                                    if (!User.IsBot)
                                    {
                                        if ((User.GetClient().GetRoleplay().IsJailed && !User.GetClient().GetRoleplay().Jailbroken))
                                        {
                                            User.GetClient().SendWhisper("Você não pode usar setas para fugir enquanto você está preso!", 1);
                                            break;
                                        }

                                        if (User.GetClient().GetRoleplay().IsDead)
                                        {
                                            User.GetClient().SendWhisper("Você não pode usar setas enquanto está morto!", 1);
                                            break;
                                        }

                                        if (User.GetClient().GetRoleplay().InsideTaxi)
                                            User.GetClient().GetRoleplay().InsideTaxi = false;

                                        User.ClearMovement(true);
                                    }

                                    if (!ItemTeleporterFinder.IsTeleLinked(Item.Id, Room))
                                        User.UnlockWalking();
                                    else
                                    {
                                        int LinkedTele = ItemTeleporterFinder.GetLinkedTele(Item.Id, Room);
                                        int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);

                                        if (User.GetClient() != null)
                                        {

                                            object[] Bits = new object[6];
                                            Bits[0] = Item.GetX;
                                            Bits[1] = Item.GetY;
                                            Bits[2] = Item.RoomId;
                                            Bits[3] = TeleRoomId;
                                            Bits[4] = Item.Id;
                                            Bits[5] = LinkedTele;

                                            EventManager.TriggerEvent("OnTeleport", User.GetClient(), Bits);

                                        }
                                        if (TeleRoomId == Room.RoomId)
                                        {
                                            Item TargetItem = Room.GetRoomItemHandler().GetItem(LinkedTele);
                                            if (TargetItem == null)
                                            {
                                                if (User.GetClient() != null)
                                                    User.GetClient().SendWhisper("Ei, essa seta está ruim!", 1);
                                                break;
                                            }
                                            else
                                            {
                                                Room.GetGameMap().TeleportToItem(User, TargetItem);
                                            }
                                        }
                                        else if (TeleRoomId != Room.RoomId)
                                        {
                                            if (User != null && !User.IsBot && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                            {
                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                User.GetClient().GetHabbo().TeleportingRoomID = TeleRoomId;
                                                User.GetClient().GetHabbo().TeleporterId = LinkedTele;
                                                RoleplayManager.SendUser(User.GetClient(), TeleRoomId);
                                            }
                                        }
                                        else if (this._room.GetRoomItemHandler().GetItem(LinkedTele) != null)
                                        {
                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                            User.SetRot(Item.Rotation, false);
                                        }
                                        else
                                            User.UnlockWalking();
                                    }
                                }
                                break;
                            }
                            #endregion
                    }
                }

                if (User.isSitting && User.TeleportEnabled)
                {
                    User.Z -= 0.35;
                    User.UpdateNeeded = true;
                }

                if (cyclegameitems)
                {
                    if (_room.GotSoccer())
                        _room.GetSoccer().OnUserWalk(User);

                    if (_room.GotBanzai())
                        _room.GetBanzai().OnUserWalk(User);

                    if (_room.GotFreeze())
                        _room.GetFreeze().OnUserWalk(User);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        private void UpdateUserEffect(RoomUser User, int x, int y)
        {
            if (User == null || User.IsBot || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                return;

            try
            {
                byte NewCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
                if (NewCurrentUserItemEffect > 0)
                {
                    if (User.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                        User.CurrentItemEffect = ItemEffectType.NONE;

                    ItemEffectType Type = ByteToItemEffectEnum.Parse(NewCurrentUserItemEffect);
                    if (Type != User.CurrentItemEffect)
                    {
                        switch (Type)
                        {
                            case ItemEffectType.Iceskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                    User.CurrentItemEffect = ItemEffectType.Iceskates;
                                    break;
                                }

                            case ItemEffectType.Normalskates:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(User.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SWIM:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(29);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimLow:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(30);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                            case ItemEffectType.SwimHalloween:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(37);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }

                            case ItemEffectType.NONE:
                                {
                                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                                    User.CurrentItemEffect = Type;
                                    break;
                                }
                        }
                    }
                }
                else if (User.CurrentItemEffect != ItemEffectType.NONE && NewCurrentUserItemEffect == 0)
                {
                    User.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                    User.CurrentItemEffect = ItemEffectType.NONE;
                }
            }
            catch
            {
            }
        }

        public int PetCount
        {
            get { return petCount; }
        }

        public ICollection<RoomUser> GetBotList()
        {
            return this._bots.Values;
        }

        public ICollection<RoomUser> GetUserList()
        {
            try
            {
                return this._users.Values;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}