using log4net;
using Plus.Core;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Gambling
{
    public class TexasHoldEmManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Gambling.TexasHoldEmManager");

        /// <summary>
        /// Concurrent dictionary containing Texas Hold Em Game List
        /// </summary>
        public static ConcurrentDictionary<int, TexasHoldEm> GameList = new ConcurrentDictionary<int, TexasHoldEm>();

        /// <summary>
        /// Generates the Texas Hold 'Em Games dictionary from the database.
        /// </summary>
        public static void Initialize()
        {
            ClearOldData();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_gambling`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int GameId = Convert.ToInt32(Row["id"]);

                        if (GameList.ContainsKey(GameId))
                            continue;

                        int RoomId = Convert.ToInt32(Row["room_id"]);
                        string[] Player1Data = Row["player_1"].ToString().Split(';');
                        string[] Player2Data = Row["player_2"].ToString().Split(';');
                        string[] Player3Data = Row["player_3"].ToString().Split(';');

                        ConcurrentDictionary<int, int> PlayerList = new ConcurrentDictionary<int, int>();
                        ConcurrentDictionary<int, TexasHoldEmItem> Player1 = GeneratePlayer(GameId, 1);
                        ConcurrentDictionary<int, TexasHoldEmItem> Player2 = GeneratePlayer(GameId, 2);
                        ConcurrentDictionary<int, TexasHoldEmItem> Player3 = GeneratePlayer(GameId, 3);
                        ConcurrentDictionary<int, TexasHoldEmItem> Banker = GeneratePlayer(GameId, 0);

                        TexasHoldEmItem JoinGate = GenerateJoinGate(Row);
                        TexasHoldEmItem PotSquare = GeneratePotSquare(Row);

                        int JoinCost = Convert.ToInt32(Row["join_cost"]);

                        TexasHoldEm Game = new TexasHoldEm(GameId, RoomId, Player1, Player2, Player3, Banker, PotSquare, JoinGate, Player1Data, Player2Data, Player3Data, JoinCost);

                        GameList.TryAdd(GameId, Game);

                        #region Spawn Furni
                        Room Room = RoleplayManager.GenerateRoom(RoomId);
                        if (Room != null && Room.GetRoomItemHandler() != null)
                        {
                            if (Game.PotSquare.Furni == null)
                                Game.PotSquare.SpawnDice();

                            if (Game.JoinGate.Furni == null)
                                Game.JoinGate.SpawnDice();

                            foreach (TexasHoldEmItem Item in Game.Player1.Values)
                            {
                                if (Item != null && Item.Furni == null)
                                    Item.SpawnDice();
                            }

                            foreach (TexasHoldEmItem Item in Game.Player2.Values)
                            {
                                if (Item != null && Item.Furni == null)
                                    Item.SpawnDice();
                            }

                            foreach (TexasHoldEmItem Item in Game.Player3.Values)
                            {
                                if (Item != null && Item.Furni == null)
                                    Item.SpawnDice();
                            }

                            foreach (TexasHoldEmItem Item in Game.Banker.Values)
                            {
                                if (Item != null && Item.Furni == null)
                                    Item.SpawnDice();
                            }
                        }
                        #endregion
                    }
                }
            }

            //log.Info("Carregado " + GameList.Count + " Jogos de Texas Hold!");
        }

        /// <summary>
        /// Clears all existing data
        /// </summary>
        public static void ClearOldData()
        {
            if (GameList.Count > 0)
            {
                foreach (TexasHoldEm Game in GameList.Values)
                {
                    if (Game.TimerManager != null)
                        Game.TimerManager.EndAllTimers();

                    Room Room = RoleplayManager.GenerateRoom(Game.RoomId);

                    if (Room != null)
                    {
                        #region Reset Game
                        Game.RemovePotFurni();

                        lock (Game.PlayerList)
                        {
                            foreach (var Player in Game.PlayerList.Values)
                            {
                                if (Player == null)
                                    continue;

                                if (Player.UserId > 0)
                                    Game.RemovePlayerFromGame(Player.UserId);
                            }
                        }

                        Game.ResetGame();
                        #endregion

                        #region Bet Furni
                        foreach (int Key in Game.PlayerList.Keys)
                        {
                            Game.RemoveBetFurni(Key);
                        }
                        Game.RemovePotFurni();
                        #endregion

                        #region PotSquare Check
                        if (Game.PotSquare.Furni != null)
                        {
                            if (Room.GetRoomItemHandler().GetFloor.Contains(Game.PotSquare.Furni))
                                Room.GetRoomItemHandler().RemoveFurniture(null, Game.PotSquare.Furni.Id);
                        }
                        #endregion

                        #region JoinGate Check
                        if (Game.JoinGate.Furni != null)
                        {
                            if (Room.GetRoomItemHandler().GetFloor.Contains(Game.JoinGate.Furni))
                                Room.GetRoomItemHandler().RemoveFurniture(null, Game.JoinGate.Furni.Id);
                        }
                        #endregion

                        #region Player1 Check
                        foreach (TexasHoldEmItem Item in Game.Player1.Values)
                        {
                            if (Item != null && Item.Furni != null)
                            {
                                if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                    Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                            }
                        }
                        #endregion

                        #region Player2 Check
                        foreach (TexasHoldEmItem Item in Game.Player2.Values)
                        {
                            if (Item != null && Item.Furni != null)
                            {
                                if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                    Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                            }
                        }
                        #endregion

                        #region Player3 Check
                        foreach (TexasHoldEmItem Item in Game.Player3.Values)
                        {
                            if (Item != null && Item.Furni != null)
                            {
                                if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                    Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                            }
                        }
                        #endregion

                        #region Banker Check
                        foreach (TexasHoldEmItem Item in Game.Banker.Values)
                        {
                            if (Item != null && Item.Furni != null)
                            {
                                if (Room.GetRoomItemHandler().GetFloor.Contains(Item.Furni))
                                    Room.GetRoomItemHandler().RemoveFurniture(null, Item.Furni.Id);
                            }
                        }
                        #endregion
                    }
                }
            }
            GameList.Clear();
        }

        /// <summary>
        /// Generates the players for game id
        /// </summary>
        public static ConcurrentDictionary<int, TexasHoldEmItem> GeneratePlayer(int GameId, int PlayerId)
        {
            ConcurrentDictionary<int, TexasHoldEmItem> Player = new ConcurrentDictionary<int, TexasHoldEmItem>();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_gambling_items` WHERE `game_id` = '" + GameId + "' AND `player_id` = '" + PlayerId + "'");
                DataTable Table = dbClient.getTable();

                if (Table == null)
                    return null;

                foreach (DataRow Row in Table.Rows)
                {
                    int DiceId = Convert.ToInt32(Row["dice_id"]);

                    if (Player.ContainsKey(DiceId))
                        continue;

                    int ItemId = Convert.ToInt32(Row["item_id"]);
                    int RoomId = Convert.ToInt32(Row["room_id"]);
                    int X = Convert.ToInt32(Row["x"]);
                    int Y = Convert.ToInt32(Row["y"]);
                    double Z = Convert.ToDouble(Row["z"]);
                    int Rotation = Convert.ToInt32(Row["rotation"]);

                    TexasHoldEmItem Item = new TexasHoldEmItem(RoomId, ItemId, X, Y, Z, Rotation);

                    Player.TryAdd(DiceId, Item);
                }
            }

            return Player;
        }
        
        /// <summary>
        /// Generates the pot square data from the database.
        /// </summary>
        public static TexasHoldEmItem GeneratePotSquare(DataRow Row)
        {
            if (Row == null)
                return null;

            int ItemId = Convert.ToInt32(Row["item_id"]);
            int RoomId = Convert.ToInt32(Row["room_id"]);
            int X = Convert.ToInt32(Row["x"]);
            int Y = Convert.ToInt32(Row["y"]);
            double Z = Convert.ToDouble(Row["z"]);
            int Rotation = Convert.ToInt32(Row["rotation"]);

            return new TexasHoldEmItem(RoomId, ItemId, X, Y, Z, Rotation);
        }

        /// <summary>
        /// Generates the join gate data from the database.
        /// </summary>
        public static TexasHoldEmItem GenerateJoinGate(DataRow Row)
        {
            if (Row == null)
                return null;

            int ItemId = Convert.ToInt32(Row["gate_item_id"]);
            int RoomId = Convert.ToInt32(Row["gate_room_id"]);
            int X = Convert.ToInt32(Row["gate_x"]);
            int Y = Convert.ToInt32(Row["gate_y"]);
            double Z = Convert.ToDouble(Row["gate_z"]);
            int Rotation = Convert.ToInt32(Row["gate_rotation"]);

            return new TexasHoldEmItem(RoomId, ItemId, X, Y, Z, Rotation);
        }

        /// <summary>
        /// Gets all texas hold 'em games in the room based
        /// </summary>
        public static List<TexasHoldEm> GetGamesByRoomId(int RoomId)
        {
            if (GameList.Values.Where(x => x.RoomId == RoomId).ToList().Count > 0)
                return GameList.Values.Where(x => x.RoomId == RoomId).ToList();
            else
                return new List<TexasHoldEm>();
        }
        
        /// <summary>
        /// Removes the player from their game
        /// </summary>
        public static void RemovePlayer(int UserId)
        {
            try
            {
                TexasHoldEm Game = GetGameForUser(UserId);

                if (Game != null)
                    Game.RemovePlayerFromGame(UserId);
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in RemovePlayer() void: " + e);
            }
        }

        /// <summary>
        /// Gets the Texas Hold 'Em Game that the player is in
        /// </summary>
        public static TexasHoldEm GetGameForUser(int UserId)
        {
            if (GameList.Count <= 0)
                return null;

            return GameList.Values.FirstOrDefault(x => x != null && x.PlayerList != null && x.PlayerList.Values.Where(y => y != null && y.UserId == UserId).ToList().Count > 0);
        }

        /// <summary>
        /// Gets the player id based on the Dice Furni
        /// </summary>
        public static int GetPlayerByDice(Item Item, out TexasHoldEm RealGame)
        {
            RealGame = null;
            int Number = 0;

            if (Item == null || Item.TexasHoldEmData == null)
                return Number;

            if (GameList == null || GameList.Count <= 0)
                return Number;

            lock (GameList)
            {
                foreach (TexasHoldEm Game in GameList.Values)
                {
                    if (Game == null)
                        continue;

                    if (Game.Banker.Values.Where(x => x.Furni != null && x.Furni == Item).ToList().Count > 0)
                    {
                        RealGame = Game;
                        Number = 0;
                        break;
                    }

                    if (Game.Player1.Values.Where(x => x.Furni != null && x.Furni == Item).ToList().Count > 0)
                    {
                        RealGame = Game;
                        Number = 1;
                        break;
                    }

                    if (Game.Player2.Values.Where(x => x.Furni != null && x.Furni == Item).ToList().Count > 0)
                    {
                        RealGame = Game;
                        Number = 2;
                        break;
                    }

                    if (Game.Player3.Values.Where(x => x.Furni != null && x.Furni == Item).ToList().Count > 0)
                    {
                        RealGame = Game;
                        Number = 3;
                        break;
                    }
                }
            }

            if (RealGame == null || Number == 0 || RealGame.PlayerList == null || !RealGame.PlayerList.ContainsKey(Number))
                return 0;

            return RealGame.PlayerList[Number].UserId;
        }
    }
}
