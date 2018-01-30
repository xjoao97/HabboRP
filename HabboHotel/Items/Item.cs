using System;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Collections.Generic;

using Plus.Core;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items.Data.RentableSpace;
using Plus.HabboHotel.Items.Data.WhisperTile;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Items.Interactor;


using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;


using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboRoleplay.Gambling;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Farming;
using System.Collections.Concurrent;
using System.Threading;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Items
{

    public class Item
    {
        public int Id;
        private ItemData _data;
        public int BaseItem;
        public string ExtraData;
        public string Figure;
        public string Gender;
        public int GroupId;
        public int InteractingUser;
        public int InteractingUser2;
        public int LimitedNo;
        public int LimitedTot;
        public bool MagicRemove = false;
        public int RoomId;
        public int Rotation;
        public int UpdateCounter;
        public int UserID;
        public string Username;
        public int interactingBallUser;
        public byte interactionCount;
        public byte interactionCountHelper;

        public string DeliveryType = "";

        private int _coordX;
        private int _coordY;
        private double _coordZ;

        public TEAM team;
        public bool pendingReset = false;
        public FreezePowerUp freezePowerUp;


        public int value;
        public string wallCoord;
        private bool updateNeeded;

        private Room _room;
        private static Random _random = new Random();
        private List<Point> _affectedPoints;
        public RentableSpaceData RentableSpaceData;
        public WhisperTileData WhisperTileData;
        public FarmingData FarmingData;
        public TexasHoldEmItem TexasHoldEmData;

        private readonly bool mIsRoller;
        private readonly bool mIsWallItem;
        private readonly bool mIsFloorItem;

        public Item(int Id, int RoomId, int BaseItem, string ExtraData, int X, int Y, Double Z, int Rot, int Userid, int Group, int limitedNumber, int limitedStack, string wallCoord, Room Room = null, House House = null, FarmingSpace FarmingSpace = null, TexasHoldEmItem TexasHoldEmData = null)
        {
            ItemData Data = null;
            if (PlusEnvironment.GetGame().GetItemManager().GetItem(BaseItem, out Data))
            {
                this.Id = Id;
                this.RoomId = RoomId;
                this._room = Room;
                this._data = Data;
                this.BaseItem = BaseItem;
                this.ExtraData = ExtraData;
                this.GroupId = Group;

                this._coordX = X;
                this._coordY = Y;
                if (!double.IsInfinity(Z))
                    this._coordZ = Z;
                this.Rotation = Rot;
                this.UpdateNeeded = false;
                this.UpdateCounter = 0;
                this.InteractingUser = 0;
                this.InteractingUser2 = 0;
                this.interactingBallUser = 0;
                this.interactionCount = 0;
                this.value = 0;

                this.UserID = Userid;
                this.Username = PlusEnvironment.GetUsernameById(Userid);

                this.LimitedNo = limitedNumber;
                this.LimitedTot = limitedStack;

                this.TexasHoldEmData = TexasHoldEmData;

                if (GetBaseItem().InteractionType == InteractionType.FARMING)
                    this.FarmingData = new FarmingData(Id);
                else
                    this.FarmingData = null;

                if (GetBaseItem().InteractionType == InteractionType.RENTABLE_SPACE)
                {
                    if (House != null)
                        this.RentableSpaceData = new RentableSpaceData(House, Id);
                    else if (FarmingSpace != null)
                        this.RentableSpaceData = new RentableSpaceData(FarmingSpace, Id);
                    else
                        this.RentableSpaceData = new RentableSpaceData(Id);
                }
                else
                    this.RentableSpaceData = null;

                if (GetBaseItem().InteractionType == InteractionType.WHISPER_TILE)
                    this.WhisperTileData = new WhisperTileData(Id);
                else
                    this.WhisperTileData = null;

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.TELEPORT:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.HOPPER:
                        RequestUpdate(0, true);
                        break;

                    case InteractionType.ROLLER:
                        mIsRoller = true;
                        if (RoomId > 0)
                        {
                            GetRoom().GetRoomItemHandler().GotRollers = true;
                        }
                        break;

                    case InteractionType.banzaiscoreblue:
                    case InteractionType.footballcounterblue:
                    case InteractionType.banzaigateblue:
                    case InteractionType.FREEZE_BLUE_GATE:
                    case InteractionType.freezebluecounter:
                        team = TEAM.BLUE;
                        break;

                    case InteractionType.banzaiscoregreen:
                    case InteractionType.footballcountergreen:
                    case InteractionType.banzaigategreen:
                    case InteractionType.freezegreencounter:
                    case InteractionType.FREEZE_GREEN_GATE:
                        team = TEAM.GREEN;
                        break;

                    case InteractionType.banzaiscorered:
                    case InteractionType.footballcounterred:
                    case InteractionType.banzaigatered:
                    case InteractionType.freezeredcounter:
                    case InteractionType.FREEZE_RED_GATE:
                        team = TEAM.RED;
                        break;

                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.footballcounteryellow:
                    case InteractionType.banzaigateyellow:
                    case InteractionType.freezeyellowcounter:
                    case InteractionType.FREEZE_YELLOW_GATE:
                        team = TEAM.YELLOW;
                        break;

                    case InteractionType.banzaitele:
                        {
                            this.ExtraData = "";
                            break;
                        }
                }

                this.mIsWallItem = (GetBaseItem().Type.ToString().ToLower() == "i");
                this.mIsFloorItem = (GetBaseItem().Type.ToString().ToLower() == "s");

                if (this.mIsFloorItem)
                    this._affectedPoints = Gamemap.GetAffectedTiles(GetBaseItem().Length, GetBaseItem().Width, GetX, GetY, Rot).Values.Select(x => new Point(x.X, x.Y)).ToList();
                else if (this.mIsWallItem)
                {
                    this.wallCoord = wallCoord;
                    this.mIsWallItem = true;
                    this.mIsFloorItem = false;
                    this._affectedPoints = new List<Point>();
                }
            }
        }

        public ItemData Data
        {
            get { return this._data; }
            set { this._data = value; }
        }

        public List<Point> GetAffectedTiles
        {
            get { return this._affectedPoints; }
        }

        public int GetX
        {
            get { return _coordX; }
            set { this._coordX = value; }
        }

        public int GetY
        {
            get { return _coordY; }
            set { this._coordY = value; }
        }

        public double GetZ
        {
            get { return _coordZ; }
            set { this._coordZ = value; }
        }

        public bool UpdateNeeded
        {
            get { return updateNeeded; }
            set
            {
                if (value && GetRoom() != null)
                    GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);
                updateNeeded = value;
            }
        }

        public bool IsRoller
        {
            get { return mIsRoller; }
        }

        public Point Coordinate
        {
            get { return new Point(GetX, GetY); }
        }

        public List<Point> GetCoords
        {
            get
            {
                var toReturn = new List<Point>();
                toReturn.Add(Coordinate);

                foreach (Point tile in _affectedPoints.ToList())
                {
                    toReturn.Add(new Point(tile.X, tile.Y));
                }

                return toReturn;
            }
        }

        public List<Point> GetSides()
        {
            var toReturn = new List<Point>();
            toReturn.Add(SquareBehind);
            toReturn.Add(SquareInFront);
            toReturn.Add(SquareLeft);
            toReturn.Add(SquareRight);
            toReturn.Add(Coordinate);
            return toReturn;
        }

        public double TotalHeight
        {
            get
            {
                double CurHeight = 0.0;
                if (this.GetBaseItem().AdjustableHeights.Count > 0)
                {
                    if (this.ExtraData == "")
                        this.ExtraData = "0";

                    if (this.GetBaseItem().AdjustableHeights.ContainsKey(this.ExtraData))
                        CurHeight = this.GetZ + this.GetBaseItem().AdjustableHeights[this.ExtraData];
                    else
                        CurHeight = this.GetZ + this.GetBaseItem().Height;
                }
                else
                    CurHeight = this.GetZ + this.GetBaseItem().Height;
                return CurHeight;
            }
        }

        public bool IsWallItem
        {
            get { return mIsWallItem; }
        }

        public bool IsFloorItem
        {
            get { return mIsFloorItem; }
        }

        public Point SquareInFront
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.Y--;
                }
                else if (Rotation == 2)
                {
                    Sq.X++;
                }
                else if (Rotation == 4)
                {
                    Sq.Y++;
                }
                else if (Rotation == 6)
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
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.Y++;
                }
                else if (Rotation == 2)
                {
                    Sq.X--;
                }
                else if (Rotation == 4)
                {
                    Sq.Y--;
                }
                else if (Rotation == 6)
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
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.X++;
                }
                else if (Rotation == 2)
                {
                    Sq.Y--;
                }
                else if (Rotation == 4)
                {
                    Sq.X--;
                }
                else if (Rotation == 6)
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
                var Sq = new Point(GetX, GetY);

                if (Rotation == 0)
                {
                    Sq.X--;
                }
                else if (Rotation == 2)
                {
                    Sq.Y++;
                }
                else if (Rotation == 4)
                {
                    Sq.X++;
                }
                else if (Rotation == 6)
                {
                    Sq.Y--;
                }
                return Sq;
            }
        }

        public IFurniInteractor Interactor
        {
            get
            {
                if (IsWired)
                {
                    return new InteractorWired();
                }

                switch (GetBaseItem().InteractionType)
                {

                    case InteractionType.CRAFTING:
                        return new InteractorCrafting();

                    case InteractionType.FARMING:
                        return new InteractorFarming();

                    case InteractionType.ATM_MACHINE:
                        return new InteractorATM();

                    case InteractionType.RP_NUKE:
                        return new InteractorNuking();

                    case InteractionType.TRASH_CAN:
                        return new InteractorTrashCan();

                    case InteractionType.DELIVERY_BOX:
                        return new InteractorDeliveryBox();

                    case InteractionType.GATE:
                        return new InteractorGate();

                    case InteractionType.TELEPORT:
                        return new InteractorTeleport();

                    case InteractionType.HOPPER:
                        return new InteractorHopper();

                    case InteractionType.BOTTLE:
                        return new InteractorSpinningBottle();

                    case InteractionType.DICE:
                        return new InteractorDice();

                    case InteractionType.HABBO_WHEEL:
                        return new InteractorHabboWheel();

                    case InteractionType.LOVE_SHUFFLER:
                        return new InteractorLoveShuffler();

                    case InteractionType.ONE_WAY_GATE:
                        return new InteractorOneWayGate();

                    case InteractionType.ALERT:
                        return new InteractorAlert();

                    case InteractionType.VENDING_MACHINE:
                        return new InteractorVendor();

                    case InteractionType.SCOREBOARD:
                        return new InteractorScoreboard();

                    case InteractionType.PUZZLE_BOX:
                        return new InteractorPuzzleBox();

                    case InteractionType.MANNEQUIN:
                        return new InteractorMannequin();

                    case InteractionType.banzaicounter:
                        return new InteractorBanzaiTimer();

                    case InteractionType.freezetimer:
                        return new InteractorFreezeTimer();

                    case InteractionType.FREEZE_TILE_BLOCK:
                    case InteractionType.FREEZE_TILE:
                        return new InteractorFreezeTile();

                    case InteractionType.footballcounterblue:
                    case InteractionType.footballcountergreen:
                    case InteractionType.footballcounterred:
                    case InteractionType.footballcounteryellow:
                        return new InteractorScoreCounter();

                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaiscoreyellow:
                        return new InteractorBanzaiScoreCounter();

                    case InteractionType.WF_FLOOR_SWITCH_1:
                    case InteractionType.WF_FLOOR_SWITCH_2:
                        return new InteractorSwitch();

                    case InteractionType.LOVELOCK:
                        return new InteractorLoveLock();

                    case InteractionType.CANNON:
                        return new InteractorCannon();

                    case InteractionType.COUNTER:
                        return new InteractorCounter();

                    case InteractionType.NONE:
                    default:
                        return new InteractorGenericSwitch();
                }
            }
        }

        public bool IsWired
        {
            get
            {
                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.WIRED_EFFECT:
                    case InteractionType.WIRED_TRIGGER:
                    case InteractionType.WIRED_CONDITION:
                        return true;
                }

                return false;
            }
        }

        public void SetState(int pX, int pY, Double pZ, Dictionary<int, ThreeDCoord> Tiles)
        {
            GetX = pX;
            GetY = pY;
            if (!double.IsInfinity(pZ))
            {
                _coordZ = pZ;
            }
            _affectedPoints = Tiles.Values.Select(x => new Point(x.X, x.Y)).ToList();
        }

        public void Destroy()
        {
            _affectedPoints.Clear();
        }

        public void ProcessUpdates()
        {
            if (this == null)
                return;

            try
            {
                UpdateCounter--;

                if (UpdateCounter <= 0)
                {
                    UpdateNeeded = false;
                    UpdateCounter = 0;

                    RoomUser User = null;
                    RoomUser User2 = null;

                    #region Treadmill (Roleplay)
                    if (GetBaseItem().ItemName.ToLower() == "olympics_c16_treadmill" || GetBaseItem().ItemName.ToLower() == "olympics_c16_crosstrainer")
                    {
                        if (GetRoom().GetGameMap().GetRoomUsers(Coordinate) == null || GetRoom().GetGameMap().GetRoomUsers(Coordinate).Count <= 0)
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                            InteractingUser = 0;
                        }
                        else
                            RequestUpdate(1, false);
                        return;
                    }
                    #endregion

                    switch (GetBaseItem().InteractionType)
                    {
                        #region Roleplay
                        #region Shower
                        case InteractionType.SHOWER:
                            {
                                if (ExtraData == "1")
                                {
                                    bool HasUsers = false;
                                    foreach (var square in Gamemap.GetAffectedTiles(this.GetBaseItem().Length, this.GetBaseItem().Width, this.GetX, this.GetY, this.Rotation).Values)
                                    {
                                        if (GetRoom().GetRoomUserManager().GetUserForSquare(square.X, square.Y) != null)
                                            HasUsers = true;
                                    }

                                    if (!HasUsers)
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                        InteractingUser = 0;
                                    }
                                    else
                                    {
                                        RequestUpdate(1, false);
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Trash Can
                        case InteractionType.TRASH_CAN:
                            {
                                if (ExtraData == "1")
                                {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                }
                                break;
                            }
                        #endregion
                        #endregion

                        #region Group Gates
                        case InteractionType.SLIDING_DOORS:
                        case InteractionType.GUILD_GATE:
                            {
                                if (ExtraData == "1")
                                {
                                    bool HasUsers = false;
                                    foreach (var square in Gamemap.GetAffectedTiles(this.GetBaseItem().Length, this.GetBaseItem().Width, this.GetX, this.GetY, this.Rotation).Values)
                                    {
                                        if (GetRoom().GetRoomUserManager().GetUserForSquare(square.X, square.Y) != null)
                                            HasUsers = true;
                                    }

                                    if (!HasUsers)
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                    else
                                    {
                                        RequestUpdate(2, false);
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region Item Effects
                        case InteractionType.EFFECT:
                        case InteractionType.BEDEFFECT:
                            {
                                if (ExtraData == "1")
                                {
                                    if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null)
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                    else
                                    {
                                        RequestUpdate(2, false);
                                    }
                                }
                                break;
                            }
                        #endregion

                        #region One Way Gates
                        case InteractionType.ONE_WAY_GATE:

                            User = null;

                            if (InteractingUser > 0)
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            if (User != null && User.X == GetX && User.Y == GetY)
                            {
                                ExtraData = "1";

                                User.MoveTo(SquareBehind);
                                User.InteractingGate = false;
                                User.GateId = 0;
                                RequestUpdate(1, false);
                                UpdateState(false, true);
                            }
                            else if (User != null && User.Coordinate == SquareBehind)
                            {
                                User.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                User.InteractingGate = false;
                                User.GateId = 0;
                                UpdateState(false, true);
                            }
                            else if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (User == null)
                            {
                                InteractingUser = 0;
                            }

                            break;
                        #endregion

                        #region VIP Gate
                        case InteractionType.GATE_VIP:

                            User = null;


                            if (InteractingUser > 0)
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            }

                            int NewY = 0;
                            int NewX = 0;

                            if (User != null && User.X == GetX && User.Y == GetY)
                            {
                                if (User.RotBody == 4)
                                {
                                    NewY = 1;
                                }
                                else if (User.RotBody == 0)
                                {
                                    NewY = -1;
                                }
                                else if (User.RotBody == 6)
                                {
                                    NewX = -1;
                                }
                                else if (User.RotBody == 2)
                                {
                                    NewX = 1;
                                }


                                User.MoveTo(User.X + NewX, User.Y + NewY);
                                RequestUpdate(1, false);
                            }
                            else if (User != null && (User.Coordinate == SquareBehind || User.Coordinate == SquareInFront))
                            {
                                User.UnlockWalking();

                                ExtraData = "0";
                                InteractingUser = 0;
                                UpdateState(false, true);
                            }
                            else if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }

                            if (User == null)
                            {
                                InteractingUser = 0;
                            }

                            break;
                        #endregion

                        #region Hopper
                        case InteractionType.HOPPER:
                            {
                                User = null;
                                User2 = null;
                                bool showHopperEffect = false;
                                bool keepDoorOpen = false;
                                int Pause = 0;


                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0)
                                {
                                    User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (User != null)
                                    {
                                        // Is he in the tele?
                                        if (User.Coordinate == Coordinate)
                                        {
                                            //Remove the user from the square
                                            User.AllowOverride = false;
                                            if (User.TeleDelay == 0)
                                            {
                                                int RoomHopId = ItemHopperFinder.GetAHopper(User.RoomId);
                                                int NextHopperId = ItemHopperFinder.GetHopperId(RoomHopId);

                                                if (!User.IsBot && User != null && User.GetClient() != null &&
                                                    User.GetClient().GetHabbo() != null)
                                                {
                                                    User.GetClient().GetHabbo().IsHopping = true;
                                                    User.GetClient().GetHabbo().HopperId = NextHopperId;
                                                    User.GetClient().GetHabbo().PrepareRoom(RoomHopId, "");
                                                    //User.GetClient().SendMessage(new RoomForwardComposer(RoomHopId));
                                                    InteractingUser = 0;
                                                }
                                            }
                                            else
                                            {
                                                User.TeleDelay--;
                                                showHopperEffect = true;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (User.Coordinate == SquareInFront)
                                        {
                                            User.AllowOverride = true;
                                            keepDoorOpen = true;

                                            // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                            if (User.IsWalking && (User.GoalX != GetX || User.GoalY != GetY))
                                            {
                                                User.ClearMovement(true);
                                            }

                                            User.CanWalk = false;
                                            User.AllowOverride = true;

                                            // Move into the tele
                                            User.MoveTo(Coordinate.X, Coordinate.Y, true);
                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else
                                        {
                                            InteractingUser = 0;
                                        }
                                    }
                                    else
                                    {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                if (InteractingUser2 > 0)
                                {
                                    User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (User2 != null)
                                    {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        User2.UnlockWalking();
                                        User2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (keepDoorOpen)
                                {
                                    if (ExtraData != "1")
                                    {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                }
                                else if (showHopperEffect)
                                {
                                    if (ExtraData != "2")
                                    {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                }
                                else
                                {
                                    if (ExtraData != "0")
                                    {
                                        if (Pause == 0)
                                        {
                                            ExtraData = "0";
                                            UpdateState(false, true);
                                            Pause = 2;
                                        }
                                        else
                                        {
                                            Pause--;
                                        }
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }
                        #endregion

                        #region Teleports
                        case InteractionType.TELEPORT:
                            {
                                User = null;
                                User2 = null;

                                bool keepDoorOpen = false;
                                bool showTeleEffect = false;

                                // Do we have a primary user that wants to go somewhere?
                                if (InteractingUser > 0)
                                {
                                    User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                                    // Is this user okay?
                                    if (User != null)
                                    {
                                        // Is he in the tele?
                                        if (User.Coordinate == Coordinate)
                                        {
                                            //Remove the user from the square
                                            User.AllowOverride = false;

                                            if (ItemTeleporterFinder.IsTeleLinked(Id, GetRoom()))
                                            {
                                                showTeleEffect = true;

                                                if (true)
                                                {
                                                    // Woop! No more delay.
                                                    int TeleId = ItemTeleporterFinder.GetLinkedTele(Id, GetRoom());
                                                    int RoomId = ItemTeleporterFinder.GetTeleRoomId(TeleId, GetRoom());

                                                    // Do we need to tele to the same room or gtf to another?
                                                    if (RoomId == this.RoomId)
                                                    {
                                                        Item Item = GetRoom().GetRoomItemHandler().GetItem(TeleId);

                                                        if (Item == null)
                                                        {
                                                            User.UnlockWalking();
                                                        }
                                                        else
                                                        {
                                                            // Set pos
                                                            User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                                            User.SetRot(Item.Rotation, false);

                                                            // Force tele effect update (dirty)
                                                            Item.ExtraData = "2";
                                                            Item.UpdateState(false, true);

                                                            // Set secondary interacting user
                                                            Item.InteractingUser2 = InteractingUser;
                                                            GetRoom().GetGameMap().RemoveUserFromMap(User, new Point(GetX, GetY));

                                                            InteractingUser = 0;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (User.TeleDelay == 0)
                                                        {
                                                            // Let's run the teleport delegate to take futher care of this.. WHY DARIO?!
                                                            if (!User.IsBot && User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
                                                            {
                                                                User.GetClient().GetHabbo().IsTeleporting = true;
                                                                User.GetClient().GetHabbo().TeleportingRoomID = RoomId;
                                                                User.GetClient().GetHabbo().TeleporterId = TeleId;
                                                                User.GetClient().GetHabbo().PrepareRoom(RoomId, "");
                                                                //User.GetClient().SendMessage(new RoomForwardComposer(RoomId));
                                                                InteractingUser = 0;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            User.TeleDelay--;
                                                            showTeleEffect = true;
                                                        }
                                                        //PlusEnvironment.GetGame().GetRoomManager().AddTeleAction(new TeleUserData(User.GetClient().GetMessageHandler(), User.GetClient().GetHabbo(), RoomId, TeleId));
                                                    }
                                                    GetRoom().GetGameMap().GenerateMaps();
                                                    // We're done with this tele. We have another one to bother.
                                                }
                                                else
                                                {
                                                    // We're linked, but there's a delay, so decrease the delay and wait it out.
                                                    //User.TeleDelay--;
                                                }
                                            }
                                            else
                                            {
                                                // This tele is not linked, so let's gtfo.
                                                User.UnlockWalking();
                                                InteractingUser = 0;
                                            }
                                        }
                                        // Is he in front of the tele?
                                        else if (User.Coordinate == SquareInFront)
                                        {
                                            User.AllowOverride = true;
                                            // Open the door
                                            keepDoorOpen = true;

                                            // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                            if (User.IsWalking && (User.GoalX != GetX || User.GoalY != GetY))
                                            {
                                                User.ClearMovement(true);
                                            }

                                            User.CanWalk = false;
                                            User.AllowOverride = true;

                                            // Move into the tele
                                            User.MoveTo(Coordinate.X, Coordinate.Y, true);

                                        }
                                        // Not even near, do nothing and move on for the next user.
                                        else
                                        {
                                            InteractingUser = 0;
                                        }
                                    }
                                    else
                                    {
                                        // Invalid user, do nothing and move on for the next user. 
                                        InteractingUser = 0;
                                    }
                                }

                                // Do we have a secondary user that wants to get out of the tele?
                                if (InteractingUser2 > 0)
                                {
                                    User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                                    // Is this user okay?
                                    if (User2 != null)
                                    {
                                        // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                        keepDoorOpen = true;
                                        User2.UnlockWalking();
                                        User2.MoveTo(SquareInFront);
                                    }

                                    // This is a one time thing, whether the user's valid or not.
                                    InteractingUser2 = 0;
                                }

                                // Set the new item state, by priority
                                if (showTeleEffect)
                                {
                                    if (ExtraData != "2")
                                    {
                                        ExtraData = "2";
                                        UpdateState(false, true);
                                    }
                                }
                                else if (keepDoorOpen)
                                {
                                    if (ExtraData != "1")
                                    {
                                        ExtraData = "1";
                                        UpdateState(false, true);
                                    }
                                }
                                else
                                {
                                    if (ExtraData != "0")
                                    {
                                        ExtraData = "0";
                                        UpdateState(false, true);
                                    }
                                }

                                // We're constantly going!
                                RequestUpdate(1, false);
                                break;
                            }
                        #endregion

                        #region Bottle
                        case InteractionType.BOTTLE:
                            ExtraData = RandomNumber.GenerateNewRandom(0, 7).ToString();
                            UpdateState();
                            break;
                        #endregion

                        #region Dice
                        case InteractionType.DICE:
                            {
                                string[] numbers = new string[] { "1", "2", "3", "4", "5", "6" };
                                if (ExtraData == "-1")
                                    ExtraData = RandomizeStrings(numbers)[0];

                                #region Texas Hold 'Em Check
                                if (TexasHoldEmData != null)
                                {
                                    TexasHoldEmData.Rolled = true;
                                    TexasHoldEmData.Value = Convert.ToInt32(ExtraData);

                                    TexasHoldEm Game;
                                    int PlayerId = TexasHoldEmManager.GetPlayerByDice(this, out Game);

                                    #region Banker Roll
                                    if (Game != null && PlayerId == 0)
                                    {
                                        UpdateState(false, true);

                                        if (Game.GameSequence < 3)
                                        {
                                            Game.PlayersTurn = 0;
                                            Game.ChangeTurn();
                                        }
                                        else
                                        {
                                            new Thread(() =>
                                            {
                                                lock (Game)
                                                {
                                                    #region Send All Dice to All Players
                                                    foreach (var Item in Game.Player1.Values)
                                                    {
                                                        if (Item.Furni != null)
                                                            Item.Furni.UpdateState(false, true);
                                                    }
                                                    foreach (var Item in Game.Player2.Values)
                                                    {
                                                        if (Item.Furni != null)
                                                            Item.Furni.UpdateState(false, true);
                                                    }
                                                    foreach (var Item in Game.Player3.Values)
                                                    {
                                                        if (Item.Furni != null)
                                                            Item.Furni.UpdateState(false, true);
                                                    }
                                                    #endregion


                                                    Game.SendStartMessage("O vencedor será escolhido em 5 segundos!");
                                                    Thread.Sleep(5000);
                                                    Game.ChooseWinner();
                                                }
                                            }).Start();
                                        }
                                    }
                                    #endregion

                                    #region Player Roll
                                    if (Game != null && PlayerId > 0)
                                    {
                                        if (Game.GameSequence != 0)
                                            break;

                                        GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(PlayerId);

                                        if (Client != null)
                                        {
                                            if (IsFloorItem)
                                                Client.SendMessage(new ObjectUpdateComposer(this, 0));
                                            else
                                                Client.SendMessage(new ItemUpdateComposer(this, 0));
                                        }

                                        if (Game.PlayerList != null && Game.PlayerList.Where(x => x.Value != null && x.Value.UserId == PlayerId).ToList().Count > 0)
                                        {
                                            int Number = Game.PlayerList.FirstOrDefault(x => x.Value != null && x.Value.UserId == PlayerId).Key;

                                            ConcurrentDictionary<int, TexasHoldEmItem> PlayerData;
                                            if (Number == 1)
                                                PlayerData = Game.Player1;
                                            else if (Number == 2)
                                                PlayerData = Game.Player2;
                                            else
                                                PlayerData = Game.Player3;

                                            if (PlayerData.Values.Where(x => x != null && x.Rolled).ToList().Count >= 2)
                                                Game.ChangeTurn();
                                        }
                                    }
                                    #endregion
                                }
                                #endregion
                                else
                                    UpdateState();
                            }
                            break;
                        #endregion

                        #region Habbo Wheel
                        case InteractionType.HABBO_WHEEL:
                            ExtraData = RandomNumber.GenerateRandom(1, 10).ToString();
                            UpdateState();
                            break;
                        #endregion

                        #region Love Shuffler
                        case InteractionType.LOVE_SHUFFLER:

                            if (ExtraData == "0")
                            {
                                ExtraData = RandomNumber.GenerateNewRandom(1, 4).ToString();
                                RequestUpdate(20, false);
                            }
                            else if (ExtraData != "-1")
                            {
                                ExtraData = "-1";
                            }

                            UpdateState(false, true);
                            break;
                        #endregion

                        #region Alert
                        case InteractionType.ALERT:
                            if (ExtraData == "1")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }
                            break;
                        #endregion

                        #region Vending Machine
                        case InteractionType.VENDING_MACHINE:

                            if (ExtraData == "1")
                            {
                                User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                                if (User == null)
                                    break;
                                User.UnlockWalking();
                                if (GetBaseItem().VendingIds.Count > 0)
                                {
                                    int randomDrink = GetBaseItem().VendingIds[RandomNumber.GenerateRandom(0, (GetBaseItem().VendingIds.Count - 1))];
                                    User.CarryItem(randomDrink);
                                }


                                InteractingUser = 0;
                                ExtraData = "0";

                                UpdateState(false, true);
                            }
                            break;
                        #endregion

                        #region Scoreboard
                        case InteractionType.SCOREBOARD:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;


                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;

                                        ExtraData = seconds.ToString();
                                        UpdateState();
                                    }
                                    else
                                        interactionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                    UpdateCounter = 0;

                                break;
                            }
                        #endregion

                        #region Banzai Counter
                        case InteractionType.banzaicounter:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;

                                        if (GetRoom().GetBanzai().isBanzaiActive)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        interactionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateCounter = 0;
                                    GetRoom().GetBanzai().BanzaiEnd();
                                }

                                break;
                            }
                        #endregion

                        #region Banzai Tele
                        case InteractionType.banzaitele:
                            {
                                ExtraData = string.Empty;
                                UpdateState();
                                break;
                            }
                        #endregion

                        #region Banzai Floor
                        case InteractionType.banzaifloor:
                            {
                                if (value == 3)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        interactionCountHelper = 0;

                                        switch (team)
                                        {
                                            case TEAM.BLUE:
                                                {
                                                    ExtraData = "11";
                                                    break;
                                                }

                                            case TEAM.GREEN:
                                                {
                                                    ExtraData = "8";
                                                    break;
                                                }

                                            case TEAM.RED:
                                                {
                                                    ExtraData = "5";
                                                    break;
                                                }

                                            case TEAM.YELLOW:
                                                {
                                                    ExtraData = "14";
                                                    break;
                                                }
                                        }
                                    }
                                    else
                                    {
                                        ExtraData = "";
                                        interactionCountHelper++;
                                    }

                                    UpdateState();

                                    interactionCount++;

                                    if (interactionCount < 16)
                                    {
                                        UpdateCounter = 1;
                                    }
                                    else
                                        UpdateCounter = 0;
                                }
                                break;
                            }
                        #endregion

                        #region Banzai Puck
                        case InteractionType.banzaipuck:
                            {
                                if (interactionCount > 4)
                                {
                                    interactionCount++;
                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    interactionCount = 0;
                                    UpdateCounter = 0;
                                }

                                break;
                            }
                        #endregion

                        #region Freeze Tile
                        case InteractionType.FREEZE_TILE:
                            {
                                if (InteractingUser > 0)
                                {
                                    this.ExtraData = "11000";
                                    this.UpdateState(false, true);
                                    this.GetRoom().GetFreeze().onFreezeTiles(this, this.freezePowerUp);
                                    this.InteractingUser = 0;
                                    this.interactionCountHelper = 0;
                                }
                                break;
                            }
                        #endregion

                        #region Football Counter
                        case InteractionType.COUNTER:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;
                                        if (GetRoom().GetSoccer().GameIsStarted)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        interactionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateNeeded = false;
                                    GetRoom().GetSoccer().StopGame();
                                }

                                break;
                            }
                        #endregion

                        #region Freeze Timer
                        case InteractionType.freezetimer:
                            {
                                if (string.IsNullOrEmpty(ExtraData))
                                    break;

                                int seconds = 0;

                                try
                                {
                                    seconds = int.Parse(ExtraData);
                                }
                                catch
                                {
                                }

                                if (seconds > 0)
                                {
                                    if (interactionCountHelper == 1)
                                    {
                                        seconds--;
                                        interactionCountHelper = 0;
                                        if (GetRoom().GetFreeze().GameIsStarted)
                                        {
                                            ExtraData = seconds.ToString();
                                            UpdateState();
                                        }
                                        else
                                            break;
                                    }
                                    else
                                        interactionCountHelper++;

                                    UpdateCounter = 1;
                                }
                                else
                                {
                                    UpdateNeeded = false;
                                    GetRoom().GetFreeze().StopGame();
                                }

                                break;
                            }
                        #endregion

                        #region Pressure Pad
                        case InteractionType.PRESSURE_PAD:
                            {
                                ExtraData = "1";
                                UpdateState();
                                break;
                            }
                        #endregion

                        #region Wired
                        case InteractionType.WIRED_EFFECT:
                        case InteractionType.WIRED_TRIGGER:
                        case InteractionType.WIRED_CONDITION:
                            {
                                if (ExtraData == "1")
                                {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                }
                            }
                            break;
                        #endregion

                        #region Cannon
                        case InteractionType.CANNON:
                            {
                                if (ExtraData != "1")
                                    break;

                                #region Target Calculation
                                Point TargetStart = Coordinate;
                                List<Point> TargetSquares = new List<Point>();
                                switch (Rotation)
                                {
                                    case 0:
                                        {
                                            TargetStart = new Point(GetX - 1, GetY);

                                            if (!TargetSquares.Contains(TargetStart))
                                                TargetSquares.Add(TargetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point TargetSquare = new Point(TargetStart.X - I, TargetStart.Y);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                    TargetSquares.Add(TargetSquare);
                                            }

                                            break;
                                        }

                                    case 2:
                                        {
                                            TargetStart = new Point(GetX, GetY - 1);

                                            if (!TargetSquares.Contains(TargetStart))
                                                TargetSquares.Add(TargetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point TargetSquare = new Point(TargetStart.X, TargetStart.Y - I);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                    TargetSquares.Add(TargetSquare);
                                            }

                                            break;
                                        }

                                    case 4:
                                        {
                                            TargetStart = new Point(GetX + 2, GetY);

                                            if (!TargetSquares.Contains(TargetStart))
                                                TargetSquares.Add(TargetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point TargetSquare = new Point(TargetStart.X + I, TargetStart.Y);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                    TargetSquares.Add(TargetSquare);
                                            }

                                            break;
                                        }

                                    case 6:
                                        {
                                            TargetStart = new Point(GetX, GetY + 2);


                                            if (!TargetSquares.Contains(TargetStart))
                                                TargetSquares.Add(TargetStart);

                                            for (int I = 1; I <= 3; I++)
                                            {
                                                Point TargetSquare = new Point(TargetStart.X, TargetStart.Y + I);

                                                if (!TargetSquares.Contains(TargetSquare))
                                                    TargetSquares.Add(TargetSquare);
                                            }

                                            break;
                                        }
                                }
                                #endregion

                                if (TargetSquares.Count > 0)
                                {
                                    foreach (Point Square in TargetSquares.ToList())
                                    {
                                        List<RoomUser> affectedUsers = _room.GetGameMap().GetRoomUsers(Square).ToList();

                                        if (affectedUsers == null || affectedUsers.Count == 0)
                                            continue;

                                        foreach (RoomUser Target in affectedUsers)
                                        {
                                            if (Target == null || Target.IsBot || Target.IsPet)
                                                continue;

                                            if (Target.GetClient() == null || Target.GetClient().GetHabbo() == null)
                                                continue;

                                            if (_room.CheckRights(Target.GetClient(), true))
                                                continue;

                                            Target.ApplyEffect(EffectsList.Twinkle);
                                            Target.GetClient().SendMessage(new RoomNotificationComposer("Expulso do quarto", "Você foi atingido por uma bola de canhão!", "room_kick_cannonball", "insideroomid"));
                                            Target.ApplyEffect(0);
                                            _room.GetRoomUserManager().RemoveUserFromRoom(Target.GetClient(), true);
                                        }
                                    }
                                }

                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                            break;
                            #endregion
                    }
                }
            }
            catch (Exception e) { Logging.LogException(e.ToString()); }
        }

        public static string[] RandomizeStrings(string[] arr)
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            // Add all strings from array
            // Add new random int each time
            foreach (string s in arr)
            {
                list.Add(new KeyValuePair<int, string>(_random.Next(), s));
            }
            // Sort the list by the random number
            var sorted = from item in list
                         orderby item.Key
                         select item;
            // Allocate new string array
            string[] result = new string[arr.Length];
            // Copy values to array
            int index = 0;
            foreach (KeyValuePair<int, string> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }
            // Return copied array
            return result;
        }

        public void RequestUpdate(int Cycles, bool setUpdate)
        {
            UpdateCounter = Cycles;
            if (setUpdate)
                UpdateNeeded = true;
        }

        public void UpdateState()
        {
            UpdateState(true, true);
        }

        public void UpdateState(bool inDb, bool inRoom)
        {
            if (GetRoom() == null)
                return;

            if (inDb)
                GetRoom().GetRoomItemHandler().UpdateItem(this);

            if (inRoom)
            {
                if (IsFloorItem)
                    GetRoom().SendMessage(new ObjectUpdateComposer(this, this.UserID));
                else
                    GetRoom().SendMessage(new ItemUpdateComposer(this, this.UserID));
            }
        }

        public void ResetBaseItem()
        {
            this._data = null;
            this._data = this.GetBaseItem();
        }

        public ItemData GetBaseItem()
        {
            if (this._data == null)
            {
                ItemData I = null;
                if (PlusEnvironment.GetGame().GetItemManager().GetItem(this.BaseItem, out I))
                    this._data = I;
            }

            return this._data;
        }

        public Room GetRoom()
        {
            if (this._room != null)
                return this._room;

            Room Room;
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room))
                return Room;

            return null;
        }

        public void UserFurniCollision(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            GetRoom().GetWired().TriggerEvent(Wired.WiredBoxType.TriggerUserFurniCollision, user.GetClient().GetHabbo(), this);
        }

        public void UserWalksOnFurni(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.TENT || GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
            {
                GetRoom().AddUserToTent(Id, user, this);
            }

            GetRoom().GetWired().TriggerEvent(Wired.WiredBoxType.TriggerWalkOnFurni, user.GetClient().GetHabbo(), this);
            user.LastItem = this;
        }

        public void UserWalksOffFurni(RoomUser user)
        {
            if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.TENT || GetBaseItem().InteractionType == InteractionType.TENT_SMALL)
                GetRoom().RemoveUserFromTent(Id, user, this);

            GetRoom().GetWired().TriggerEvent(Wired.WiredBoxType.TriggerWalkOffFurni, user.GetClient().GetHabbo(), this);
        }

        public List<Point> GetBedTiles(Point Point, out Point Square)
        {
            Square = this.Coordinate;

            try
            {
                if (this.GetRoom() == null || this.GetRoom().GetGameMap() == null)
                    return new List<Point>();

                if (this.GetAffectedTiles.Count < 6)
                    return this.GetAffectedTiles;

                List<Point> TilesLeft = new List<Point>();
                List<Point> TilesRight = new List<Point>();

                if (this.Rotation == 0)
                {
                    TilesLeft = this.GetAffectedTiles.Where(x => x.X == this.GetX).ToList();
                    TilesRight = this.GetAffectedTiles.Where(x => x.X == (this.GetX + 1)).ToList();
                }

                if (this.Rotation == 2)
                {
                    TilesLeft = this.GetAffectedTiles.Where(x => x.Y == this.GetY).ToList();
                    TilesRight = this.GetAffectedTiles.Where(x => x.Y == (this.GetY + 1)).ToList();
                }

                if (TilesLeft.Contains(Point))
                    return TilesLeft;

                if (TilesRight.Contains(Point))
                {
                    if (this.Rotation == 0)
                        Square = new Point(this.GetX + 1, this.GetY);

                    if (this.Rotation == 2)
                        Square = new Point(this.GetX, this.GetY + 1);

                    return TilesRight;
                }

                return new List<Point>();
            }
            catch
            {
                return new List<Point>();
            }
        }
    }
}