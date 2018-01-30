using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Plus.Core;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms.Games.Teams;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using System.Diagnostics;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms
{
    public class Gamemap
    {
        private Room _room;
        private byte[,] mGameMap;//0 = none, 1 = pool, 2 = normal skates, 3 = ice skates

        public bool gotPublicPool;
        public bool DiagonalEnabled;
        private RoomModel mStaticModel;
        private byte[,] mUserItemEffect;
        private double[,] mItemHeightMap;
        private DynamicRoomModel mDynamicModel;
        private ConcurrentDictionary<Point, List<int>> mCoordinatedItems;
        private ConcurrentDictionary<Point, List<RoomUser>> userMap;

        public Gamemap(Room room)
        {
            this._room = room;
            this.DiagonalEnabled = true;

            mStaticModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(room.ModelName);
            if (mStaticModel == null)
            {
                PlusEnvironment.GetGame().GetRoomManager().LoadModel(room.ModelName);
                mStaticModel = PlusEnvironment.GetGame().GetRoomManager().GetModel(room.ModelName);
            }

            if (mStaticModel == null)
                return;

            mDynamicModel = new DynamicRoomModel(mStaticModel);

            mCoordinatedItems = new ConcurrentDictionary<Point, List<int>>();


            gotPublicPool = room.RoomData.Model.gotPublicPool;
            mGameMap = new byte[Model.MapSizeX, Model.MapSizeY];
            mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

            userMap = new ConcurrentDictionary<Point, List<RoomUser>>();
        }

        public void AddUserToMap(RoomUser user, Point coord)
        {
            if (userMap.ContainsKey(coord))
            {
                ((List<RoomUser>)userMap[coord]).Add(user);
            }
            else
            {
                List<RoomUser> users = new List<RoomUser>();
                users.Add(user);
                userMap.TryAdd(coord, users);
            }
        }

        public void TeleportToItem(RoomUser user, Item item)
        {
            if (item == null || user == null)
                return;

            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(item.Coordinate.X, item.Coordinate.Y), user);
            user.X = item.GetX;
            user.Y = item.GetY;
            user.Z = item.GetZ;

            user.SqState = GameMap[item.GetX, item.GetY];
            GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rotation;
            user.RotHead = item.Rotation;

            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }

        public void TeleportToSquare(RoomUser user, Point point)
        {
            if (user == null)
                return;

            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), point, user);
            user.X = point.X;
            user.Y = point.Y;
            user.Z = this.GetHeightForSquare(point);

            user.SqState = GameMap[point.X, point.Y];
            GameMap[user.X, user.Y] = 1;

            Item Item;
            if (this.GetHighestItemForSquare(point, out Item))
            {
                user.RotBody = Item.Rotation;
                user.RotHead = Item.Rotation;
            }

            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }


        public void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord, true);
            AddUserToMap(user, newCoord);
        }

        public void RemoveUserFromMap(RoomUser user, Point coord, bool UpdatingMovement = false)
        {
            if (userMap != null)
            {
                if (userMap.ContainsKey(coord))
                    ((List<RoomUser>)userMap[coord]).RemoveAll(x => x != null && x.VirtualId == user.VirtualId);
            }
        }

        public bool MapGotUser(Point coord, bool CheckingInvisible, bool IsInvisible)
        {
            List<RoomUser> List = GetRoomUsers(coord).Where(RoomUser => !RoomUser.IsBot).ToList();
            
            if (List == null)
                return false;

            #region Invisible
            if (CheckingInvisible)
            {
                List<RoomUser> CheckingUsers = new List<RoomUser>();
                lock (List)
                {
                    foreach (var user in List)
                    {
                        if (user == null)
                            continue;

                        if (user.IsBot)
                        {
                            if (user.GetBotRoleplay() == null)
                                continue;

                            if (!user.GetBotRoleplay().Invisible)
                                CheckingUsers.Add(user);
                        }
                        else
                        {
                            if (user.GetClient() == null)
                                continue;

                            if (user.GetClient().GetRoleplay() == null)
                                continue;

                            if (!user.GetClient().GetRoleplay().Invisible)
                                CheckingUsers.Add(user);

                            if (user.GetClient().GetRoleplay().Invisible && IsInvisible)
                                CheckingUsers.Add(user);
                        }
                    }
                    return (CheckingUsers.Count > 0);
                }
            }
            #endregion

            return (List.Count > 0);
        }

        public List<RoomUser> GetRoomUsers(Point coord)
        {
            if (userMap == null)
                return new List<RoomUser>();

            if (userMap.ContainsKey(coord))
                return (List<RoomUser>)userMap[coord];
            else
                return new List<RoomUser>();
        }

        public Point getRandomWalkableSquare(bool CheckUsers = false)
        {
            var walkableSquares = new List<Point>();
            for (int y = 0; y < mGameMap.GetUpperBound(1); y++)
            {
                for (int x = 0; x < mGameMap.GetUpperBound(0); x++)
                {
                    if (/*mStaticModel.DoorX != x && mStaticModel.DoorY != y &&*/ mGameMap[x, y] == 1)
                        walkableSquares.Add(new Point(x, y));
                }
            }

            int RandomNumber = new CryptoRandom().Next(0, walkableSquares.Count);
            int i = 0;

            foreach (Point coord in walkableSquares.ToList())
            {

                if (CheckUsers)
                {
                    if (this.SquareHasUsers(coord.X, coord.Y))
                        continue;
                }

                if (i == RandomNumber)
                {
                    return coord;
                }
                i++;

            }

            return new Point(0, 0);
        }


        public bool isInMap(int X, int Y)
        {
            var walkableSquares = new List<Point>();
            for (int y = 0; y < mGameMap.GetUpperBound(1); y++)
            {
                for (int x = 0; x < mGameMap.GetUpperBound(0); x++)
                {
                    if (mGameMap[x, y] >= 1)
                        walkableSquares.Add(new Point(x, y));
                }
            }

            if (walkableSquares.Contains(new Point(X, Y)))
                return true;
            return false;
        }

        public void AddToMap(Item item)
        {
            AddItemToMap(item);
        }

        private void SetDefaultValue(int x, int y)
        {
            mGameMap[x, y] = 0;
            mUserItemEffect[x, y] = 0;
            mItemHeightMap[x, y] = 0.0;

            if (x == Model.DoorX && y == Model.DoorY)
            {
                mGameMap[x, y] = 1/*3*/;
            }
            else if (Model.SqState[x, y] == SquareState.OPEN)
            {
                mGameMap[x, y] = 1;
            }
            else if (Model.SqState[x, y] == SquareState.SEAT)
            {
                mGameMap[x, y] = 2;
            }
        }

        public void updateMapForItem(Item item)
        {
            RemoveFromMap(item);
            AddToMap(item);
        }

        public void GenerateMaps(bool checkLines = true)
        {
            int MaxX = 0;
            int MaxY = 0;
            mCoordinatedItems = new ConcurrentDictionary<Point, List<int>>();

            if (checkLines)
            {
                Item[] items = _room.GetRoomItemHandler().GetFloor.ToArray();
                foreach (Item item in items.ToList())
                {
                    if (item == null)
                        continue;

                    if (item.GetX > Model.MapSizeX && item.GetX > MaxX)
                        MaxX = item.GetX;
                    if (item.GetY > Model.MapSizeY && item.GetY > MaxY)
                        MaxY = item.GetY;
                }

                Array.Clear(items, 0, items.Length);
                items = null;
            }

            #region Dynamic game map handling

            if (MaxY > (Model.MapSizeY - 1) || MaxX > (Model.MapSizeX - 1))
            {
                if (MaxX < Model.MapSizeX)
                    MaxX = Model.MapSizeX;
                if (MaxY < Model.MapSizeY)
                    MaxY = Model.MapSizeY;

                Model.SetMapsize(MaxX + 7, MaxY + 7);
                GenerateMaps(false);
                return;
            }

            if (MaxX != StaticModel.MapSizeX || MaxY != StaticModel.MapSizeY)
            {
                mUserItemEffect = new byte[Model.MapSizeX, Model.MapSizeY];
                mGameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                //if (modelRemap)
                //    Model.Generate(); //Clears model

                for (int line = 0; line < Model.MapSizeY; line++)
                {
                    for (int chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        mGameMap[chr, line] = 0;
                        mUserItemEffect[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            mGameMap[chr, line] = 1/*3*/;
                        }
                        else if (Model.SqState[chr, line] == SquareState.OPEN)
                        {
                            mGameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.SEAT)
                        {
                            mGameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.POOL)
                        {
                            mUserItemEffect[chr, line] = 6;
                        }
                    }
                }

                if (gotPublicPool)
                {
                    for (int y = 0; y < StaticModel.MapSizeY; y++)
                    {
                        for (int x = 0; x < StaticModel.MapSizeX; x++)
                        {
                            if (StaticModel.mRoomModelfx[x, y] != 0)
                            {
                                mUserItemEffect[x, y] = StaticModel.mRoomModelfx[x, y];
                            }
                        }
                    }
                }

                /** COMENTADO YA QUE SALAS PUBLICAS NUEVA CRYPTO NO NECESARIO
                if (!string.IsNullOrEmpty(StaticModel.StaticFurniMap)) 
                {
                     * foreach (PublicRoomSquare square in StaticModel.Furnis)
                    {
                        if (square.Content.Contains("chair") || square.Content.Contains("sofa"))
                        {
                            mGameMap[square.X, square.Y] = 1;
                        } else {
                            mGameMap[square.X, square.Y] = 0;
                        }
                    }
                }*/
            }
            #endregion

            #region Static game map handling

            else
            {
                //mGameMap
                //mUserItemEffect
                mUserItemEffect = new byte[Model.MapSizeX, Model.MapSizeY];
                mGameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                //if (modelRemap)
                //    Model.Generate(); //Clears model

                for (int line = 0; line < Model.MapSizeY; line++)
                {
                    for (int chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        mGameMap[chr, line] = 0;
                        mUserItemEffect[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                        {
                            mGameMap[chr, line] = 1/*3*/;
                        }
                        else if (Model.SqState[chr, line] == SquareState.OPEN)
                        {
                            mGameMap[chr, line] = 1;
                        }
                        else if (Model.SqState[chr, line] == SquareState.SEAT)
                        {
                            mGameMap[chr, line] = 2;
                        }
                        else if (Model.SqState[chr, line] == SquareState.POOL)
                        {
                            mUserItemEffect[chr, line] = 6;
                        }
                    }
                }

                if (gotPublicPool)
                {
                    for (int y = 0; y < StaticModel.MapSizeY; y++)
                    {
                        for (int x = 0; x < StaticModel.MapSizeX; x++)
                        {
                            if (StaticModel.mRoomModelfx[x, y] != 0)
                            {
                                mUserItemEffect[x, y] = StaticModel.mRoomModelfx[x, y];
                            }
                        }
                    }
                }

                /** COMENTADO YA QUE SALAS PUBLICAS NUEVA CRYPTO NO NECESARIO
                 * foreach (PublicRoomSquare square in StaticModel.Furnis)
                {
                    if (square.Content.Contains("chair") || square.Content.Contains("sofa"))
                    {
                        mGameMap[square.X, square.Y] = 1;
                    }
                    else
                    {
                        mGameMap[square.X, square.Y] = 0;
                    }
                }*/
            }

            #endregion

            Item[] tmpItems = _room.GetRoomItemHandler().GetFloor.ToArray();
            foreach (Item Item in tmpItems.ToList())
            {
                if (Item == null)
                    continue;

                if (!AddItemToMap(Item))
                    continue;
            }
            Array.Clear(tmpItems, 0, tmpItems.Length);
            tmpItems = null;

            if (_room.RoomBlockingEnabled == 0)
            {
                foreach (RoomUser user in _room.GetRoomUserManager().GetUserList().ToList())
                {
                    if (user == null)
                        continue;

                    user.SqState = mGameMap[user.X, user.Y];
                    mGameMap[user.X, user.Y] = 0;
                }
            }

            try
            {
                mGameMap[Model.DoorX, Model.DoorY] = 1/*3*/;
            }
            catch { }
        }

        private bool ConstructMapForItem(Item Item, Point Coord)
        {
            try
            {
                if (Coord.X > (Model.MapSizeX - 1))
                {
                    Model.AddX();
                    GenerateMaps();
                    return false;
                }

                if (Coord.Y > (Model.MapSizeY - 1))
                {
                    Model.AddY();
                    GenerateMaps();
                    return false;
                }

                if (Model.SqState[Coord.X, Coord.Y] == SquareState.BLOCKED)
                {
                    Model.OpenSquare(Coord.X, Coord.Y, Item.GetZ);
                }
                if (mItemHeightMap[Coord.X, Coord.Y] <= Item.TotalHeight)
                {
                    mItemHeightMap[Coord.X, Coord.Y] = Item.TotalHeight - mDynamicModel.SqFloorHeight[Item.GetX, Item.GetY];
                    mUserItemEffect[Coord.X, Coord.Y] = 0;


                    switch (Item.GetBaseItem().InteractionType)
                    {
                        case InteractionType.POOL:
                            mUserItemEffect[Coord.X, Coord.Y] = 1;
                            break;
                        case InteractionType.NORMAL_SKATES:
                            mUserItemEffect[Coord.X, Coord.Y] = 2;
                            break;
                        case InteractionType.ICE_SKATES:
                            mUserItemEffect[Coord.X, Coord.Y] = 3;
                            break;
                        case InteractionType.lowpool:
                            mUserItemEffect[Coord.X, Coord.Y] = 4;
                            break;
                        case InteractionType.haloweenpool:
                            mUserItemEffect[Coord.X, Coord.Y] = 5;
                            break;
                    }


                    //SwimHalloween
                    if (Item.GetBaseItem().Walkable)    // If this item is walkable and on the floor, allow users to walk here.
                    {
                        if (mGameMap[Coord.X, Coord.Y] != 3)
                            mGameMap[Coord.X, Coord.Y] = 1;
                    }
                    else if (Item.GetZ <= (Model.SqFloorHeight[Item.GetX, Item.GetY] + 0.1) && Item.GetBaseItem().InteractionType == InteractionType.GATE && Item.ExtraData == "1")// If this item is a gate, open, and on the floor, allow users to walk here.
                    {
                        if (mGameMap[Coord.X, Coord.Y] != 3)
                            mGameMap[Coord.X, Coord.Y] = 1;
                    }
                    else if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().IsBed())
                        mGameMap[Coord.X, Coord.Y] = 3;
                    else // Finally, if it's none of those, block the square.
                    {
                        if (mGameMap[Coord.X, Coord.Y] != 3)
                            mGameMap[Coord.X, Coord.Y] = 0;
                    }
                }

                // Set bad maps
                if (Item.GetBaseItem().IsBed())
                    mGameMap[Coord.X, Coord.Y] = 3;
            }
            catch (Exception e)
            {
                Logging.LogException("Erro durante a geração do mapa para o quarto " + _room.RoomId + ". Exceção: " + e);
            }
            return true;
        }

        public void AddCoordinatedItem(Item item, Point coord)
        {
            List<int> Items = new List<int>(); //mCoordinatedItems[CoordForItem];

            if (!mCoordinatedItems.TryGetValue(coord, out Items))
            {
                Items = new List<int>();

                if (!Items.Contains(item.Id))
                    Items.Add(item.Id);

                if (!mCoordinatedItems.ContainsKey(coord))
                    mCoordinatedItems.TryAdd(coord, Items);
            }
            else
            {
                if (!Items.Contains(item.Id))
                {
                    Items.Add(item.Id);
                    mCoordinatedItems[coord] = Items;
                }
            }
        }

        public List<Item> GetCoordinatedItems(Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            List<Item> Items = new List<Item>();

            lock (mCoordinatedItems)
            {
                if (mCoordinatedItems.ContainsKey(point))
                {
                    List<int> Ids = mCoordinatedItems[point];
                    Items = GetItemsFromIds(Ids);
                    return Items;
                }
            }
            return new List<Item>();
        }

        public bool RemoveCoordinatedItem(Item item, Point coord)
        {
            Point point = new Point(coord.X, coord.Y);
            if (mCoordinatedItems != null && mCoordinatedItems.ContainsKey(point))
            {
                ((List<int>)mCoordinatedItems[point]).RemoveAll(x => x == item.Id);
                return true;
            }
            return false;
        }

        private void AddSpecialItems(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FOOTBALL_GATE:
                    //IsTrans = true;
                    _room.GetSoccer().RegisterGate(item);


                    string[] splittedExtraData = item.ExtraData.Split(':');

                    if (string.IsNullOrEmpty(item.ExtraData) || splittedExtraData.Length <= 1)
                    {
                        item.Gender = "M";
                        switch (item.team)
                        {
                            case TEAM.YELLOW:
                                item.Figure = "lg-275-93.hr-115-61.hd-207-14.ch-265-93.sh-305-62";
                                break;
                            case TEAM.RED:
                                item.Figure = "lg-275-96.hr-115-61.hd-180-3.ch-265-96.sh-305-62";
                                break;
                            case TEAM.GREEN:
                                item.Figure = "lg-275-102.hr-115-61.hd-180-3.ch-265-102.sh-305-62";
                                break;
                            case TEAM.BLUE:
                                item.Figure = "lg-275-108.hr-115-61.hd-180-3.ch-265-108.sh-305-62";
                                break;
                        }
                    }
                    else
                    {
                        item.Gender = splittedExtraData[0];
                        item.Figure = splittedExtraData[1];
                    }
                    break;

                case InteractionType.banzaifloor:
                    {
                        _room.GetBanzai().AddTile(item, item.Id);
                        break;
                    }

                case InteractionType.banzaipyramid:
                    {
                        _room.GetGameItemHandler().AddPyramid(item, item.Id);
                        break;
                    }

                case InteractionType.banzaitele:
                    {
                        _room.GetGameItemHandler().AddTeleport(item, item.Id);
                        item.ExtraData = "";
                        break;
                    }
                case InteractionType.banzaipuck:
                    {
                        _room.GetBanzai().AddPuck(item);
                        break;
                    }

                case InteractionType.FOOTBALL:
                    {
                        _room.GetSoccer().AddBall(item);
                        break;
                    }
                case InteractionType.FREEZE_TILE_BLOCK:
                    {
                        _room.GetFreeze().AddFreezeBlock(item);
                        break;
                    }
                case InteractionType.FREEZE_TILE:
                    {
                        _room.GetFreeze().AddFreezeTile(item);
                        break;
                    }
                case InteractionType.freezeexit:
                    {
                        _room.GetFreeze().AddExitTile(item);
                        break;
                    }
            }
        }

        private void RemoveSpecialItem(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FOOTBALL_GATE:
                    _room.GetSoccer().UnRegisterGate(item);
                    break;
                case InteractionType.banzaifloor:
                    _room.GetBanzai().RemoveTile(item.Id);
                    break;
                case InteractionType.banzaipuck:
                    _room.GetBanzai().RemovePuck(item.Id);
                    break;
                case InteractionType.banzaipyramid:
                    _room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.banzaitele:
                    _room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.FOOTBALL:
                    _room.GetSoccer().RemoveBall(item.Id);
                    break;
                case InteractionType.FREEZE_TILE:
                    _room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                case InteractionType.FREEZE_TILE_BLOCK:
                    _room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;
                case InteractionType.freezeexit:
                    _room.GetFreeze().RemoveExitTile(item.Id);
                    break;
            }
        }

        public bool RemoveFromMap(Item item, bool handleGameItem)
        {
            if (handleGameItem)
                RemoveSpecialItem(item);

            if (_room.GotSoccer())
                _room.GetSoccer().onGateRemove(item);

            bool isRemoved = false;
            foreach (Point coord in item.GetCoords.ToList())
            {
                if (RemoveCoordinatedItem(item, coord))
                    isRemoved = true;
            }

            ConcurrentDictionary<Point, List<Item>> items = new ConcurrentDictionary<Point, List<Item>>();
            foreach (Point Tile in item.GetCoords.ToList())
            {
                lock (mCoordinatedItems)
                {
                    Point point = new Point(Tile.X, Tile.Y);
                    if (mCoordinatedItems.ContainsKey(point))
                    {
                        List<int> Ids = (List<int>)mCoordinatedItems[point];
                        List<Item> __items = GetItemsFromIds(Ids);

                        if (!items.ContainsKey(Tile))
                            items.TryAdd(Tile, __items);
                    }
                    SetDefaultValue(Tile.X, Tile.Y);
                }
            }

            foreach (Point Coord in items.Keys.ToList())
            {
                if (!items.ContainsKey(Coord))
                    continue;

                List<Item> SubItems = (List<Item>)items[Coord];
                foreach (Item Item in SubItems.ToList())
                {
                    ConstructMapForItem(Item, Coord);
                }
            }


            items.Clear();
            items = null;


            return isRemoved;
        }

        public bool RemoveFromMap(Item item)
        {
            return RemoveFromMap(item, true);
        }

        public bool AddItemToMap(Item Item, bool handleGameItem, bool NewItem = true)
        {

            if (handleGameItem)
            {
                AddSpecialItems(Item);

                switch (Item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FOOTBALL_GOAL_RED:
                    case InteractionType.footballcounterred:
                    case InteractionType.banzaiscorered:
                    case InteractionType.banzaigatered:
                    case InteractionType.freezeredcounter:
                    case InteractionType.FREEZE_RED_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                                _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.RED);
                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_GREEN:
                    case InteractionType.footballcountergreen:
                    case InteractionType.banzaiscoregreen:
                    case InteractionType.banzaigategreen:
                    case InteractionType.freezegreencounter:
                    case InteractionType.FREEZE_GREEN_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                                _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.GREEN);
                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_BLUE:
                    case InteractionType.footballcounterblue:
                    case InteractionType.banzaiscoreblue:
                    case InteractionType.banzaigateblue:
                    case InteractionType.freezebluecounter:
                    case InteractionType.FREEZE_BLUE_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                                _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.BLUE);
                            break;
                        }
                    case InteractionType.FOOTBALL_GOAL_YELLOW:
                    case InteractionType.footballcounteryellow:
                    case InteractionType.banzaiscoreyellow:
                    case InteractionType.banzaigateyellow:
                    case InteractionType.freezeyellowcounter:
                    case InteractionType.FREEZE_YELLOW_GATE:
                        {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                                _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.YELLOW);
                            break;
                        }
                    case InteractionType.freezeexit:
                        {
                            _room.GetFreeze().AddExitTile(Item);
                            break;
                        }
                    case InteractionType.ROLLER:
                        {
                            if (!_room.GetRoomItemHandler().GetRollers().Contains(Item))
                                _room.GetRoomItemHandler().TryAddRoller(Item.Id, Item);
                            break;
                        }
                }
            }

            if (Item.GetBaseItem().Type != 's')
                return true;

            foreach (Point coord in Item.GetCoords.ToList())
            {
                AddCoordinatedItem(Item, new Point(coord.X, coord.Y));
            }

            if (Item.GetX > (Model.MapSizeX - 1))
            {
                Model.AddX();
                GenerateMaps();
                return false;
            }

            if (Item.GetY > (Model.MapSizeY - 1))
            {
                Model.AddY();
                GenerateMaps();
                return false;
            }

            bool Return = true;

            foreach (Point coord in Item.GetCoords.ToList())
            {
                if (!ConstructMapForItem(Item, coord))
                    Return = false;
                else
                    Return = true;
            }

            return Return;
        }


        public bool CanWalk(int X, int Y, bool Override)
        {
            if (Override)
            {
                return true;
            }

            if (_room.GetRoomUserManager().GetUserForSquare(X, Y) != null && _room.RoomBlockingEnabled == 0)
                return false;

            return true;
        }

        public bool AddItemToMap(Item Item, bool NewItem = true)
        {
            return AddItemToMap(Item, true, NewItem);
        }

        public bool ItemCanMove(Item Item, Point MoveTo)
        {
            List<ThreeDCoord> Points = Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, MoveTo.X, MoveTo.Y, Item.Rotation).Values.ToList();

            if (Points == null || Points.Count == 0)
                return true;

            foreach (ThreeDCoord Coord in Points)
            {
                if (Coord.X >= Model.MapSizeX || Coord.Y >= Model.MapSizeY)
                    return false;

                if (!SquareIsOpen(Coord.X, Coord.Y, false))
                    return false;

                continue;
            }

            return true;
        }

        public byte GetFloorStatus(Point coord)
        {
            if (coord.X > mGameMap.GetUpperBound(0) || coord.Y > mGameMap.GetUpperBound(1))
                return 1;

            return mGameMap[coord.X, coord.Y];
        }

        public void SetFloorStatus(int X, int Y, byte Status)
        {
            mGameMap[X, Y] = Status;
        }

        public double GetHeightForSquareFromData(Point coord)
        {
            if (coord.X > mDynamicModel.SqFloorHeight.GetUpperBound(0) ||
                coord.Y > mDynamicModel.SqFloorHeight.GetUpperBound(1))
                return 1;
            return mDynamicModel.SqFloorHeight[coord.X, coord.Y];
        }

        public bool CanRollItemHere(int x, int y)
        {
            if (!ValidTile(x, y))
                return false;

            if (Model.SqState[x, y] == SquareState.BLOCKED)
                return false;

            return true;
        }

        public bool SquareIsOpen(int x, int y, bool pOverride)
        {
            if ((mDynamicModel.MapSizeX - 1) < x || (mDynamicModel.MapSizeY - 1) < y)
                return false;

            return CanWalk(mGameMap[x, y], pOverride);
        }

        public bool GetHighestItemForSquare(Point Square, out Item Item)
        {
            List<Item> Items = GetAllRoomItemForSquare(Square.X, Square.Y);
            Item = null;
            double HighestZ = -1;

            if (Items != null && Items.Count() > 0)
            {
                foreach (Item uItem in Items.ToList())
                {
                    if (uItem == null)
                        continue;

                    if (uItem.TotalHeight > HighestZ)
                    {
                        HighestZ = uItem.TotalHeight;
                        Item = uItem;
                        continue;
                    }
                    else
                        continue;
                }
            }
            else
                return false;

            return true;
        }

        public double GetHeightForSquare(Point Coord)
        {
            Item rItem;

            if (GetHighestItemForSquare(Coord, out rItem))
                if (rItem != null)
                    return rItem.TotalHeight;

            return 0.0;
        }

        public Point GetChaseMovement(Item Item)
        {
            int Distance = 99;
            Point Coord = new Point(0, 0);
            int iX = Item.GetX;
            int iY = Item.GetY;
            bool X = false;

            foreach (RoomUser User in _room.GetRoomUserManager().GetRoomUsers())
            {
                if (User.X == Item.GetX || Item.GetY == User.Y)
                {
                    if (User.X == Item.GetX)
                    {
                        int Difference = Math.Abs(User.Y - Item.GetY);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = false;
                        }
                        else
                            continue;

                    }
                    else if (User.Y == Item.GetY)
                    {
                        int Difference = Math.Abs(User.X - Item.GetX);
                        if (Difference < Distance)
                        {
                            Distance = Difference;
                            Coord = User.Coordinate;
                            X = true;
                        }
                        else
                            continue;
                    }
                    else
                        continue;
                }
            }

            if (Distance > 5)
                return Item.GetSides().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (X && Distance < 99)
            {
                if (iX > Coord.X)
                {
                    iX--;
                    return new Point(iX, iY);
                }
                else
                {
                    iX++;
                    return new Point(iX, iY);
                }
            }
            else if (!X && Distance < 99)
            {
                if (iY > Coord.Y)
                {
                    iY--;
                    return new Point(iX, iY);
                }
                else
                {
                    iY++;
                    return new Point(iX, iY);
                }
            }
            else
                return Item.Coordinate;
        }

        public bool IsValidStep2(RoomUser User, Vector2D From, Vector2D To, bool EndOfPath, bool Override)
        {
            if (User == null)
                return false;

            if (!ValidTile(To.X, To.Y))
                return false;

            if (Override)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            List<Item> Items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);
            if (Items.Count > 0)
            {
                bool HasGroupGate = Items.ToList().Where(x => x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE).ToList().Count() > 0;
                if (HasGroupGate)
                {
                    Item I = Items.FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE);
                    if (I != null)
                    {
                        if (User.IsBot)
                        {
                            I.ExtraData = "1";
                            I.UpdateState(false, true);
                            I.RequestUpdate(4, true);
                            return true;
                        }

                        Group Group = null;

                        if (I.GroupId < 1000)
                            Group = GroupManager.GetJob(I.GroupId);
                        else
                            Group = GroupManager.GetGang(I.GroupId);

                        if (Group == null)
                            return false;

                        if (User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetRoleplay() == null)
                            return false;

                        if (I.GroupId < 1000)
                        {
                            GroupRank Rank = GroupManager.GetJobRank(Group.Id, 1);
                            if (Rank != null && Rank.HasCommand("arrest"))
                            {
                                if (User.GetClient().GetRoleplay().PoliceTrial)
                                {
                                    I.ExtraData = "1";
                                    I.UpdateState(false, true);
                                    I.RequestUpdate(4, true);
                                    return true;
                                }
                            }
                        }

                        if (Group.IsMember(User.GetClient().GetHabbo().Id) && User.GetClient().GetRoleplay().IsWorking || User.GetClient().GetHabbo().GetPermissions().HasRight("corporation_rights") || GroupManager.HasJobCommand(User.GetClient(), "guide") && User.GetClient().GetRoleplay().IsWorking)
                        {
                            I.ExtraData = "1";
                            I.UpdateState(false, true);
                            I.RequestUpdate(4, true);
                            return true;
                        }
                        else
                        {
                            if (User.Path.Count > 0)
                                User.Path.Clear();
                            User.PathRecalcNeeded = false;
                            return false;
                        }
                    }
                }

                bool HasSlidingDoors = Items.ToList().Where(x => x.GetBaseItem().InteractionType == InteractionType.SLIDING_DOORS).ToList().Count() > 0;
                if (HasSlidingDoors)
                {
                    Item I = Items.FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.SLIDING_DOORS);
                    if (I != null)
                    {
                        if (User.IsBot)
                        {
                            I.ExtraData = "1";
                            I.UpdateState(false, true);
                            I.RequestUpdate(4, true);
                            return true;
                        }

                        if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                            return false;

                        I.ExtraData = "1";
                        I.InteractingUser = User.GetClient().GetHabbo().Id;
                        I.UpdateState(false, true);
                        I.RequestUpdate(4, true);
                        return true;
                    }
                }

                bool HasBed = Items.Where(x => x.GetBaseItem().IsBed()).ToList().Count > 0;
                if (HasBed)
                {
                    var Item = Items.Where(x => x.GetBaseItem().IsBed()).FirstOrDefault();

                    Point Square;
                    List<Point> BedTiles = Item.GetBedTiles(new Point(To.X, To.Y), out Square);

                    foreach (Point Point in BedTiles)
                    {
                        if (User.IsBot && SquareHasUsers(Point.X, Point.Y))
                            return false;
                        if (!User.IsBot && SquareHasUsers(Point.X, Point.Y, true, User.GetClient().GetRoleplay().Invisible))
                            return false;
                    }

                    if (!EndOfPath)
                        return false;
                }
            }

            bool Chair = false;
            double HighestZ = -1;
            foreach (Item Item in Items.ToList())
            {
                if (Item == null)
                    continue;

                if (Item.GetZ < HighestZ)
                {
                    Chair = false;
                    continue;
                }

                HighestZ = Item.GetZ;
                if (Item.GetBaseItem().IsSeat)
                    Chair = true;
            }

            if ((mGameMap[To.X, To.Y] == 3 && !EndOfPath && !Chair) || (mGameMap[To.X, To.Y] == 0) || (mGameMap[To.X, To.Y] == 2 && !EndOfPath))
            {
                if (User.Path.Count > 0)
                    User.Path.Clear();
                User.PathRecalcNeeded = true;
            }

            double HeightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(From.X, From.Y);
            if (HeightDiff > 1.5 && !User.RidingHorse)
                return false;

            //Check this last, because ya.
            RoomUser Userx = _room.GetRoomUserManager().GetUserForSquare(To.X, To.Y);
            if (Userx != null && !User.IsBot)
            {
                if (!Userx.IsBot && _room.TutorialEnabled)
                    return true;

                if (Userx.IsBot)
                {
                    if (Userx.GetBotRoleplay() != null)
                    {
                        if (Userx.GetBotRoleplay().Invisible)
                            return true;
                    }
                }

                if (!Userx.IsBot)
                {
                    if (Userx.GetClient() != null && Userx.GetClient().GetRoleplay() != null && User.GetClient() != null && User.GetClient().GetRoleplay() != null)
                    {
                        if (Userx.GetClient().GetRoleplay().Invisible && !User.GetClient().GetRoleplay().Invisible)
                            return true;
                    }
                }

                if (!Userx.IsWalking && EndOfPath)
                    return false;
            }
            return true;
        }

        public bool IsValidStep(Vector2D From, Vector2D To, bool EndOfPath, bool Override, bool Roller = false, bool IsBot = false, bool IsInvisibleUser = false)
        {
            if (!ValidTile(To.X, To.Y))
                return false;

            if (Override)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            if (!IsBot && _room.RoomBlockingEnabled == 0 && SquareHasUsers(To.X, To.Y, true, IsInvisibleUser) && !_room.TutorialEnabled)
                return false;

            List<Item> Items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);
            if (Items.Count > 0)
            {
                bool HasGroupGate = Items.ToList().Where(x => x != null && x.GetBaseItem().InteractionType == InteractionType.GUILD_GATE).Count() > 0;
                if (HasGroupGate)
                    return true;

                bool HasSlidingDoors = Items.ToList().Where(x => x != null && x.GetBaseItem().InteractionType == InteractionType.SLIDING_DOORS).Count() > 0;
                if (HasSlidingDoors)
                    return true;

                bool HasBed = Items.Where(x => x.GetBaseItem().IsBed()).ToList().Count > 0;
                if (HasBed)
                {
                    var Item = Items.Where(x => x.GetBaseItem().IsBed()).FirstOrDefault();
                    Point Square;
                    List<Point> BedTiles = Item.GetBedTiles(new Point(To.X, To.Y), out Square);

                    foreach (Point Point in BedTiles)
                    {
                        if (SquareHasUsers(Point.X, Point.Y))
                            return false;
                    }

                    if (!EndOfPath)
                        return false;
                }
            }

            if ((mGameMap[To.X, To.Y] == 3 && !EndOfPath) || mGameMap[To.X, To.Y] == 0 || (mGameMap[To.X, To.Y] == 2 && !EndOfPath))
                return false;

            if (!Roller)
            {
                double HeightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(From.X, From.Y);
                if (HeightDiff > 1.5)
                    return false;
            }

            return true;
        }

        public static bool CanWalk(byte pState, bool pOverride)
        {
            if (!pOverride)
            {
                if (pState == 3)
                    return true;
                if (pState == 1)
                    return true;

                return false;
            }
            return true;
        }

        public bool itemCanBePlacedHere(int x, int y)
        {
            if (mDynamicModel.MapSizeX - 1 < x || mDynamicModel.MapSizeY - 1 < y || (x == mDynamicModel.DoorX && y == mDynamicModel.DoorY))
                return false;

            return mGameMap[x, y] == 1;
        }

        public double SqAbsoluteHeight(int X, int Y)
        {
            Point Points = new Point(X, Y);

            List<int> Ids;

            lock (mCoordinatedItems)
            {
                if (mCoordinatedItems.TryGetValue(Points, out Ids))
                {
                    List<Item> Items = GetItemsFromIds(Ids);
                    return SqAbsoluteHeight(X, Y, Items);
                }
                else
                {
                    lock (mDynamicModel)
                    {
                        return mDynamicModel.SqFloorHeight[X, Y];
                    }
                }
            }
        }

        public double SqAbsoluteHeight(int X, int Y, List<Item> ItemsOnSquare)
        {
            try
            {
                bool deduct = false;
                double HighestStack = 0;
                double deductable = 0.0;

                if (ItemsOnSquare != null && ItemsOnSquare.Count > 0)
                {
                    foreach (Item Item in ItemsOnSquare.ToList())
                    {
                        if (Item == null)
                            continue;

                        if (Item.TotalHeight > HighestStack)
                        {
                            if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().IsBed())
                            {
                                deduct = true;
                                deductable = Item.GetBaseItem().Height;
                            }
                            else
                                deduct = false;
                            HighestStack = Item.TotalHeight;
                        }
                    }
                }

                double floorHeight = Model.SqFloorHeight[X, Y];
                double stackHeight = HighestStack - Model.SqFloorHeight[X, Y];

                if (deduct)
                    stackHeight -= deductable;

                if (stackHeight < 0)
                    stackHeight = 0;

                return (floorHeight + stackHeight);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SqAbsoluteHeight");
                return 0;
            }
        }

        public bool ValidTile(int X, int Y)
        {
            if (X < 0 || Y < 0 || X >= Model.MapSizeX || Y >= Model.MapSizeY)
            {
                return false;
            }

            return true;
        }

        public static Dictionary<int, ThreeDCoord> GetAffectedTiles(int Length, int Width, int PosX, int PosY, int Rotation)
        {
            int x = 0;

            var PointList = new Dictionary<int, ThreeDCoord>();

            if (Length > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (int i = 1; i < Length; i++)
                    {
                        if (!PointList.Values.Contains(new ThreeDCoord(PosX, PosY + i, i)))
                            PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (int j = 1; j < Width; j++)
                        {
                            if (!PointList.Values.Contains(new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i)))
                                PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (int i = 1; i < Length; i++)
                    {
                        if (!PointList.Values.Contains(new ThreeDCoord(PosX + i, PosY, i)))
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (int j = 1; j < Width; j++)
                        {
                            if (!PointList.Values.Contains(new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i)))
                                PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i));
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < Length; i++)
                    {
                        if (!PointList.Values.Contains(new ThreeDCoord(PosX + i, PosY, i)))
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (int j = 1; j < Width; j++)
                        {
                            if (!PointList.Values.Contains(new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i)))
                                PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i));
                        }
                    }
                }
            }

            if (Width > 1)
            {
                if (Rotation == 0 || Rotation == 4)
                {
                    for (int i = 1; i < Width; i++)
                    {
                        if (!PointList.Values.Contains(new ThreeDCoord(PosX + i, PosY, i)))
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (int j = 1; j < Length; j++)
                        {
                            if (!PointList.Values.Contains(new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i)))
                                PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, (i < j) ? j : i));
                        }
                    }
                }
                else if (Rotation == 2 || Rotation == 6)
                {
                    for (int i = 1; i < Width; i++)
                    {
                        if (!PointList.Values.Contains(new ThreeDCoord(PosX, PosY + i, i)))
                            PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (int j = 1; j < Length; j++)
                        {
                            if (!PointList.Values.Contains(new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i)))
                                PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < Width; i++)
                    {
                        if (!PointList.Values.Contains(new ThreeDCoord(PosX, PosY + i, i)))
                            PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (int j = 1; j < Length; j++)
                        {
                            if (!PointList.Values.Contains(new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i)))
                                PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, (i < j) ? j : i));
                        }
                    }
                }
            }

            if (!PointList.Values.Contains(new ThreeDCoord(PosX, PosY, 0)))
                PointList.Add(PointList.Count + 1, new ThreeDCoord(PosX, PosY, 0));

            return PointList;
        }

        public List<Item> GetItemsFromIds(List<int> Input)
        {
            if (Input == null || Input.Count == 0)
                return new List<Item>();

            List<int> Ids = new List<int>(Input).Where(x => _room.GetRoomItemHandler().GetItem(x) != null).ToList();
            List<Item> Items = new List<Item>();

            try
            {
                lock (Ids)
                {
                    foreach (int Id in Ids)
                    {
                        Item Itm = _room.GetRoomItemHandler().GetItem(Id);
                        if (Itm != null && !Items.Contains(Itm))
                            Items.Add(Itm);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Erro em GetItemsFromIds vazio: " + e);
            }

            return Items.ToList();
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY, double minZ)
        {
            var itemsToReturn = new List<Item>();

            lock (mCoordinatedItems)
            {
                var coord = new Point(pX, pY);
                if (mCoordinatedItems.ContainsKey(coord))
                {
                    var itemsFromSquare = GetItemsFromIds((List<int>)mCoordinatedItems[coord]);

                    foreach (Item item in itemsFromSquare)
                        if (item.GetZ > minZ)
                            if (item.GetX == pX && item.GetY == pY)
                                itemsToReturn.Add(item);
                }
            }
            return itemsToReturn;
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY)
        {
            var coord = new Point(pX, pY);
            var itemsToReturn = new List<Item>();

            lock (mCoordinatedItems)
            {
                if (mCoordinatedItems.ContainsKey(coord))
                {
                    var itemsFromSquare = GetItemsFromIds((List<int>)mCoordinatedItems[coord]);

                    foreach (Item item in itemsFromSquare)
                    {
                        if (item.Coordinate.X == coord.X && item.Coordinate.Y == coord.Y)
                            itemsToReturn.Add(item);
                    }
                }
            }
            return itemsToReturn;
        }

        public List<Item> GetAllRoomItemForSquare(int pX, int pY)
        {
            Point Coord = new Point(pX, pY);

            List<Item> Items = new List<Item>();
            List<int> Ids;

            lock (mCoordinatedItems)
            {
                if (mCoordinatedItems.TryGetValue(Coord, out Ids))
                    Items = GetItemsFromIds(Ids);
                else
                    Items = new List<Item>();
            }
            return Items;
        }

        public bool SquareHasUsers(int X, int Y, bool CheckingInvisible = false, bool IsInvisible = false)
        {
            return MapGotUser(new Point(X, Y), CheckingInvisible, IsInvisible);
        }


        public static bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            if (!(Math.Abs(X1 - X2) > 1 || Math.Abs(Y1 - Y2) > 1)) return true;
            if (X1 == X2 && Y1 == Y2) return true;
            return false;
        }

        public static int TileDistance(int X1, int Y1, int X2, int Y2)
        {
            return Math.Abs(X1 - X2) + Math.Abs(Y1 - Y2);
        }

        public DynamicRoomModel Model
        {
            get { return mDynamicModel; }
        }

        public RoomModel StaticModel
        {
            get { return mStaticModel; }
        }

        public byte[,] EffectMap
        {
            get { return mUserItemEffect; }
        }

        public byte[,] GameMap
        {
            get { return mGameMap; }
        }

        public void Dispose()
        {
            userMap.Clear();
            mDynamicModel.Destroy();
            mCoordinatedItems.Clear();

            Array.Clear(mGameMap, 0, mGameMap.Length);
            Array.Clear(mUserItemEffect, 0, mUserItemEffect.Length);
            Array.Clear(mItemHeightMap, 0, mItemHeightMap.Length);

            userMap = null;
            mGameMap = null;
            mUserItemEffect = null;
            mItemHeightMap = null;
            mCoordinatedItems = null;

            mDynamicModel = null;
            this._room = null;
            mStaticModel = null;
        }
    }
}