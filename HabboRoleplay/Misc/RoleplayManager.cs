using System;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Weapons;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Roleplay.Web.Incoming.Interactions;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Timers;
using Plus.HabboRoleplay.Bots;
using System.Threading;
using Plus.HabboRoleplay.Farming;
using Plus.HabboRoleplay.Gambling;

namespace Plus.HabboRoleplay.Misc
{
    public class RoleplayManager
    {
        /// <summary>
        /// Statistic Caps
        /// </summary>
        public static int LevelCap = Convert.ToInt32(RoleplayData.GetData("level", "cap"));
        public static int FarmingLevelCap = Convert.ToInt32(RoleplayData.GetData("farming", "cap"));
        public static int IntelligenceCap = Convert.ToInt32(RoleplayData.GetData("intelligence", "cap"));
        public static int StrengthCap = Convert.ToInt32(RoleplayData.GetData("strength", "cap"));
        public static int StaminaCap = Convert.ToInt32(RoleplayData.GetData("stamina", "cap"));
        public static int DeathTime = Convert.ToInt32(RoleplayData.GetData("hospital", "deathtime"));
        public static int StunGunRange = Convert.ToInt32(RoleplayData.GetData("police", "stungunrange"));
        public static bool LevelDifference = Convert.ToBoolean(RoleplayData.GetData("level", "leveldifference"));
        public static bool AccurateUserCount = Convert.ToBoolean(RoleplayData.GetData("server", "accurateusercount"));
        public static bool StartWorkInPoliceHQ = Convert.ToBoolean(RoleplayData.GetData("police", "startworkinhq"));
        public static bool DayNightSystem = Convert.ToBoolean(RoleplayData.GetData("server", "daynightsystem"));
        public static bool DayNightTaxiTime = Convert.ToBoolean(RoleplayData.GetData("server", "daynighttaxi"));
        public static bool ConfiscateWeapons = Convert.ToBoolean(RoleplayData.GetData("server", "confiscateweapons"));
        public static bool NewVIPAlert = Convert.ToBoolean(RoleplayData.GetData("server", "newvipalerts"));

        public static bool JobCAPTCHABox = Convert.ToBoolean(RoleplayData.GetData("captcha", "job"));
        public static bool WorkoutCAPTCHABox = Convert.ToBoolean(RoleplayData.GetData("captcha", "workout"));
        public static bool FarmingCAPTCHABox = Convert.ToBoolean(RoleplayData.GetData("captcha", "farming"));

        public static int DefaultHitCooldown = Convert.ToInt32(RoleplayData.GetData("combat", "defaulthitcooldown"));
        public static int HitCooldown = Convert.ToInt32(RoleplayData.GetData("combat", "hitcooldown"));
        public static int HitCooldownInEvent = Convert.ToInt32(RoleplayData.GetData("combat", "hitcooldowninevent"));

        public static bool UnloadRoomsAutomatically = Convert.ToBoolean(RoleplayData.GetData("server", "autounloadrooms"));
        public static bool FollowFriends = Convert.ToBoolean(RoleplayData.GetData("server", "followfriends"));
        public static bool PushPullOnArrows = Convert.ToBoolean(RoleplayData.GetData("server", "allowpponarrows"));

        public static int NukeMinutes = Convert.ToInt32(RoleplayData.GetData("npa", "nukeminutes"));
        public static int BreakDownMinutes = Convert.ToInt32(RoleplayData.GetData("npa", "breakdownminutes"));
        public static int NPACoolDown = Convert.ToInt32(RoleplayData.GetData("npa", "cooldown"));

        /// <summary>
        /// Global RP System Timer Manager
        /// </summary>
        public static SystemTimerManager TimerManager = new SystemTimerManager();

        /// <summary>
        /// Gun Store deliveries
        /// </summary>
        public static int UserWhoCalledDelivery = 0;
        public static bool CalledDelivery = false;
        public static Weapon DeliveryWeapon = null;

        /// <summary>
        /// Purge
        /// </summary>
        public static bool PurgeStarted = false;

        /// <summary>
        /// Court stuff
        /// </summary>
        public static bool CourtVoteEnabled = false;
        public static int InnocentVotes = 0;
        public static int GuiltyVotes = 0;

        public static int CourtJuryTime = 0;
        public static bool CourtTrialIsStarting = false;
        public static bool CourtTrialStarted = false;
        public static GameClient Defendant = null;
        public static List<GameClient> InvitedUsersToJuryDuty = new List<GameClient>();
        public static List<GameClient> InvitedUsersToRemove = new List<GameClient>();

        /// <summary>
        /// Thread-safe dictionary containing wanted info
        /// </summary>
        public static ConcurrentDictionary<int, Wanted> WantedList = new ConcurrentDictionary<int, Wanted>();

        /// <summary>
        /// Updates the roleplaymanager variables
        /// </summary>
        public static void UpdateRPData()
        {
            LevelCap = Convert.ToInt32(RoleplayData.GetData("level", "cap"));
            FarmingLevelCap = Convert.ToInt32(RoleplayData.GetData("farming", "cap"));
            IntelligenceCap = Convert.ToInt32(RoleplayData.GetData("intelligence", "cap"));
            StrengthCap = Convert.ToInt32(RoleplayData.GetData("strength", "cap"));
            StaminaCap = Convert.ToInt32(RoleplayData.GetData("stamina", "cap"));
            DeathTime = Convert.ToInt32(RoleplayData.GetData("hospital", "deathtime"));
            StunGunRange = Convert.ToInt32(RoleplayData.GetData("police", "stungunrange"));
            LevelDifference = Convert.ToBoolean(RoleplayData.GetData("level", "leveldifference"));
            AccurateUserCount = Convert.ToBoolean(RoleplayData.GetData("server", "accurateusercount"));
            StartWorkInPoliceHQ = Convert.ToBoolean(RoleplayData.GetData("police", "startworkinhq"));
            DayNightSystem = Convert.ToBoolean(RoleplayData.GetData("server", "daynightsystem"));
            DayNightTaxiTime = Convert.ToBoolean(RoleplayData.GetData("server", "daynighttaxi"));
            ConfiscateWeapons = Convert.ToBoolean(RoleplayData.GetData("server", "confiscateweapons"));
            NewVIPAlert = Convert.ToBoolean(RoleplayData.GetData("server", "newvipalerts"));

            JobCAPTCHABox = Convert.ToBoolean(RoleplayData.GetData("captcha", "job"));
            WorkoutCAPTCHABox = Convert.ToBoolean(RoleplayData.GetData("captcha", "workout"));
            FarmingCAPTCHABox = Convert.ToBoolean(RoleplayData.GetData("captcha", "farming"));

            DefaultHitCooldown = Convert.ToInt32(RoleplayData.GetData("combat", "defaulthitcooldown"));
            HitCooldown = Convert.ToInt32(RoleplayData.GetData("combat", "hitcooldown"));
            HitCooldownInEvent = Convert.ToInt32(RoleplayData.GetData("combat", "hitcooldowninevent"));

            UnloadRoomsAutomatically = Convert.ToBoolean(RoleplayData.GetData("server", "autounloadrooms"));
            FollowFriends = Convert.ToBoolean(RoleplayData.GetData("server", "followfriends"));
            PushPullOnArrows = Convert.ToBoolean(RoleplayData.GetData("server", "pponarrows"));

            NukeMinutes = Convert.ToInt32(RoleplayData.GetData("npa", "nukeminutes"));
            BreakDownMinutes = Convert.ToInt32(RoleplayData.GetData("npa", "breakdownminutes"));
            NPACoolDown = Convert.ToInt32(RoleplayData.GetData("npa", "cooldown"));
    }

        /// <summary>
        /// Gets all vital parts of a users figure
        /// </summary>
        public static string SplitFigure(string Look, string Outfit)
        {
            ConcurrentDictionary<string, string> NewFigure = new ConcurrentDictionary<string, string>();
            string[] Splitted = Look.Split('.');
            string[] Splitted2 = Outfit.Split('.');

            for (int i = 0; i < Splitted.Length; i++)
            {
                string[] SplittedPart = Splitted[i].Split('-');

                if (Splitted == null)
                    continue;

                string BodyPart = SplittedPart[0];
                string Type = SplittedPart[1];
                string Colour;

                if (SplittedPart.Length >= 3)
                    Colour = SplittedPart[2];
                else
                    Colour = "110";

                string SpecificPart = "-" + Type + "-" + Colour;

                if (!NewFigure.ContainsKey(BodyPart))
                    NewFigure.TryAdd(BodyPart, SpecificPart);
                else
                    NewFigure.TryUpdate(BodyPart, SpecificPart, NewFigure[BodyPart]);
            }

            for (int i = 0; i < Splitted2.Length; i++)
            {
                string[] SplittedPart2 = Splitted2[i].Split('-');

                if (Splitted2 == null)
                    continue;

                string BodyPart2 = SplittedPart2[0];
                string Type2 = SplittedPart2[1];
                string Colour2;

                if (SplittedPart2.Length >= 3)
                    Colour2 = SplittedPart2[2];
                else
                    Colour2 = "110";

                string SpecificPart2 = "-" + Type2 + "-" + Colour2;

                if (!NewFigure.ContainsKey(BodyPart2))
                    NewFigure.TryAdd(BodyPart2, SpecificPart2);
                else
                    NewFigure.TryUpdate(BodyPart2, SpecificPart2, NewFigure[BodyPart2]);
            }

            string ReturnFigure = "";

            int count = 0;
            foreach (var Row in NewFigure)
            {
                count++;

                if (NewFigure.Count == count)
                    ReturnFigure += Row.Key + Row.Value;
                else
                    ReturnFigure += Row.Key + Row.Value + ".";
            }

            return ReturnFigure;
        }

        /// <summary>
        /// Gets the distance between 2 points
        /// </summary>
        public static double GetDistanceBetweenPoints2D(Point From, Point To)
        {
            Vector2D Pos1 = new Vector2D(From.X, From.Y);
            Vector2D Pos2 = new Vector2D(To.X, To.Y);

            double XDistance = Math.Abs(Pos1.X - Pos2.X);
            double YDistance = Math.Abs(Pos1.Y - Pos2.Y);

            if (XDistance == 0 && YDistance == 0)
                return 0;

            if (XDistance == 0)
                return YDistance;

            if (YDistance == 0)
                return XDistance;

            double DiagonalDistance = Math.Sqrt(XDistance * XDistance + YDistance * YDistance);

            return DiagonalDistance;
        }

        /// <summary>
        /// Generates a shout message based on paramter session
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="Speech"></param>
        /// <param name="Bubble"></param>
        public static void Chat(GameClient Session, string Speech, int Bubble = 0)
        {
            Room Room = null;
            RoomUser User = null;

            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null || Session.GetRoomUser() == null)
                return;

            Room = Session.GetHabbo().CurrentRoom;
            User = Session.GetRoomUser();

            if (User != null)
            {
                if (User.GetClient() != null && User.GetClient().GetHabbo() != null)
                {
                    if (Room != null)
                    {
                        if (!Room.TutorialEnabled)
                        {
                            foreach (RoomUser roomUser in Room.GetRoomUserManager().GetRoomUsers())
                            {
                                if (roomUser == null || roomUser.IsBot)
                                    continue;

                                if (roomUser.GetClient() == null || roomUser.GetClient().GetConnection() == null)
                                    continue;

                                if (User.GetClient().GetRoleplay().Invisible)
                                    if (User.GetClient().GetHabbo().Username != roomUser.GetClient().GetHabbo().Username && !roomUser.GetClient().GetRoleplay().Invisible)
                                        continue;

                                roomUser.GetClient().SendMessage(new ChatComposer(User.VirtualId, Speech, 0, Bubble));
                            }
                        }
                        else
                            Session.SendMessage(new ChatComposer(User.VirtualId, Speech, 0, Bubble));
                    }
                }
            }
        }
        
        /// <summary>
        /// Generates a shout message based on paramter session
        /// </summary>
        /// <param name="Session"></param>
        /// <param name="Speech"></param>
        /// <param name="Bubble"></param>
        public static void Shout(GameClient Session, string Speech, int Bubble = 0)
        {
            Room Room = null;
            RoomUser User = null;

            if (Speech.StartsWith("*"))
                Speech = "" + Char.ToLowerInvariant(Speech[1]) + Speech.Substring(2);

            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null || Session.GetRoomUser() == null)
                return;

            Room = Session.GetHabbo().CurrentRoom;
            User = Session.GetRoomUser();

            if (User != null)
            {
                if (User.GetClient() != null && User.GetClient().GetHabbo() != null)
                {
                    if (Room != null)
                    {
                        if (!Room.TutorialEnabled)
                        {
                            User.SendNameColourPacket();
                            User.SendMeCommandPacket();

                            foreach (RoomUser roomUser in Room.GetRoomUserManager().GetRoomUsers())
                            {
                                if (roomUser == null || roomUser.IsBot)
                                    continue;

                                if (roomUser.GetClient() == null || roomUser.GetClient().GetConnection() == null)
                                    continue;

                                if (User.GetClient().GetRoleplay().Invisible)
                                    if (User.GetClient().GetHabbo().Username != roomUser.GetClient().GetHabbo().Username && !roomUser.GetClient().GetRoleplay().Invisible)
                                        continue;

                                roomUser.GetClient().SendMessage(new ShoutComposer(User.VirtualId, Speech, 0, Bubble));
                            }
                        }
                        else
                        {
                            User.SendNameColourPacket();
                            User.SendMeCommandPacket();

                            Session.SendMessage(new ShoutComposer(User.VirtualId, Speech, 0, Bubble));
                        }
                    }
                }
                User.SendNamePacket();
            }
        }


        /// <summary>
        /// Generates room based on roomid
        /// </summary>
        /// <param name="RoomId"></param>
        /// <returns></returns>
        public static Room GenerateRoom(int RoomId, bool BotCheck = false)
        {
            if (PlusEnvironment.GetGame() == null || PlusEnvironment.GetGame().GetRoomManager() == null)
                return null;

            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(RoomId, BotCheck);
            return Room;
        }

        /// <summary>
        /// Gets the look of the user
        /// </summary>
        /// <param name="Client"></param>
        /// <returns></returns>
        public static void GetLookAndMotto(GameClient Client, string Type = "")
        {
            string WorkLook = "";
            string Look = Client.GetHabbo().Look;
            string Motto = Client.GetHabbo().Motto;
            string Gender = Client.GetHabbo().Gender;

            if (Type.ToLower() == "poof")
            {
                Look = Client.GetRoleplay().OriginalOutfit;
                Motto = Client.GetRoleplay().Class;
            }

            int JobId = Client.GetRoleplay().JobId;
            int JobRank = Client.GetRoleplay().JobRank;

            Group Group = GroupManager.GetJob(JobId);
            GroupRank GroupRank = GroupManager.GetJobRank(JobId, JobRank);

            if (Client.GetRoleplay().IsDead)
            {
                if (Gender.ToLower() == "m")
                    Look = SplitFigure(Look, "lg-280-83.ch-215-83");

                if (Gender.ToLower() == "f")
                    Look = SplitFigure(Look, "lg-710-83.ch-635-83");

                Motto = "[Morto] " + Client.GetRoleplay().Class;
            }

            if (Client.GetRoleplay().IsJailed)
            {
                Random Random = new Random();
                int PrisonNumber = Random.Next(11111, 100000);

                if (Gender.ToLower() == "m")
                    Look = SplitFigure(Look, "lg-280-1323.sh-3016-92.ch-220-1323");

                if (Gender.ToLower() == "f")
                    Look = SplitFigure(Look, "lg-710-1323.sh-3016-92.ch-3067-1323");

                if (Client.GetRoleplay().Jailbroken)
                    Motto = "[Encerrado] ID [#" + PrisonNumber + "]";
                else
                    Motto = "[Preso] ID [#" + PrisonNumber + "]";
            }

            if (Client.GetRoleplay().IsWorking)
            {
                if (Gender.ToLower() == "m" && GroupRank.MaleFigure != "*")
                    WorkLook = GroupRank.MaleFigure;

                if (Gender.ToLower() == "f" && GroupRank.FemaleFigure != "*")
                    WorkLook = GroupRank.FemaleFigure;

                Look = SplitFigure(Look, WorkLook);
                Motto = "[TRABALHANDO] " + GroupRank.Name;
            }

            if (Client.GetRoleplay().SexTimer > 0)
            {
                if (Gender.ToLower() == "m")
                    Look = SplitFigure(Look, "lg-78322-79.ch-3203-153638.-180-7");

                if (Gender.ToLower() == "f")
                    Look = SplitFigure(Look, "ch-3135-1320.lg-78322-66.-600-1");
            }

            Client.SendMessage(new AvatarAspectUpdateMessageComposer(Look, Gender));

            var RoomUser = Client.GetRoomUser();
            if (RoomUser != null)
            {
                Client.GetHabbo().Look = Look;
                if (RoomUser.IsAsleep)
                    Client.GetHabbo().Motto = "[AUSENTE] - " + Motto;
                else
                    Client.GetHabbo().Motto = Motto;

                Client.SendMessage(new UserChangeComposer(RoomUser, true));

                if (Client.GetHabbo().CurrentRoom != null)
                    Client.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(RoomUser, false));
            }
        }

        /// <summary>
        /// Sends the user to desired bed in the room
        /// </summary>
        /// <param name="Client"></param>
        public static void SpawnBeds(GameClient Client, string BedName, RoomUser Bot = null)
        {
            RoomUser RoomUser;

            if (Bot != null)
                RoomUser = Bot;
            else
                RoomUser = Client.GetRoomUser();

            List<Item> Beds = new List<Item>();

            if (RoomUser == null)
                return;

            if (RoomUser.isSitting || RoomUser.Statusses.ContainsKey("sit"))
            {
                if (RoomUser.Statusses.ContainsKey("sit"))
                    RoomUser.RemoveStatus("sit");
                RoomUser.isSitting = false;
                RoomUser.UpdateNeeded = true;
            }

            if (RoomUser.isLying || RoomUser.Statusses.ContainsKey("lay"))
            {
                if (RoomUser.Statusses.ContainsKey("lay"))
                    RoomUser.RemoveStatus("lay");
                RoomUser.isLying = false;
                RoomUser.UpdateNeeded = true;
            }

            if (RoomUser != null)
                RoomUser.ClearMovement(true);

            lock (RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
            {
                foreach (Item item in RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
                {
                    if (item.GetBaseItem().ItemName == BedName)
                    {
                        if (!Beds.Contains(item))
                            Beds.Add(item);
                    }
                }

                var Beds2 = new List<Item>();
                foreach (var bed in Beds)
                {
                    if (!bed.GetRoom().GetGameMap().SquareHasUsers(bed.GetX, bed.GetY))
                    {
                        if (!Beds2.Contains(bed))
                            Beds2.Add(bed);
                    }
                }
                Item LandItem = null;
                Random Random = new Random();
                if (Beds2.Count >= 1)
                {
                    if (Beds2.Count == 1)
                        LandItem = Beds2[0];
                    else
                        LandItem = Beds2[Random.Next(0, Beds2.Count)];
                }
                else if (Beds.Count >= 1)
                {
                    if (Beds.Count == 1)
                        LandItem = Beds[0];
                    else
                        LandItem = Beds[Random.Next(0, Beds.Count)];
                }

                if (LandItem != null)
                {
                    if (RoomUser.Statusses.ContainsKey("sit"))
                        RoomUser.RemoveStatus("sit");
                    if (RoomUser.Statusses.ContainsKey("lay"))
                        RoomUser.RemoveStatus("lay");
                    RoomUser.Statusses.Add("lay", Utilities.TextHandling.GetString(LandItem.GetBaseItem().Height) + " null");

                    Point OldCoord = new Point(RoomUser.X, RoomUser.Y);
                    Point NewCoord = new Point(LandItem.GetX, LandItem.GetY);


                    RoomUser.X = LandItem.GetX;
                    RoomUser.Y = LandItem.GetY;
                    RoomUser.Z = LandItem.GetZ;
                    RoomUser.RotHead = LandItem.Rotation;
                    RoomUser.RotBody = LandItem.Rotation;

                    RoomUser.UpdateNeeded = true;
                    RoomUser.GetRoom().GetGameMap().UpdateUserMovement(OldCoord, NewCoord, RoomUser);
                }
            }
        }

        /// <summary>
        /// Sends the user to desired chair in the room
        /// </summary>
        /// <param name="Client"></param>
        public static void SpawnChairs(GameClient Client, string ChairName, RoomUser Bot = null)
        {
            try
            {
                RoomUser RoomUser;

                if (Client != null)
                {
                    if (Client.GetHabbo().CurrentRoomId != Convert.ToInt32(RoleplayData.GetData("court", "roomid")))
                    {
                        Client.GetHabbo().Look = Client.GetRoleplay().OriginalOutfit;
                        Client.GetHabbo().Motto = Client.GetRoleplay().Class;
                        Client.GetHabbo().Poof(false);
                    }
                }

                if (Bot != null)
                    RoomUser = Bot;
                else
                    RoomUser = Client.GetRoomUser();

                List<Item> Chairs = new List<Item>();

                if (RoomUser == null)
                    return;

                if (RoomUser.isSitting || RoomUser.Statusses.ContainsKey("sit"))
                {
                    if (RoomUser.Statusses.ContainsKey("sit"))
                        RoomUser.RemoveStatus("sit");
                    RoomUser.isSitting = false;
                    RoomUser.UpdateNeeded = true;
                }

                if (RoomUser.isLying || RoomUser.Statusses.ContainsKey("lay"))
                {
                    if (RoomUser.Statusses.ContainsKey("lay"))
                        RoomUser.RemoveStatus("lay");
                    RoomUser.isLying = false;
                    RoomUser.UpdateNeeded = true;
                }

                RoomUser.CanWalk = false;
                RoomUser.ClearMovement(true);

                lock (RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
                {
                    foreach (Item item in RoomUser.GetRoom().GetRoomItemHandler().GetFloor)
                    {
                        if (item.GetBaseItem().ItemName == ChairName)
                        {
                            if (!Chairs.Contains(item))
                            {
                                Chairs.Add(item);
                            }
                        }
                    }

                    var Chairs2 = new List<Item>();
                    foreach (var chair in Chairs)
                    {
                        if (!chair.GetRoom().GetGameMap().SquareHasUsers(chair.GetX, chair.GetY))
                        {
                            if (!Chairs2.Contains(chair))
                                Chairs2.Add(chair);
                        }
                    }

                    Item LandItem = null;
                    Random Random = new Random();
                    if (Chairs2.Count >= 1)
                    {
                        if (Chairs2.Count == 1)
                            LandItem = Chairs2[0];
                        else
                            LandItem = Chairs2[Random.Next(0, Chairs2.Count)];
                    }
                    else if (Chairs2.Count >= 1)
                    {
                        if (Chairs.Count == 1)
                            LandItem = Chairs[0];
                        else
                            LandItem = Chairs[Random.Next(0, Chairs.Count)];
                    }

                    if (LandItem != null)
                    {
                        if (RoomUser.Statusses.ContainsKey("sit"))
                            RoomUser.RemoveStatus("sit");
                        if (RoomUser.Statusses.ContainsKey("lay"))
                            RoomUser.RemoveStatus("lay");
                        RoomUser.Statusses.Add("sit", Utilities.TextHandling.GetString(LandItem.GetBaseItem().Height));

                        Point OldCoord = new Point(RoomUser.Coordinate.X, RoomUser.Coordinate.Y);
                        Point NewCoord = new Point(LandItem.GetX, LandItem.GetY);

                        var Room = GenerateRoom(RoomUser.RoomId);

                        if (Room != null)
                            Room.GetGameMap().UpdateUserMovement(OldCoord, NewCoord, RoomUser);

                        RoomUser.X = LandItem.GetX;
                        RoomUser.Y = LandItem.GetY;
                        RoomUser.Z = LandItem.GetZ;
                        RoomUser.RotHead = LandItem.Rotation;
                        RoomUser.RotBody = LandItem.Rotation;
                    }
                    RoomUser.CanWalk = true;
                    RoomUser.UpdateNeeded = true;
                }
            }
            catch { }
        }

        /// <summary>
        /// Adds a weapon to the users owned weapons
        /// </summary>
        /// <returns></returns>
        public static void AddWeapon(GameClient Client, Weapon Weapon)
        {
            if (!Client.GetRoleplay().OwnedWeapons.ContainsKey(Weapon.Name.ToLower()))
            {
                using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB.SetQuery("INSERT INTO `rp_weapons_owned` (`user_id`,`base_weapon`,`name`,`min_damage`,`max_damage`,`range`,`clip`) VALUES (@userid,@baseweapon,@name,@mindamage,@maxdamage,@range,@clip)");
                    DB.AddParameter("userid", Client.GetHabbo().Id);
                    DB.AddParameter("baseweapon", Weapon.Name.ToLower());
                    DB.AddParameter("name", Weapon.PublicName);
                    DB.AddParameter("mindamage", Weapon.MinDamage);
                    DB.AddParameter("maxdamage", Weapon.MaxDamage);
                    DB.AddParameter("range", Weapon.Range);
                    DB.AddParameter("clip", Weapon.ClipSize);
                    DB.RunQuery();

                    Client.GetRoleplay().OwnedWeapons.TryAdd(Weapon.Name.ToLower(), Weapon);
                }
            }
            else
            {
                Client.SendWhisper("Você já possui um(a) " + Weapon.PublicName + "!", 1);
            }
        }

        /// <summary>
        /// Sends the user to the room via a thread (instant room loading without glitches)
        /// </summary>
        public static void SendUser(GameClient Client, int RID, string Message = "")
        {
            RoomData roomData = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RID);

            if (Client != null && roomData != null)
            {
                Client.GetRoleplay().AntiArrowCheck = true;

                if (Client.GetHabbo().InRoom)
                {
                    Room OldRoom = null;
                    if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Client.GetHabbo().CurrentRoomId, out OldRoom))
                        return;

                    if (OldRoom.GetRoomUserManager() != null)
                        OldRoom.GetRoomUserManager().RemoveUserFromRoom(Client, false, false);
                }

                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, false, true));
                Client.SendMessage(new GetGuestRoomResultComposer(Client, roomData, true, false));

                if (Message != "")
                    Client.SendMessage(new MOTDNotificationComposer(Message));
            }
            else
                return;
        }

        /// <summary>
        /// Lets you place a furni in the desired location
        /// </summary>
        public static Item PlaceItemToRoom(GameClient Session, int BaseId, int GroupId, int X, int Y, double Z, int Rot, bool FromInventory, int roomid, bool ToDB = true, string ExtraData = "", bool IsFood = false, string deliverytype = "", House House = null, FarmingSpace FarmingSpace = null, TexasHoldEmItem TexasHoldEmData = null)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                Room Room = GenerateRoom(roomid, false);
                int ItDemId = 0;

                // Start at 1 bill to prevent item glitches
                int ItemId = 10000000;

                if (House != null)
                    ItemId = PlusEnvironment.GetGame().GetHouseManager().SignMultiplier + House.RoomId;
                else if (FarmingSpace != null)
                    ItemId = FarmingManager.SignMultiplier + FarmingSpace.Id;
                else if (ToDB)
                {
                    dbClient.SetQuery("INSERT INTO items (user_id,base_item,room_id) VALUES (1, " + BaseId + ", " + roomid + ")");
                    dbClient.RunQuery();
                    dbClient.SetQuery("SELECT id FROM items WHERE user_id = '1' AND room_id = '" + roomid + "' AND base_item = '" + BaseId + "' ORDER BY id DESC LIMIT 1");
                    ItDemId = dbClient.getInteger();
                    ItemId = ItDemId;
                }
                else
                {
                    while (Room.GetRoomItemHandler().GetFloor.Where(x => x.Id == ItemId).ToList().Count > 0)
                        ItemId++;
                }

                Item NewItem = new Item(ItemId, Room.RoomId, BaseId, ExtraData, X, Y, Z, Rot, 0, GroupId, 0, 0, "", null, House, FarmingSpace, TexasHoldEmData);
                NewItem.DeliveryType = deliverytype;

                if (NewItem != null && NewItem.FarmingData != null && NewItem.GetBaseItem().InteractionType == InteractionType.FARMING && Session != null && Session.GetHabbo() != null)
                {
                    NewItem.FarmingData.OwnerId = Session.GetHabbo().Id;

                    new Thread(() =>
                    {
                        if (NewItem != null && NewItem.FarmingData != null)
                            NewItem.FarmingData.BeingFarmed = true;

                        Thread.Sleep(3000);

                        if (Session != null && NewItem != null && NewItem.FarmingData != null)
                        {
                            Session.SendWhisper("O/A " + NewItem.GetBaseItem().PublicName + " que você acabou de plantar está pronto para ser regado!", 1);
                            NewItem.FarmingData.BeingFarmed = false;
                        }
                    }).Start();
                }

                if (IsFood == true)
                {
                    if (Session != null)
                    {
                        NewItem.InteractingUser = Session.GetHabbo().Id;
                        Session = null;
                    }
                }

                if (NewItem != null)
                    Room.GetRoomItemHandler().SetFloorItem(Session, NewItem, X, Y, Rot, true, false, true, false, false, null, Z, true);

                return NewItem;
            }
        }

        /// <summary>
        /// Gets the car name based on cartype
        /// </summary>
        public static string GetCarName(GameClient Client, bool Upgrade = false)
        {
            if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                return "Toyota Corolla";

            int Car = Client.GetRoleplay().CarType;

            if (Upgrade)
                Car++;

            if (Car == 0)
                return "Sem carro";
            else if (Car == 1)
                return "Toyota Corolla";
            else if (Car == 2)
                return "Honda Accord";
            else if (Car == 3)
                return "Nissan GTR";
            else
                return "Nissan GTR";
        }
        
        /// <summary>
        /// Gets the phone name based on phonetype
        /// </summary>
        public static string GetPhoneName(GameClient Client, bool Upgrade = false)
        {
            if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                return "Nokia Tijolão";

            var Phone = Client.GetRoleplay().PhoneType;

            if (Upgrade)
                Phone++;

            if (Phone == 0)
                return "Sem celular";
            else if (Phone == 1)
                return "Nokia Tijolão";
            else if (Phone == 2)
                return "iPhone 4";
            else if (Phone == 3)
                return "iPhone 7";
            else
                return "iPhone 7";
        }

        /// <summary>
        /// Generates a list of coordinates in the room based on a starting and ending coordinate
        /// </summary>
        public static List<ThreeDCoord> GenerateMap(int BeginX, int BeginY, int EndX, int EndY)
        {
            List<ThreeDCoord> Squares = new List<ThreeDCoord>();

            int Length = Math.Abs(BeginX - EndX);
            int Width = Math.Abs(BeginY - EndY);

            for (int i = 0; i < Length; i++)
            {
                Squares.Add(new ThreeDCoord(BeginX + i, BeginY, 0));

                for (int j = 0; j < Width; j++)
                {
                    Squares.Add(new ThreeDCoord(BeginX + i, BeginY + j, 0));
                }
            }

            return Squares;
        }

        /// <summary>
        /// For arrow keys (websockets)
        /// </summary>
        public static Point GetDirectionDeviation(RoomUser User)
        {
            if (User == null)
                return new Point(0, 0);

            if (User.GetClient() == null)
                return new Point(0,0);

            WalkDirections Direction = User.GetClient().GetRoleplay().WalkDirection;
            Point Deviation = new Point(User.Coordinate.X, User.Coordinate.Y);

            if (Direction == WalkDirections.Up)
            {
                 Deviation = new Point(User.Coordinate.X - 2, User.Coordinate.Y);

                 if (!User.GetRoom().GetGameMap().IsValidStep(new Vector2D(User.X, User.Y), new Vector2D(Deviation.X, Deviation.Y),
                     (User.GoalX == Deviation.X && User.GoalY == User.SetY), User.AllowOverride))
                 {
                     Deviation = new Point(User.Coordinate.X - 1, User.Coordinate.Y);
                 }
               
            }

            else if (Direction == WalkDirections.Down)
            {
                Deviation = new Point(User.Coordinate.X + 2, User.Coordinate.Y);
                if (!User.GetRoom().GetGameMap().IsValidStep(new Vector2D(User.X, User.Y), new Vector2D(Deviation.X, Deviation.Y),
                     (User.GoalX == Deviation.X && User.GoalY == User.SetY), User.AllowOverride))
                {
                    Deviation = new Point(User.Coordinate.X + 1, User.Coordinate.Y);
                }
            }

            else if (Direction == WalkDirections.Right)
            {
                Deviation = new Point(User.Coordinate.X, User.Coordinate.Y - 2);
                if (!User.GetRoom().GetGameMap().IsValidStep(new Vector2D(User.X, User.Y), new Vector2D(Deviation.X, Deviation.Y),
                     (User.GoalX == Deviation.X && User.GoalY == User.SetY), User.AllowOverride))
                {
                    Deviation = new Point(User.Coordinate.X, User.Coordinate.Y - 1);
                }
            }
            else if (Direction == WalkDirections.Left)
            {
                Deviation = new Point(User.Coordinate.X, User.Coordinate.Y + 2);
                if (!User.GetRoom().GetGameMap().IsValidStep(new Vector2D(User.X, User.Y), new Vector2D(Deviation.X, Deviation.Y),
                     (User.GoalX == Deviation.X && User.GoalY == User.SetY), User.AllowOverride))
                {
                    Deviation = new Point(User.Coordinate.X, User.Coordinate.Y + 1);
                }
            }
            return Deviation;
        }

        /// <summary>
        /// Sends a delayed whisper alert to a targetted client
        /// </summary>
        /// <param name="Client">Target user</param>
        /// <param name="Msg">Desired message</param>
        /// <param name="Bubble">Desired speech bubble</param>
        /// <param name="Time">Desired Delay (In Seconds)</param>
        public static void SendDelayedWhisper(GameClient Client, string Msg, int Bubble = 1, int Time = 3)
        {
            new Thread(() => {
                Thread.Sleep(Time * 1000);
                if (Client == null) return;
                Client.SendWhisper(Msg, Bubble);
            }).Start();
        }

        public static void RideHorseUser(RoomUser User)
        {

        }

        /// <summary>
        /// Checks if item is near the user.
        /// </summary>
        public static Item GetNearItem(string Item, Room MRoom)
        {
            Item Inter = null;
            lock (MRoom.GetRoomItemHandler().GetFloor)
            {
                foreach (Item item in MRoom.GetRoomItemHandler().GetFloor)
                {
                    if (item == null)
                        continue;

                    if (item.GetBaseItem() == null)
                        continue;

                    if (!item.GetBaseItem().ItemName.Contains(Item))
                        continue;

                    Inter = item;
                }
            }

            return Inter;
        }

        /// <summary>
        /// Checks for offline users (minigames / events)
        /// </summary>
        public static bool OfflineCheck(int UserId, bool GameCheck = false, Games.IGame Game = null)
        {
            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

            #region Generic Checks
            if (Client == null || Client.LoggingOut || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                return true;
            #endregion

            #region Check If Inside Game
            if (GameCheck)
            {
                if (Client.GetRoleplay().Game == null)
                    return true;

                if (Game != null && Client.GetRoleplay().Game != Game)
                    return true;
            }
            #endregion

            return false;
        }

        /// <summary>
        /// Turns the user into a pet
        /// </summary>
        /// <param name="TargetClient"></param>
        /// <param name="Pet"></param>
        public static void MakePet(GameClient TargetClient, string Pet)
        {

            int TargetPetId = RoleplayManager.GetPetIdByString(Pet);

            if (TargetClient == null)
                return;
            if (TargetClient.GetHabbo() == null)
                return;
            if (TargetClient.GetRoomUser() == null)
                return;

            //Change the users Pet Id.         
            TargetClient.GetHabbo().PetId = (TargetPetId == -1 ? 0 : TargetPetId);

            //Quickly remove the old user instance.
            TargetClient.GetRoomUser().GetRoom().SendMessage(new UserRemoveComposer(TargetClient.GetRoomUser().VirtualId));

            //Add the new one, they won't even notice a thing!!11 8-)
            TargetClient.GetRoomUser().GetRoom().SendMessage(new UsersComposer(TargetClient.GetRoomUser()));
        }

        public static int GetPetIdByString(string Pet)
        {
            switch (Pet.ToLower())
            {
                default:
                    return 0;
                case "habbo":
				case "humano":
                    return -1;
                case "dog":
				case "cachorro":
                    return 60;//This should be 0.
                case "cat":
				case "gato":
                    return 1;
                case "terrier":
				case "cachorrinho":
                    return 2;
                case "croc":
                case "croco":
				case "crocodilo":
                    return 3;
                case "bear":
				case "urso":
                    return 4;
                case "pig":
				case "porco":
                    return 5;
                case "lion":
				case "leao":
                    return 6;
                case "rhino":
				case "rino":
				case "rinoceronte":
                    return 7;
                case "spider":
				case "aranha":
                    return 8;
                case "turtle":
				case "tartaruga":
                    return 9;
                case "chick":
                case "chicken":
				case "pintinho":
				case "pinto":
                    return 10;
                case "frog":
				case "sapo":
				case "ra":
                    return 11;
                case "drag":
                case "dragon":
				case "dragao":
                    return 12;
                case "monkey":
				case "macaco":
                    return 14;
                case "horse":
				case "cavalo":
                    return 15;
                case "plant":
				case "cdemonio":
                    return 16;
                case "bunny":
				case "coelho":
                    return 17;
                case "evilbunny":
				case "coelhomal":
                    return 18;
                case "brownbunny":
				case "coelhomarrom":
                    return 19;
                case "pinkbunny":
				case "coelhorosa":
                    return 20;
                case "whitepigeon":
                case "whitechick":
				case "pintobranco":
                    return 21;
                case "blackpigeon":
                case "blackchick":
				case "pintopreto":
                    return 22;
                case "demon":
                case "evilmonkey":
                case "demonmonkey":
				case "demonio":
                    return 23;
                case "bbear":
                case "babybear":
                    return 24;
                case "bterrier":
                case "babyterrier":
                    return 25;
                case "gnome":
                    return 26;
                case "kitty":
                case "kitten":
                    return 28;
                case "puppy":
                case "doggy":
                    return 29;
                case "bpig":
                case "piggy":
                case "babypig":
                    return 30;
                case "oompa":
                case "oompaloompa":
                    return 31;
                case "rock":
				case "pedra":
                    return 32;
                case "ptera":
                case "pteradactyl":
                    return 33;
                case "trex":
                case "dino":
                    return 34;
            }
        }
    }
}