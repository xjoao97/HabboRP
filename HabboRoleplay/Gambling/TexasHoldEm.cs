using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Timers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Items;
using Plus.Core;

namespace Plus.HabboRoleplay.Gambling
{
    public class TexasHoldEm
    {
        #region Variables
        public int GameId;
        public int RoomId;

        public ConcurrentDictionary<int, TexasHoldEmPlayer> PlayerList;
        public ConcurrentDictionary<int, TexasHoldEmItem> Player1;
        public ConcurrentDictionary<int, TexasHoldEmItem> Player2;
        public ConcurrentDictionary<int, TexasHoldEmItem> Player3;
        public ConcurrentDictionary<int, TexasHoldEmItem> Banker;

        public TexasHoldEmItem PotSquare;
        public TexasHoldEmItem JoinGate;

        public SystemTimerManager TimerManager;

        public bool GameStarted;
        public int JoinCost;

        public string[] Player1Data;
        public string[] Player2Data;
        public string[] Player3Data;

        public int GameSequence;
        public int PlayersTurn;
        #endregion

        /// <summary>
        /// Constructor for Texas Hold 'Em Game
        /// </summary>
        public TexasHoldEm(int GameId, int RoomId, ConcurrentDictionary<int, TexasHoldEmItem> Player1, 
            ConcurrentDictionary<int, TexasHoldEmItem> Player2, ConcurrentDictionary<int, TexasHoldEmItem> Player3, 
            ConcurrentDictionary<int, TexasHoldEmItem> Banker, TexasHoldEmItem PotSquare, TexasHoldEmItem JoinGate,
            string[] Player1Data, string[] Player2Data, string[] Player3Data, int JoinCost)
        {
            this.GameId = GameId;
            this.RoomId = RoomId;

            this.PlayerList = new ConcurrentDictionary<int, TexasHoldEmPlayer>();
            this.PlayerList.TryAdd(1, null);
            this.PlayerList.TryAdd(2, null);
            this.PlayerList.TryAdd(3, null);

            this.Player1 = Player1;
            this.Player2 = Player2;
            this.Player3 = Player3;
            this.Banker = Banker;

            this.PotSquare = PotSquare;
            this.JoinGate = JoinGate;

            this.TimerManager = new SystemTimerManager();

            this.GameStarted = false;

            this.Player1Data = Player1Data;
            this.Player2Data = Player2Data;
            this.Player3Data = Player3Data;

            this.JoinCost = JoinCost;

            this.GameSequence = 0;
            this.PlayersTurn = 0;
        }

        /// <summary>
        /// Adds the player to the game based on UserId
        /// </summary>
        public void AddPlayerToGame(int UserId)
        {
            try
            {
                if (GameStarted)
                    return;

                GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

                if (Player == null || Player.GetHabbo() == null || Player.GetRoleplay() == null || Player.GetRoomUser() == null)
                    return;

                if (Player.GetHabbo().Credits < this.JoinCost)
                {
                    Player.SendWhisper("Você não tem dinheiro suficiente para jogar! Os custos iniciais são R$" + String.Format("{0:N0}", this.JoinCost) + "!", 1);
                    return;
                }

                lock (PlayerList)
                {
                    if (PlayerList.Values.Where(x => x != null && x.UserId > 0).ToList().Count >= 3)
                        return;

                    if (PlayerList.Values.Where(x => x != null && x.UserId == UserId).ToList().Count > 0)
                        return;
                }

                int Number = PlayerList.Where(x => x.Value == null || x.Value.UserId == 0).FirstOrDefault().Key;

                string[] PlayerData;
                if (Number == 1)
                    PlayerData = Player1Data;
                else if (Number == 2)
                    PlayerData = Player2Data;
                else
                    PlayerData = Player3Data;

                RoleplayManager.Shout(Player, "*Junta-se ao jogo Texas Hold como jogador " + Number + "*", 4);
                PlayerList.TryUpdate(Number, new TexasHoldEmPlayer(UserId, 0, this.JoinCost), PlayerList[Number]);

                Point NewCoord = new Point(Convert.ToInt32(PlayerData[0]), Convert.ToInt32(PlayerData[1]));

                Player.GetRoomUser().GetRoom().GetGameMap().TeleportToSquare(Player.GetRoomUser(), NewCoord);
                Player.GetRoomUser().GetRoom().GetRoomUserManager().UpdateUserStatusses();
                Player.GetRoleplay().TexasHoldEmPlayer = Number;

                SpawnStartingBet(Number);

                if (this.PlayerList.Values.Where(x => x != null && x.UserId > 0).ToList().Count == 3)
                {
                    if (!this.TimerManager.ActiveTimers.ContainsKey("texasholdem"))
                        this.TimerManager.CreateTimer("texasholdem", 1000, true, this);

                    this.PlayersTurn = 1;
                    this.GameStarted = true;
                    SendStartMessage();

                    if (this.PlayerList != null && this.PlayerList.ContainsKey(1) && this.PlayerList[1] != null)
                    {
                        GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(this.PlayerList[1].UserId);
                        if (Client != null)
                            SendStartMessage("Sua vez, " + Client.GetHabbo().Username + " gire seus dados!");
                    }
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in AddPlayerToGame() void: " + e);
            }
        }

        /// <summary>
        /// Removes a player from the game based on UserId
        /// </summary>
        public void RemovePlayerFromGame(int UserId)
        {
            try
            {
                if (this.PlayerList.Values.Where(x => x != null && x.UserId == UserId).ToList().Count <= 0)
                    return;

                int Key = this.PlayerList.Where(x => x.Value != null && x.Value.UserId == UserId).FirstOrDefault().Key;

                this.PlayerList.TryUpdate(Key, new TexasHoldEmPlayer(0, this.PlayerList[Key].CurrentBet, this.PlayerList[Key].TotalAmount), this.PlayerList[Key]);
                RemoveBetFurni(Key);

                ConcurrentDictionary<int, TexasHoldEmItem> Data;
                if (Key == 1)
                    Data = this.Player1;
                else if (Key == 2)
                    Data = this.Player2;
                else
                    Data = this.Player3;

                if (Data != null)
                {
                    foreach (var Item in Data.Values)
                    {
                        if (Item.Furni != null)
                        {
                            Item.Furni.ExtraData = "0";
                            Item.Furni.UpdateState(false, true);
                        }

                        Item.Rolled = false;
                        Item.Value = 0;
                    }
                }

                GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

                if (Player != null)
                {
                    if (Player.GetRoomUser() != null && Player.GetRoomUser().GetRoom() != null)
                    {
                        Player.GetRoomUser().GetRoom().GetGameMap().TeleportToSquare(Player.GetRoomUser(), new Point(Player.GetRoomUser().GetRoom().Model.DoorX, Player.GetRoomUser().GetRoom().Model.DoorY));
                        Player.GetRoomUser().GetRoom().GetRoomUserManager().UpdateUserStatusses();
                    }

                    if (Player.GetRoleplay() != null)
                        Player.GetRoleplay().TexasHoldEmPlayer = 0;
                }

                if (this.GameStarted && this.PlayersTurn == Key)
                    ChangeTurn();
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in RemovePlayerToGame() void: " + e);
            }
        }

        /// <summary>
        /// Sends a whisper to all players saying the game has started
        /// </summary>
        public void SendStartMessage(string Message = "")
        {
            try
            {
                if (!GameStarted)
                    return;

                lock (this.PlayerList)
                {
                    foreach (int UserId in this.PlayerList.Values.Where(x => x != null && x.UserId > 0).Select(x => x.UserId))
                    {
                        GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);

                        if (Player == null || Player.GetHabbo() == null || Player.GetRoleplay() == null)
                            continue;

                        Player.SendWhisper((Message == "" ? "O Texas Hold Game está prestes a começar!" : Message), 38);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in SendStartMessage() void: " + e);
            }
        }

        /// <summary>
        /// Removes old bet furni and spawns new bet furni
        /// </summary>
        public void SpawnStartingBet(int Number)
        {
            try
            {
                Room Room = RoleplayManager.GenerateRoom(this.RoomId);

                if (Room == null)
                    return;

                TexasHoldEmPlayer Player = this.PlayerList[Number];

                if (Player == null || Player.UserId == 0)
                    return;

                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Player.UserId);

                if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null)
                    return;

                RemoveBetFurni(Number);
                SpawnBetFurni(Number, Player.TotalAmount);
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in SpawnStartingBet() void: " + e);
            }
        }

        /// <summary>
        /// Places the players total amount (join cost) furni in the spawn point
        /// </summary>
        public void SpawnBetFurni(int Number, int Amount)
        {
            try
            {
                Room Room = RoleplayManager.GenerateRoom(this.RoomId);

                if (Room == null || Room.GetRoomItemHandler() == null || Room.GetGameMap() == null)
                    return;

                string[] Data;
                if (Number == 1)
                    Data = this.Player1Data;
                else if (Number == 2)
                    Data = this.Player2Data;
                else
                    Data = this.Player3Data;

                int X = Convert.ToInt32(Data[2]);
                int Y = Convert.ToInt32(Data[3]);

                int Hundreds = 0;
                int Fifties = 0;
                int Tens = 0;
                int Fives = 0;
                double Height = Room.GetGameMap().GetHeightForSquare(new Point(X, Y));

                #region Set Values
                if (Amount >= 100)
                {
                    Hundreds = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 100)));
                    Amount -= 100 * Hundreds;
                }

                if (Amount >= 50)
                {
                    Fifties = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 50)));
                    Amount -= 50 * Fifties;
                }

                if (Amount >= 10)
                {
                    Tens = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 10)));
                    Amount -= 10 * Tens;
                }

                if (Amount >= 5)
                {
                    Fives = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 5)));
                    Amount -= 5 * Fives;
                }
                #endregion

                #region Spawn Bars
                if (Hundreds > 0)
                {
                    while (Hundreds > 0)
                    {
                        // Diamond Bar
                        RoleplayManager.PlaceItemToRoom(null, 200054, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Hundreds--;
                    }
                }

                if (Fifties > 0)
                {
                    while (Fifties > 0)
                    {
                        // Emerald Bar
                        RoleplayManager.PlaceItemToRoom(null, 200062, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Fifties--;
                    }
                }

                if (Tens > 0)
                {
                    while (Tens > 0)
                    {
                        // Ruby Bar
                        RoleplayManager.PlaceItemToRoom(null, 200058, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Tens--;
                    }
                }

                if (Fives > 0)
                {
                    while (Fives > 0)
                    {
                        // Sapphire Bar
                        RoleplayManager.PlaceItemToRoom(null, 200056, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Fives--;
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in SpawnBetFurni() void: " + e);
            }
        }

        /// <summary>
        /// Removes all the players bet furni in the spawn point
        /// </summary>
        /// <param name="Number"></param>
        public void RemoveBetFurni(int Number)
        {
            try
            {
                Room Room = RoleplayManager.GenerateRoom(this.RoomId);

                if (Room == null || Room.GetRoomItemHandler() == null || Room.GetGameMap() == null)
                    return;

                string[] Data;
                if (Number == 1)
                    Data = this.Player1Data;
                else if (Number == 2)
                    Data = this.Player2Data;
                else
                    Data = this.Player3Data;

                int X = Convert.ToInt32(Data[2]);
                int Y = Convert.ToInt32(Data[3]);

                List<Item> Items = Room.GetGameMap().GetRoomItemForSquare(X, Y).Where(x => x.GetBaseItem().ItemName.ToLower().Contains("cfc")).ToList();

                if (Items.Count <= 0)
                    return;

                lock (Items)
                {
                    foreach (Item Item in Items)
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in RemoveBetFurni() void: " + e);
            }
        }

        /// <summary>
        /// Places the users bet onto the pot square
        /// </summary>
        public void PlacePotFurni(int Number, int Amount)
        {
            try
            {
                Room Room = RoleplayManager.GenerateRoom(this.RoomId);

                if (Room == null || Room.GetRoomItemHandler() == null || Room.GetGameMap() == null)
                    return;

                TexasHoldEmPlayer Player = this.PlayerList[Number];

                if (Player == null)
                    return;

                if (Player.TotalAmount - Amount < 0)
                    return;

                Player.TotalAmount -= Amount;
                Player.CurrentBet += Amount;

                int X = PotSquare.X;
                int Y = PotSquare.Y;

                int Thousands = 0;
                int Hundreds = 0;
                int Fifties = 0;
                int Tens = 0;
                int Fives = 0;
                double Height = Room.GetGameMap().GetHeightForSquare(new Point(X, Y));

                #region Set Values
                if (Amount >= 1000)
                {
                    Thousands = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 1000)));
                    Amount -= 1000 * Thousands;
                }

                if (Amount >= 100)
                {
                    Hundreds = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 100)));
                    Amount -= 100 * Hundreds;
                }

                if (Amount >= 50)
                {
                    Fifties = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 50)));
                    Amount -= 50 * Fifties;
                }

                if (Amount >= 10)
                {
                    Tens = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 10)));
                    Amount -= 10 * Tens;
                }

                if (Amount >= 5)
                {
                    Fives = Convert.ToInt32(Math.Floor(Convert.ToDouble((double)Amount / 5)));
                    Amount -= 5 * Fives;
                }
                #endregion

                #region Spawn Bars
                if (Thousands > 0)
                {
                    while (Thousands > 0)
                    {
                        // Obsidian Bar
                        RoleplayManager.PlaceItemToRoom(null, 200059, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Thousands--;
                    }
                }

                if (Hundreds > 0)
                {
                    while (Hundreds > 0)
                    {
                        // Diamond Bar
                        RoleplayManager.PlaceItemToRoom(null, 200054, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Hundreds--;
                    }
                }

                if (Fifties > 0)
                {
                    while (Fifties > 0)
                    {
                        // Emerald Bar
                        RoleplayManager.PlaceItemToRoom(null, 200062, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Fifties--;
                    }
                }

                if (Tens > 0)
                {
                    while (Tens > 0)
                    {
                        // Ruby Bar
                        RoleplayManager.PlaceItemToRoom(null, 200058, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Tens--;
                    }
                }

                if (Fives > 0)
                {
                    while (Fives > 0)
                    {
                        // Sapphire Bar
                        RoleplayManager.PlaceItemToRoom(null, 200056, 0, X, Y, Height, 0, false, this.RoomId, false, "0");
                        Height += 0.5;
                        Fives--;
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in AddPlayerToGame() void: " + e);
            }
        }

        /// <summary>
        /// Removes all bet furni from the pot square
        /// </summary>
        public void RemovePotFurni()
        {
            try
            {
                Room Room = RoleplayManager.GenerateRoom(this.RoomId);

                if (Room == null || Room.GetRoomItemHandler() == null || Room.GetGameMap() == null)
                    return;

                int X = PotSquare.X;
                int Y = PotSquare.Y;

                List<Item> Items = Room.GetGameMap().GetRoomItemForSquare(X, Y).Where(x => x.GetBaseItem().ItemName.ToLower().Contains("cfc")).ToList();

                if (Items.Count <= 0)
                    return;

                lock (Items)
                {
                    foreach (Item Item in Items)
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in RemovePotFurni() void: " + e);
            }
        }

        /// <summary>
        ///  Gets the required minimum bet
        /// </summary>
        public int MinimumBet(int Number)
        {
            var Player = this.PlayerList[Number];

            int Required = this.PlayerList.Values.OrderByDescending(x => x.CurrentBet).FirstOrDefault().CurrentBet;

            return (Required - Player.CurrentBet);
        }

        /// <summary>
        /// Rolls the dice for the Texas Hold 'Em Game
        /// </summary>
        public void RollDice(GameClient Session, Item Item, int Request)
        {
            try
            {
                if (!GameStarted)
                    return;

                if (Session == null || Item == null)
                    return;

                if (Item.ExtraData == "-1")
                    return;

                int Number = Session.GetRoleplay().TexasHoldEmPlayer;

                if (Number <= 0)
                    return;

                if (PlayersTurn != Number)
                {
                    Session.SendWhisper("Não é sua vez de girar seus dados!", 1);
                    return;
                }

                ConcurrentDictionary<int, TexasHoldEmItem> Data;
                if (Number == 1)
                    Data = Player1;
                else if (Number == 2)
                    Data = Player2;
                else
                    Data = Player3;

                if (Data == null || Data.Count <= 0)
                    return;

                if (Data.Values.Where(x => x != null && x.Furni != null && x.Furni == Item).ToList().Count <= 0)
                    return;

                TexasHoldEmItem TexasItem = Data.Values.FirstOrDefault(x => x != null && x.Furni != null && x.Furni == Item);

                if (TexasItem == null || TexasItem.Rolled)
                    return;

                #region Dice Roll
                if (Item.ExtraData != "-1")
                {
                    if (Request == -1)
                    {
                        Item.ExtraData = "0";
                        Item.UpdateState();
                    }
                    else
                    {
                        Item.ExtraData = "-1";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(3, true);
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in RollDice() void: " + e);
            }
        }

        /// <summary>
        /// Progress the turn of the game
        /// </summary>
        public void ChangeTurn()
        {
            try
            {
                this.PlayersTurn++;

                if (this.PlayersTurn == 4)
                {
                    this.GameSequence++;
                    InitiateSequence();
                    return;
                }

                if (this.PlayersTurn == 1 && this.PlayerList[1].UserId == 0)
                    this.PlayersTurn++;

                if (this.PlayersTurn == 2 && this.PlayerList[2].UserId == 0)
                    this.PlayersTurn++;

                if (this.PlayersTurn == 3 && this.PlayerList[3].UserId == 0)
                    this.PlayersTurn++;

                if (this.PlayersTurn == 4)
                {
                    this.GameSequence++;
                    InitiateSequence();
                    return;
                }

                int PlayerId = this.PlayerList[this.PlayersTurn].UserId;

                if (PlayerId > 0)
                {
                    GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(PlayerId);

                    if (Client != null)
                    {
                        if (GameSequence == 0)
                            SendStartMessage("Sua vez " + Client.GetHabbo().Username + ", gire seus dados!");
                        else if (GameSequence == 1)
                        {
                            SendStartMessage("Sua vez " + Client.GetHabbo().Username + ". Deseja ':apostar' ou ':passar' ou ':sairjogo'?");
                            Client.SendWhisper("The current minimum bet is $" + String.Format("{0:N0}", this.MinimumBet(this.PlayersTurn)) + "! You only have $" + String.Format("{0:N0}", this.PlayerList[this.PlayersTurn].TotalAmount) + " left!", 1);
                        }
                        else if (GameSequence == 2)
                        {
                            SendStartMessage("Sua vez " + Client.GetHabbo().Username + ". Deseja ':apostar' ou ':passar' ou ':sairjogo'?");
                            Client.SendWhisper("The current minimum bet is $" + String.Format("{0:N0}", this.MinimumBet(this.PlayersTurn)) + "! You only have $" + String.Format("{0:N0}", this.PlayerList[this.PlayersTurn].TotalAmount) + " left!", 1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in ChangeTurn() void: " + e);
            }
        }

        /// <summary>
        /// Changes the game sequence
        /// </summary>
        public void InitiateSequence()
        {
            try
            {
                if (Banker.ContainsKey(this.GameSequence))
                {
                    Item Dice = Banker[this.GameSequence].Furni;

                    if (Dice != null)
                        Dice.Interactor.OnTrigger(null, Dice, 0, false);
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in FinalSequence() void: " + e);
            }
        }

        /// <summary>
        /// Picks the winner
        /// </summary>
        public void ChooseWinner()
        {
            try
            {
                ConcurrentDictionary<int, List<TexasHoldEmItem>> Dice = new ConcurrentDictionary<int, List<TexasHoldEmItem>>();

                lock (this.PlayerList)
                {
                    if (this.PlayerList.Where(x => x.Value != null && x.Value.UserId > 0).ToList().Count == 1)
                    {
                        int WinnerId = this.PlayerList.FirstOrDefault(x => x.Value != null && x.Value.UserId > 0).Key;
                        EndGame(WinnerId);
                        return;
                    }

                    foreach (var Player in this.PlayerList.Where(x => x.Value != null && x.Value.UserId > 0).ToList())
                    {
                        int Number = Player.Key;

                        ConcurrentDictionary<int, TexasHoldEmItem> Data;
                        if (Number == 1)
                            Data = Player1;
                        else if (Number == 2)
                            Data = Player2;
                        else
                            Data = Player3;

                        Data.TryAdd(3, Banker[1]);
                        Data.TryAdd(4, Banker[2]);
                        Data.TryAdd(5, Banker[3]);

                        if (!Dice.ContainsKey(Number))
                            Dice.TryAdd(Number, Data.Values.ToList());
                    }
                }

                // Player Number <Score, Dice Number>
                Dictionary<int, KeyValuePair<int, int>> WinnerList = new Dictionary<int, KeyValuePair<int, int>>();

                foreach (var Player in Dice)
                {
                    int Number = Player.Key;
                    List<TexasHoldEmItem> PlayerDice = Player.Value;

                    int DiceValue = 0;
                    int CardValue = 0;

                    List<int> DiceRolls = PlayerDice.Select(x => x.Value).ToList();

                    var Grouped = DiceRolls.GroupBy(x => x).Select(group => new { Number = group.Key, Count = group.Count() });

                    #region 5 Of a Kind
                    if (Grouped.Where(x => x.Count == 5).ToList().Count > 0)
                    {
                        CardValue = Grouped.FirstOrDefault(x => x.Count == 5).Number;
                        DiceValue = 8;
                    }
                    #endregion

                    #region 4 Of a Kind
                    else if (Grouped.Where(x => x.Count == 4).ToList().Count > 0 && DiceValue < 7)
                    {
                        CardValue = Grouped.FirstOrDefault(x => x.Count == 4).Number;
                        DiceValue = 7;
                    }
                    #endregion

                    #region Full House
                    else if (Grouped.Where(x => x.Count == 3).ToList().Count == 1 && Grouped.Where(x => x.Count == 2).ToList().Count == 1 && DiceValue < 6)
                    {
                        CardValue = Grouped.FirstOrDefault(x => x.Count == 3).Number;
                        DiceValue = 6;
                    }
                    #endregion

                    #region Straight
                    else if (PlayerDice.OrderBy(x => x.Value).ToList()[0].Value == 2 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[1].Value == 3 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[2].Value == 4 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[3].Value == 5 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[4].Value == 6 && DiceValue < 5)
                    {
                        CardValue = 2;
                        DiceValue = 5;
                    }
                    else if (PlayerDice.OrderBy(x => x.Value).ToList()[0].Value == 1 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[1].Value == 2 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[2].Value == 3 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[3].Value == 4 &&
                        PlayerDice.OrderBy(x => x.Value).ToList()[4].Value == 5 && DiceValue < 5)
                    {
                        CardValue = 1;
                        DiceValue = 5;
                    }
                    #endregion

                    #region 3 Of a Kind
                    else if (Grouped.Where(x => x.Count == 3).ToList().Count == 1 && DiceValue < 4)
                    {
                        CardValue = Grouped.FirstOrDefault(x => x.Count == 3).Number;
                        DiceValue = 4;
                    }
                    #endregion

                    #region Two Doubles
                    else if (Grouped.Where(x => x.Count == 2).ToList().Count == 2 && DiceValue < 3)
                    {
                        CardValue = Grouped.OrderByDescending(x => x.Number).FirstOrDefault(x => x.Count == 2).Number;
                        DiceValue = 3;
                    }
                    #endregion

                    #region 2 Of a Kind
                    else if (Grouped.Where(x => x.Count == 2).ToList().Count > 0 && DiceValue < 2)
                    {
                        CardValue = Grouped.Where(x => x.Count == 2).FirstOrDefault().Number;
                        DiceValue = 2;
                    }
                    #endregion

                    #region High Card
                    else
                    {
                        CardValue = Grouped.OrderByDescending(x => x.Number).FirstOrDefault().Number;
                        DiceValue = 1;
                    }
                    #endregion

                    WinnerList.Add(Number, new KeyValuePair<int, int>(DiceValue, CardValue));
                }

                if (WinnerList.Where(x => x.Value.Key == WinnerList.OrderByDescending(y => y.Value.Key).FirstOrDefault().Key).ToList().Count > 1)
                {
                    int Winner = WinnerList.Where(x => x.Value.Key == WinnerList.OrderByDescending(y => y.Value.Key).FirstOrDefault().Key).OrderByDescending(x => x.Value.Value).FirstOrDefault().Key;

                    EndGame(Winner);
                    return;
                }
                else
                {
                    int Winner = WinnerList.OrderByDescending(x => x.Value.Key).FirstOrDefault().Key;

                    EndGame(Winner);
                    return;
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in FinalSequence() void: " + e);
            }
        }

        /// <summary>
        /// Ends the game and gives the winner the pot
        /// </summary>
        public void EndGame(int Winner)
        {
            try
            {
                int Prize = this.PlayerList.Values.Sum(x => x.CurrentBet);

                if (this.PlayerList[Winner].UserId > 0)
                {
                    GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(this.PlayerList[Winner].UserId);

                    if (Client != null && Client.GetRoleplay() != null && Client.GetHabbo() != null)
                    {
                        SendWinnerMessage("O vencedor é: " + Client.GetHabbo().Username + ", ganhou R$" + String.Format("{0:N0}", Prize) + "!");

                        Client.GetHabbo().Credits += Prize;
                        Client.GetHabbo().UpdateCreditsBalance();
                    }
                }

                RemovePotFurni();

                lock (this.PlayerList)
                {
                    foreach (var Player in this.PlayerList.Values)
                    {
                        if (Player.UserId > 0)
                            RemovePlayerFromGame(Player.UserId);
                    }
                }

                ResetGame();
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in FinalSequence() void: " + e);
            }
        }

        /// <summary>
        /// Resets the game for new players to play
        /// </summary>
        public void ResetGame()
        {
            foreach (var Item in this.Player1.Values)
            {
                if (Item.Furni != null)
                {
                    Item.Furni.ExtraData = "0";
                    Item.Furni.UpdateState(false, true);
                }
                Item.Rolled = false;
                Item.Value = 0;
            }
            foreach (var Item in this.Player2.Values)
            {
                if (Item.Furni != null)
                {
                    Item.Furni.UpdateCounter = 0;
                    Item.Furni.ExtraData = "0";
                    Item.Furni.UpdateState(false, true);
                }
                Item.Rolled = false;
                Item.Value = 0;
            }
            foreach (var Item in this.Player3.Values)
            {
                if (Item.Furni != null)
                {
                    Item.Furni.UpdateCounter = 0;
                    Item.Furni.ExtraData = "0";
                    Item.Furni.UpdateState(false, true);
                }
                Item.Rolled = false;
                Item.Value = 0;
            }
            foreach (var Item in this.Banker.Values)
            {
                if (Item.Furni != null)
                {
                    Item.Furni.UpdateCounter = 0;
                    Item.Furni.ExtraData = "0";
                    Item.Furni.UpdateState(false, true);
                }
                Item.Rolled = false;
                Item.Value = 0;
            }

            PlayerList[1] = null;
            PlayerList[2] = null;
            PlayerList[3] = null;

            this.GameSequence = 0;
            this.PlayersTurn = 0;
            this.GameStarted = false;
            this.TimerManager.EndAllTimers();
        }

        /// <summary>
        /// Sends a whisper to all players in the room
        /// </summary>
        public void SendWinnerMessage(string Message)
        {
            try
            {
                Room Room = RoleplayManager.GenerateRoom(this.RoomId);

                if (Room != null && Room.GetRoomUserManager() != null && Room.GetRoomUserManager().GetRoomUsers() != null)
                {
                    lock (Room.GetRoomUserManager().GetRoomUsers())
                    {
                        foreach (var User in Room.GetRoomUserManager().GetRoomUsers())
                        {
                            if (User == null || User.IsBot || User.GetClient() == null)
                                continue;

                            User.GetClient().SendWhisper(Message, 38);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in SendStartMessage() void: " + e);
            }
        }
    }
}
