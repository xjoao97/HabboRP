using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.Games;

using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Incoming;

using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;

using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Rooms.Chat.Commands;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUser
    {
        public bool AllowOverride;
        public BotAI BotAI;
        public RoleplayBotAI RPBotAI;

        public RoomBot BotData;
        public RoleplayBot RPBotData;

        public RoomUser Attacker;

        public bool CanWalk;
        public int CarryItemID; //byte
        public int CarryTimer; //byte
        public int ChatSpamCount = 0;
        public int ChatSpamTicks = 16;
        public ItemEffectType CurrentItemEffect;
        public int DanceId;
        public bool FastWalking = false;
        public bool SuperFastWalking = false;
        public int FreezeCounter;
        public int FreezeLives;
        public bool Freezed;
        public bool Frozen;
        public int GateId;

        public int GoalX; //byte
        public int GoalY; //byte
        public int HabboId;
        public int HorseID = 0;
        public int IdleTime; //byte
        public bool InteractingGate;
        public int InternalRoomID;
        public bool IsAsleep;
        public bool IsWalking;
        public int LastBubble = 0;
        public double LastInteraction;
        public Item LastItem = null;
        public int LockedTilesCount;

        public List<Vector2D> Path = new List<Vector2D>();
        public bool PathRecalcNeeded = false;
        public int PathStep = 1;
        public Pet PetData;

        public int PrevTime;
        public bool RidingHorse = false;
        public int RoomId;
        public int RotBody; //byte
        public int RotHead; //byte

        public bool SetStep;
        public int SetX; //byte
        public int SetY; //byte
        public double SetZ;
        public double SignTime;
        public byte SqState;
        public Dictionary<string, string> Statusses;
        public int TeleDelay; //byte
        public bool TeleportEnabled;
        public bool UpdateNeeded;
        public int VirtualId;

        public int X; //byte
        public int Y; //byte
        public double Z;

        public FreezePowerUp banzaiPowerUp;
        public bool isLying = false;
        public bool isSitting = false;
        private GameClient mClient;
        private Room mRoom;
        public bool moonwalkEnabled = false;
        public bool shieldActive;
        public int shieldCounter;
        public TEAM Team;
        public bool FreezeInteracting;
        public int UserId;
        public bool IsJumping;

        public bool isRolling = false;
        public int rollerDelay = 0;

        public int LLPartner = 0;
        public double TimeInRoom = 0;

        public bool ForceSit = false;
        public bool ForceLay = false;

        public RoomUser(int HabboId, int RoomId, int VirtualId, Room room)
        {
            this.Freezed = false;
            this.HabboId = HabboId;
            this.RoomId = RoomId;
            this.VirtualId = VirtualId;
            this.IdleTime = 0;

            this.X = 0;
            this.Y = 0;
            this.Z = 0;
            this.PrevTime = 0;
            this.RotHead = 0;
            this.RotBody = 0;
            this.UpdateNeeded = true;
            this.Statusses = new Dictionary<string, string>();

            this.TeleDelay = -1;
            this.mRoom = room;

            this.AllowOverride = false;
            this.CanWalk = true;


            this.SqState = 3;

            this.InternalRoomID = 0;
            this.CurrentItemEffect = ItemEffectType.NONE;

            this.FreezeLives = 0;
            this.InteractingGate = false;
            this.GateId = 0;
            this.LastInteraction = 0;
            this.LockedTilesCount = 0;

            this.IsJumping = false;
            this.TimeInRoom = 0;
        }

        public Point GetUniqueSpot(int Spot)
        {
            if (Spot == 1)
                return this.SquareBehind;
            if (Spot == 2)
                return this.SquareInFront;
            if (Spot == 3)
                return this.SquareRight;
            if (Spot == 4)
                return this.SquareLeft;

            return new Point(0, 0);
        }

        public bool IsRoleplayBot
        {
            get { return (RPBotData != null && IsBot); }
        }

        public Point Coordinate
        {
            get { return new Point(X, Y); }
        }
       
        public bool IsPet
        {
            get { return (IsBot && BotData.IsPet); }
        }

        public int CurrentEffect
        {
            get
            {
                if (GetClient() == null)
                    return 0;

                if (GetClient().GetHabbo() == null)
                    return 0;

                if (GetClient().GetHabbo().Effects() == null)
                    return 0;

                return GetClient().GetHabbo().Effects().CurrentEffect;
            }
        }


        public bool IsDancing
        {
            get
            {
                if (DanceId >= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public bool NeedsAutokick
        {
            get
            {
                if (IsBot)
                    return false;

                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return true;

                if (GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || GetRoom().OwnerId == HabboId)
                    return false;

                if (IdleTime >= 7200)
                    return true;

                return false;
            }
        }

        public bool IsTrading
        {
            get
            {
                if (IsBot)
                    return false;

                if (Statusses.ContainsKey("trd"))
                    return true;

                return false;
            }
        }

        public bool IsBot
        {
            get
            {
                if (BotData != null)
                    return true;

                return false;
            }
        }

        public string GetUsername()
        {
            if (IsBot)
                return string.Empty;

            if (GetClient() != null)
            {
                if (GetClient().GetHabbo() != null)
                {
                    return GetClient().GetHabbo().Username;
                }
                else
                    return PlusEnvironment.GetUsernameById(HabboId);

            }
            else
                return PlusEnvironment.GetUsernameById(HabboId);
        } 

        public RoleplayBot GetBotRoleplay()
        {
            if (!IsBot)
                return null;
            else
                return RPBotData;
        }

        public RoleplayBotAI GetBotRoleplayAI()
        {
            if (!IsBot)
                return null;
            else
                return RPBotAI;
        }

        public void UnIdle(bool ForcedWakeup = false)
        {
            if (!IsBot)
            {
                if (GetClient() != null && GetClient().GetHabbo() != null)
                    GetClient().GetHabbo().TimeAFK = 0;
            }

            IdleTime = 0;

            if (IsAsleep)
            {
                IsAsleep = false;
                GetRoom().SendMessage(new SleepComposer(this, false));

                if (GetClient() != null && GetClient().GetHabbo() != null && GetClient().GetRoleplay() != null)
                {
                    if (!GetClient().GetRoleplay().IsJailed && !GetClient().GetRoleplay().IsDead && GetClient().GetRoleplay().Game == null)
                    {
                        GetClient().GetHabbo().Motto = GetClient().GetRoleplay().Class;
                        GetClient().GetHabbo().Poof(false);
                    }
                }
            }
        }

        public void Dispose()
        {
            Statusses.Clear();
            mRoom = null;
            mClient = null;
        }

        public void Chat(string Message, bool Shout = true, int colour = 0)
        {
            if (GetRoom() == null)
                return;

            if (!IsBot)
                return;

            if (IsPet)
            {
                foreach (RoomUser User in GetRoom().GetRoomUserManager().GetUserList().ToList())
                {
                    if (User == null || User.IsBot)
                        continue;

                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;

                    if (!User.GetClient().GetHabbo().AllowPetSpeech)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, 0));
                }
            }
            else
            {

                if (GetRoom() == null) return;
                if (GetRoom().GetRoomUserManager() == null) return;
                if (GetRoom().GetRoomUserManager().GetUserList() == null) return;
 
                foreach (RoomUser User in GetRoom().GetRoomUserManager().GetUserList().ToList())
                {
                    if (User == null || User.IsBot)
                        continue;

                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;

                    if (!Shout)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, (colour == 0 ? 2 : colour)));
                    else
                        User.GetClient().SendMessage(new ShoutComposer(VirtualId, Message, 0, (colour == 0 ? 2 : colour)));
                }
            }
        }

        public void HandleSpamTicks()
        {
            if (ChatSpamTicks >= 0)
            {
                ChatSpamTicks--;

                if (ChatSpamTicks == -1)
                {
                    ChatSpamCount = 0;
                }
            }
        }

        public bool IncrementAndCheckFlood(out int MuteTime)
        {
            MuteTime = 0;

            ChatSpamCount++;
            if (ChatSpamTicks == -1)
                ChatSpamTicks = 8;
            else if (ChatSpamCount >= 6)
            {
                if (GetClient().GetHabbo().GetPermissions().HasRight("events_staff"))
                    MuteTime = 3;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("gold_vip"))
                    MuteTime = 7;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("silver_vip"))
                    MuteTime = 10;
                else
                    MuteTime = 15;

                GetClient().GetHabbo().FloodTime = PlusEnvironment.GetUnixTimestamp() + MuteTime;

                ChatSpamCount = 0;
                return true;
            }
            return false;
        }

        public void OnChat(int Colour, string Message, bool Shout)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null || mRoom == null)
                return;

            if (mRoom.GetWired().TriggerEvent(Items.Wired.WiredBoxType.TriggerUserSays, GetClient().GetHabbo(), Message))
                return;

            if (UsingColourCode(Message))
            {
                if (GetClient().GetHabbo().VIPRank < 1)
                    Message = ReplaceColourCode(Message);
            }

            GetClient().GetHabbo().HasSpoken = true;

            if (mRoom.WordFilterList.Count > 0 && !GetClient().GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = mRoom.GetFilter().CheckMessage(Message);

            ServerPacket Packet = null;

            if (GetClient().GetHabbo().Translating)
            {
                string LG1 = GetClient().GetHabbo().FromLanguage.ToLower();
                string LG2 = GetClient().GetHabbo().ToLanguage.ToLower();

                if (Shout)
                    Packet = new ShoutComposer(VirtualId, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);
                else
                    Packet = new ChatComposer(VirtualId, PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);
            }
            else
            {
                if (Shout)
                    Packet = new ShoutComposer(VirtualId, Message, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);
                else
                    Packet = new ChatComposer(VirtualId, Message, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);

            }

            if (GetClient().GetHabbo().TentId > 0)
            {
                mRoom.SendToTent(GetClient().GetHabbo().Id, GetClient().GetHabbo().TentId, Packet);

                Packet = new WhisperComposer(this.VirtualId, "[Chat da Tenda] " + Message, 0, Colour);

                List<RoomUser> ToNotify = mRoom.GetRoomUserManager().GetRoomUserByRank(2);

                if (ToNotify.Count > 0)
                {
                    SendNameColourPacket();
                    foreach (RoomUser user in ToNotify)
                    {
                        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null ||
                            user.GetClient().GetHabbo().TentId == GetClient().GetHabbo().TentId)
                        {
                            continue;
                        }

                        user.GetClient().SendMessage(Packet);
                    }
                    SendNamePacket();
                }
            }
            else
            {
                if (mRoom.TutorialEnabled)
                    this.GetClient().SendMessage(Packet);
                else
                {
                    SendNameColourPacket();
                    foreach (RoomUser User in mRoom.GetRoomUserManager().GetRoomUsers().ToList())
                    {
                        if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().MutedUsers.Contains(mClient.GetHabbo().Id))
                            continue;

                        if (GetClient().GetRoleplay().Invisible)
                        {
                            if (!User.GetClient().GetRoleplay().Invisible)
                                continue;
                        }

                        User.GetClient().SendMessage(Packet);
                    }
                    SendNamePacket();
                }
            }

            #region Pets/Bots responces
            if (Shout)
            {
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (!User.IsBot)
                        continue;

                    if (User.IsBot)
                    {
                        if (User.GetBotRoleplayAI() != null)
                            User.GetBotRoleplayAI().OnUserShout(this, Message);
                        else
                            User.BotAI.OnUserShout(this, Message);
                    }
                }
            }
            else
            {
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (!User.IsBot)
                        continue;

                    if (User.GetBotRoleplayAI() != null)
                        User.GetBotRoleplayAI().OnUserSay(this, Message);
                    else
                        User.BotAI.OnUserSay(this, Message);
                }
            }
            #endregion
        }

        public bool UsingColourCode(string Message)
        {
            bool UsingColour = false;
            string Starting = Message.Split(' ')[0];

            if (Starting.ToLower().Contains("@red@"))
                UsingColour = true;
            else if (Starting.ToLower().Contains("@blue@"))
                UsingColour = true;
            else if (Starting.ToLower().Contains("@purple@"))
                UsingColour = true;
            else if (Starting.ToLower().Contains("@green@"))
                UsingColour = true;
            else if (Starting.ToLower().Contains("@cyan@"))
                UsingColour = true;

            return UsingColour;
        }

        public string ReplaceColourCode(string Message)
        {
            string NewMessage = Message;
            string Starting = Message.Split(' ')[0];

            if (Starting.ToLower().Contains("@red@"))
                NewMessage = Message.Replace("@red@", String.Empty);
            else if (Starting.ToLower().Contains("@blue@"))
                NewMessage = Message.Replace("@blue@", String.Empty);
            else if (Starting.ToLower().Contains("@purple@"))
                NewMessage = Message.Replace("@purple@", String.Empty);
            else if (Starting.ToLower().Contains("@green@"))
                NewMessage = Message.Replace("@green@", String.Empty);
            else if (Starting.ToLower().Contains("@cyan@"))
                NewMessage = Message.Replace("@cyan@", String.Empty);

            return NewMessage;
        }

        public void SendNameColourPacket()
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null)
                return;

            if (GetClient().GetHabbo().Colour == null || GetClient().GetHabbo().Colour == String.Empty)
                return;

            if (GetClient().GetHabbo().ChatPreference)
                return;

            string Username = "<font color='#" + GetClient().GetHabbo().Colour + "'>" + GetClient().GetHabbo().Username + "</font>";

            if (GetClient().GetHabbo().Colour.ToLower() == "rainbow")
                Username = CommandManager.GenerateRainbowText(GetClient().GetHabbo().Username);

            if (GetRoom() != null)
                GetRoom().SendMessage(new UserNameChangeComposer(RoomId, VirtualId, Username));
        }

        public void SendMeCommandPacket()
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null)
                return;

            string Username = "*" + GetClient().GetHabbo().Username;

            if (!GetClient().GetHabbo().ChatPreference)
            {
                if (GetClient().GetHabbo().Colour != null && GetClient().GetHabbo().Colour != String.Empty)
                {
                    if (GetClient().GetHabbo().Colour.ToLower() == "rainbow")
                        Username = "*" + CommandManager.GenerateRainbowText(GetClient().GetHabbo().Username);
                    else
                        Username = "*<font color='#" + GetClient().GetHabbo().Colour + "'>" + GetClient().GetHabbo().Username + "</font>";
                }
            }
            if (GetRoom() != null)
                GetRoom().SendMessage(new UserNameChangeComposer(RoomId, VirtualId, Username));
        }

        public void SendNamePacket()
        {
            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null)
                return;

            string Username = GetClient().GetHabbo().Username;

            if (GetRoom() != null)
                GetRoom().SendMessage(new UserNameChangeComposer(RoomId, VirtualId, Username));
        }

        public void ClearMovement(bool Update)
        {
            IsWalking = false;
            Statusses.Remove("mv");
            GoalX = 0;
            GoalY = 0;
            SetStep = false;
            SetX = 0;
            SetY = 0;
            SetZ = 0;

            if (Update)
            {
                UpdateNeeded = true;
            }
        }

        public void MoveTo(Point c)
        {
            MoveTo(c.X, c.Y);
        }

        public void MoveTo(int pX, int pY, bool pOverride)
        {

            if (this.ForceLay || this.ForceSit)
                return;

            if (TeleportEnabled)
            {
                Point CurrentPoint = new Point(X, Y);
                Point NewPoint = new Point(pX, pY);

                if (CurrentPoint == NewPoint)
                    return;

                List<Item> Items = GetRoom().GetGameMap().GetAllRoomItemForSquare(pX, pY);

                if (isLying || Statusses.ContainsKey("lay"))
                {
                    bool HasBed = Items.ToList().Where(x => x != null && x.GetBaseItem().IsBed()).Count() > 0;

                    if (HasBed)
                    {
                        var Item = Items.ToList().Where(x => x.GetBaseItem().IsBed()).First();

                        if (Item.GetX == CurrentPoint.X && Item.GetY == CurrentPoint.Y)
                            return;
                    }
                }

                if (isSitting || Statusses.ContainsKey("sit"))
                {
                    if (Statusses.ContainsKey("sit"))
                        RemoveStatus("sit");
                    isSitting = false;
                }

                if (isLying || Statusses.ContainsKey("lay"))
                {
                    if (Statusses.ContainsKey("lay"))
                        RemoveStatus("lay");
                    isLying = false;
                }

                UnIdle();
                GetRoom().SendMessage(GetRoom().GetRoomItemHandler().UpdateUserOnRoller(this, NewPoint, 0, GetRoom().GetGameMap().SqAbsoluteHeight(pX, pY, GetRoom().GetGameMap().GetAllRoomItemForSquare(pX, pY))));

                if (Items.Count > 0)
                {
                    bool HasBed = Items.ToList().Where(x => x != null && x.GetBaseItem().IsBed()).Count() > 0;
                    bool HasChair = Items.ToList().Where(x => x != null && x.GetBaseItem().IsSeat).Count() > 0;

                    #region BedCheck
                    if (HasBed)
                    {
                        var Item = Items.ToList().Where(x => x.GetBaseItem().IsBed()).First();

                        Statusses.Add("lay", Utilities.TextHandling.GetString(Item.GetBaseItem().Height) + " null");

                        X = Item.GetX;
                        Y = Item.GetY;
                        Z = Item.GetZ;
                        RotHead = Item.Rotation;
                        RotBody = Item.Rotation;

                        Point OldCoord = new Point(X, Y);
                        Point NewCoord = new Point(Item.GetX, Item.GetY);

                        GetRoom().GetGameMap().UpdateUserMovement(OldCoord, NewCoord, this);
                    }
                    #endregion

                    #region ChairCheck
                    else if (HasChair)
                    {
                        var Item = Items.ToList().Where(x => x.GetBaseItem().IsSeat).First();
                        Statusses.Add("sit", Utilities.TextHandling.GetString(Item.GetBaseItem().Height));

                        Z = Item.GetZ;
                        RotHead = Item.Rotation;
                        RotBody = Item.Rotation;
                    }
                    #endregion
                }
                UpdateNeeded = true;
                return;
            }

            if ((!IsBot && !GetRoom().TutorialEnabled && GetRoom().GetGameMap().SquareHasUsers(pX, pY, true, this.GetClient().GetRoleplay().Invisible) && !pOverride && (X != pX && Y != pY)) || Frozen)
                return;

            UnIdle();
            GoalX = pX;
            GoalY = pY;
            PathRecalcNeeded = true;
            FreezeInteracting = false;
        }
        public void MoveTo(int pX, int pY)
        {
            MoveTo(pX, pY, false);
        }

        public void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }


        public void SetPos(int pX, int pY, double pZ)
        {
            X = pX;
            Y = pY;
            Z = pZ;
        }

        public void CarryItem(int Item)
        {
            CarryItemID = Item;

            if (Item > 0)
                CarryTimer = 240;
            else
                CarryTimer = 0;

            GetRoom().SendMessage(new CarryObjectComposer(VirtualId, Item));
        }


        public void SetRot(int Rotation, bool HeadOnly)
        {
            if (Statusses.ContainsKey("lay") || IsWalking)
            {
                return;
            }

            int diff = RotBody - Rotation;

            RotHead = RotBody;

            if (Statusses.ContainsKey("sit") || HeadOnly)
            {
                if (RotBody == 2 || RotBody == 4)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
                else if (RotBody == 0 || RotBody == 6)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
            }
            else if (diff <= -2 || diff >= 2)
            {
                RotHead = Rotation;
                RotBody = Rotation;
            }
            else
            {
                RotHead = Rotation;
            }

            UpdateNeeded = true;
        }

        public void SetStatus(string Key, string Value)
        {
            if (Statusses.ContainsKey(Key))
            {
                Statusses[Key] = Value;
            }
            else
            {
                AddStatus(Key, Value);
            }
        }

        public void AddStatus(string Key, string Value)
        {
            Statusses[Key] = Value;
        }

        public void RemoveStatus(string Key)
        {
            if (Statusses.ContainsKey(Key))
            {
                Statusses.Remove(Key);
            }
        }

        public void ApplyEffect(int effectID)
        {
            if (IsBot)
            {
                this.mRoom.SendMessage(new AvatarEffectComposer(VirtualId, effectID));
                return;
            }

            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().Effects() == null)
                return;

            GetClient().GetHabbo().Effects().ApplyEffect(effectID);
        }

        public Point SquareInFront
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.Y--;
                }
                else if (RotBody == 2)
                {
                    Sq.X++;
                }
                else if (RotBody == 4)
                {
                    Sq.Y++;
                }
                else if (RotBody == 6)
                {
                    Sq.X--;
                }

                return Sq;
            }
        }

        public Point SquareBehind
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.Y++;
                }
                else if (RotBody == 2)
                {
                    Sq.X--;
                }
                else if (RotBody == 4)
                {
                    Sq.Y--;
                }
                else if (RotBody == 6)
                {
                    Sq.X++;
                }

                return Sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.X++;
                }
                else if (RotBody == 2)
                {
                    Sq.Y--;
                }
                else if (RotBody == 4)
                {
                    Sq.X--;
                }
                else if (RotBody == 6)
                {
                    Sq.Y++;
                }

                return Sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.X--;
                }
                else if (RotBody == 2)
                {
                    Sq.Y++;
                }
                else if (RotBody == 4)
                {
                    Sq.X++;
                }
                else if (RotBody == 6)
                {
                    Sq.Y--;
                }
                return Sq;
            }
        }


        public GameClient GetClient()
        {
            if (IsBot)
            {
                return null;
            }
            if (mClient == null)
                mClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboId);
            return mClient;
        }

        public Room GetRoom()
        {
            if (mRoom == null)
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out mRoom))
                    return mRoom;

            return mRoom;
        }
    }

    public enum ItemEffectType
    {
        NONE,
        SWIM,
        SwimLow,
        SwimHalloween,
        Iceskates,
        Normalskates,
        PublicPool,
        //Skateboard?
    }

    public static class ByteToItemEffectEnum
    {
        public static ItemEffectType Parse(byte pByte)
        {
            switch (pByte)
            {
                case 0:
                    return ItemEffectType.NONE;
                case 1:
                    return ItemEffectType.SWIM;
                case 2:
                    return ItemEffectType.Normalskates;
                case 3:
                    return ItemEffectType.Iceskates;
                case 4:
                    return ItemEffectType.SwimLow;
                case 5:
                    return ItemEffectType.SwimHalloween;
                case 6:
                    return ItemEffectType.PublicPool;
                //case 7:
                //return ItemEffectType.Custom;
                default:
                    return ItemEffectType.NONE;
            }
        }
    }

    //0 = none
    //1 = pool
    //2 = normal skates
    //3 = ice skates
}