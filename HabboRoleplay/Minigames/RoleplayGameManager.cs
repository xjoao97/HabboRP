using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboRoleplay.Games.Modes.HungerGames;
using Plus.HabboRoleplay.Games.Modes.Brawl;
using Plus.HabboRoleplay.Games.Modes.ColourWars;
using Plus.HabboRoleplay.Games.Modes.TeamBrawl;
using Plus.HabboRoleplay.Games.Modes.SoloQueue;
using Plus.HabboRoleplay.Minigames.Modes.MafiaWars;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using log4net;
using System.Drawing;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboRoleplay.Games
{
    public static class RoleplayGameManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Games.RoleplayGameManager");

        /// <summary>
        /// Thread-safe dictionary containing all auto-mated minigame events
        /// </summary>
        public static ConcurrentDictionary<DayOfWeek, List<AutomaticGame>> AutomatedGames = new ConcurrentDictionary<DayOfWeek, List<AutomaticGame>>();

        /// <summary>
        /// Gametick thread
        /// </summary>
        private static Thread GameTick;

        /// <summary>
        /// Dictionary containing all running games
        /// </summary>
        public static Dictionary<GameMode, IGame> RunningGames;

        /// <summary>
        /// Starts the game manager
        /// </summary>
        public static void Initialize()
        {
            AutomatedGames.Clear();

            RunningGames = new Dictionary<GameMode, IGame>();
            AutomatedGames.TryAdd(DayOfWeek.Monday, new List<AutomaticGame>());
            AutomatedGames.TryAdd(DayOfWeek.Tuesday, new List<AutomaticGame>());
            AutomatedGames.TryAdd(DayOfWeek.Wednesday, new List<AutomaticGame>());
            AutomatedGames.TryAdd(DayOfWeek.Thursday, new List<AutomaticGame>());
            AutomatedGames.TryAdd(DayOfWeek.Friday, new List<AutomaticGame>());
            AutomatedGames.TryAdd(DayOfWeek.Saturday, new List<AutomaticGame>());
            AutomatedGames.TryAdd(DayOfWeek.Sunday, new List<AutomaticGame>());
            GenerateAutomatedGames();

            //log.Info("Gerenciados de Eventos -> CARREGADO!");
        }

        /// <summary>
        /// Creates a game based on the string gameMode
        /// </summary>
        /// <param name="gameMode"></param>
        /// <returns></returns>
        public static IGame CreateGame(string gameMode)
        {
            GameMode mode = GameList.GetGameModeType(gameMode);
            IGame game = GetNewGame(mode);

            if (game == null || RunningGames.ContainsKey(mode))
                return null;

            string EventData = RoleplayData.GetData("eventslobby", gameMode);
            if (EventData != null)
            {
                int EventsLobby = Convert.ToInt32(EventData);
                var EventsRoom = RoleplayManager.GenerateRoom(EventsLobby);

                if (EventsRoom != null)
                {
                    EventsRoom.RoleplayEvent = game;
                    EventsRoom.RoomData.RoleplayEvent = EventsRoom.RoleplayEvent;
                    PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(EventsRoom);
                }
            }

            RunningGames.Add(mode, game);

            if (GameTick == null || GameTick.ThreadState != ThreadState.Running)
            {
                GameTick = new Thread(TickGames);
                GameTick.Start();
            }

            return game;
        }

        /// <summary>
        /// Creates a game based on the GameMode gameMode
        /// </summary>
        /// <param name="gameMode"></param>
        /// <returns></returns>
        public static IGame CreateGame(GameMode mode)
        {
            IGame game = GetNewGame(mode);

            if (game == null || RunningGames.ContainsKey(mode))
                return null;

            RunningGames.Add(mode, game);

            if (GameTick == null || GameTick.ThreadState != ThreadState.Running)
            {
                GameTick = new Thread(TickGames);
                GameTick.Start();
            }

            return game;
        }

        /// <summary>
        ///  Stops a game based on the gamemode
        /// </summary>
        /// <param name="Mode"></param>
        public static void StopGame(GameMode Mode)
        {
            RunningGames.Remove(Mode);
        }

        /// <summary>
        /// Adds a player to the game
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public static string AddPlayerToGame(IGame game, GameClient player, string teamName, bool Forced = false)
        {
            bool gameStarted = game.HasGameStarted();
            int maxPlayers = game.GetMaxPlayers();
            int players = game.GetPlayerCount();
            RoleplayTeam team = game.GetTeam(teamName);

            if (!Forced)
            {
                if (gameStarted)
                    return "GAMESTARTED";
                if (players >= maxPlayers)
                    return "MAXPLAYERS";

                if (game.GetGameMode() != GameMode.Brawl && game.GetGameMode() != GameMode.SoloQueue && game.GetGameMode() != GameMode.SoloQueueGuns)
                {
                    if (team == null)
                        return "TEAMNULL";
                    if (!game.CanJoinTeam(team))
                        return "TEAMFULL";
                }
            }

            bool HasPlayerBeenAdded = game.AddPlayerToGame(player, team);

            if (!HasPlayerBeenAdded)
                return "ERROR";

            if (game.GetGameMode() == GameMode.Brawl || game.GetGameMode() == GameMode.SoloQueue || game.GetGameMode() == GameMode.SoloQueueGuns)
                player.SendWhisper("Você se juntou ao Evento " + game.GetName() + "!", 1);
            else
                player.SendWhisper("Você se juntou ao Time " + team.Name + " no Evento " + game.GetName() + "!", 1);
            player.GetRoleplay().Game = game;
            player.GetRoleplay().Team = team;

            if (Forced)
                player.GetRoleplay().ReplenishStats();

            if (game.GetGameMode() != GameMode.Brawl && game.GetGameMode() != GameMode.SoloQueue && game.GetGameMode() != GameMode.SoloQueueGuns)
                player.GetRoleplay().Team.SendToPoint(player);

            return "OK";
        }

        /// <summary>
        /// Runs the gametick to check for running games
        /// </summary>
        public static void TickGames()
        {
            while (RunningGames.Count > 0)
            {
                try
                {
                    lock (RunningGames)
                    {
                        foreach (var game in RunningGames)
                        {
                            if (game.Value == null)
                                continue;

                            if (game.Value.Finished())
                            {
                                #region Reset Colour Wars Gates
                                if (game.Value.GetGameMode() == GameMode.ColourWars)
                                {
                                    var Room = RoleplayManager.GenerateRoom(Convert.ToInt32(RoleplayData.GetData("colourwars", "lobbyid")));

                                    if (Room != null)
                                    {
                                        foreach (var item in Room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigateblue || x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigategreen || x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigatered || x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigateyellow).ToList())
                                        {
                                            if (item != null)
                                            {
                                                item.ExtraData = "0";
                                                item.UpdateState();

                                                if (item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] == 0)
                                                {
                                                    foreach (var sser in item.GetRoom().GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                                    {
                                                        sser.SqState = 1;
                                                    }
                                                    item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Reset Team Brawl Gates
                                if (game.Value.GetGameMode() == GameMode.TeamBrawl)
                                {
                                    var Room = RoleplayManager.GenerateRoom(Convert.ToInt32(RoleplayData.GetData("teambrawl", "lobbyid")));

                                    if (Room != null)
                                    {
                                        foreach (var item in Room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigateblue || x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigategreen || x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigatered || x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigateyellow).ToList())
                                        {
                                            if (item != null)
                                            {
                                                item.ExtraData = "0";
                                                item.UpdateState();

                                                if (item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] == 0)
                                                {
                                                    foreach (var sser in item.GetRoom().GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                                    {
                                                        sser.SqState = 1;
                                                    }
                                                    item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Reset Mafia Wars Gates
                                if (game.Value.GetGameMode() == GameMode.MafiaWars)
                                {
                                    var Room = RoleplayManager.GenerateRoom(Convert.ToInt32(RoleplayData.GetData("mafiawars", "lobbyid")));

                                    if (Room != null)
                                    {
                                        foreach (var item in Room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigateblue || x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.banzaigategreen).ToList())
                                        {
                                            if (item != null)
                                            {
                                                item.ExtraData = "0";
                                                item.UpdateState();

                                                if (item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] == 0)
                                                {
                                                    foreach (var sser in item.GetRoom().GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                                                    {
                                                        sser.SqState = 1;
                                                    }
                                                    item.GetRoom().GetGameMap().GameMap[item.GetX, item.GetY] = 1;
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                StopGame(game.Value.GetGameMode());
                                break;
                            }
                            else
                            {
                                game.Value.Check();
                            }
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// Generates a new game
        /// </summary>
        /// <param name="gameMode"></param>
        /// <returns></returns>
        private static IGame GetNewGame(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.HungerGames:
                    return new HungerGames(Convert.ToInt32(RoleplayData.GetData("hungergames", "usersrequired")), GameMode.HungerGames);

                case GameMode.Brawl:
                    return new Brawl(Convert.ToInt32(RoleplayData.GetData("brawl", "usersrequired")), GameMode.Brawl);

                case GameMode.ColourWars:
                    return new ColourWars(Convert.ToInt32(RoleplayData.GetData("colourwars", "usersrequired")), GameMode.ColourWars);

                case GameMode.MafiaWars:
                    return new MafiaWars(Convert.ToInt32(RoleplayData.GetData("mafiawars", "usersrequired")), GameMode.MafiaWars);

                case GameMode.TeamBrawl:
                    return new TeamBrawl(Convert.ToInt32(RoleplayData.GetData("teambrawl", "usersrequired")), GameMode.TeamBrawl);

                case GameMode.SoloQueue:
                    return new SoloQueue(2, GameMode.SoloQueue);

                case GameMode.SoloQueueGuns:
                    return new SoloQueueGuns(2, GameMode.SoloQueueGuns);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets a game based on gamemode from the runninggames dictionary
        /// </summary>
        /// <param name="gameMode"></param>
        /// <returns></returns>
        public static IGame GetGame(GameMode gameMode)
        {
            if (!RunningGames.ContainsKey(gameMode))
                return null;

            return RunningGames[gameMode];
        }

        /// <summary>
        /// Generates the automatic games from database
        /// </summary>
        public static void GenerateAutomatedGames()
        {
            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_automated_events` WHERE `enabled` = '1'");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        string gameMode = Row["event"].ToString();
                        GameMode Mode = GameList.GetGameModeType(gameMode);

                        if (Mode != GameMode.None)
                        {
                            string Day = Row["day"].ToString().ToLower();

                            DayOfWeek DayOfWeek;
                            if (!TryGetDayOfWeek(Day, out DayOfWeek))
                                continue;

                            int Hour = Convert.ToInt32(Row["time"].ToString().Split(':')[0]);
                            int Minute = Convert.ToInt32(Row["time"].ToString().Split(':')[1]);

                            AutomaticGame Game = new AutomaticGame(Mode, Hour, Minute);
                            List<AutomaticGame> List = AutomatedGames[DayOfWeek];

                            if (!List.Contains(Game))
                            {
                                List.Add(Game);
                                AutomatedGames[DayOfWeek] = List;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the automated games to start their event
        /// </summary>
        public static void CheckAutomaticGames()
        {
            DateTime TimeNow = DateTime.Now;
            DayOfWeek DayOfWeek = TimeNow.DayOfWeek;
            TimeSpan TimeOfDay = TimeNow.TimeOfDay;

            List<AutomaticGame> Games = AutomatedGames[DayOfWeek];

            foreach (AutomaticGame Game in Games)
            {
                if (Game.Activated)
                    continue;

                if (TimeOfDay.Hours == Game.Hour && TimeOfDay.Minutes == Game.Minute)
                {
                    IGame IGame = CreateGame(Game.Mode);

                    if (IGame != null)
                        EventAlert(IGame);
                    Game.Activated = true;
                }
            }
        }

        /// <summary>
        /// Tries to get the DayOfWeek enum based on the string
        /// </summary>
        /// <param name="Day"></param>
        /// <param name="DayOfWeek"></param>
        /// <returns></returns>
        public static bool TryGetDayOfWeek(string Day, out DayOfWeek DayOfWeek)
        {
            switch (Day.ToLower())
            {
                case "monday":
				case "segunda":
                    {
                        DayOfWeek = DayOfWeek.Monday;
                        return true;
                    }
                case "tuesday":
				case "terca":
                    {
                        DayOfWeek = DayOfWeek.Tuesday;
                        return true;
                    }
                case "wednesday":
				case "quarta":
                    {
                        DayOfWeek = DayOfWeek.Wednesday;
                        return true;
                    }
                case "thursday":
				case "quinta":
                    {
                        DayOfWeek = DayOfWeek.Thursday;
                        return true;
                    }
                case "friday":
				case "sexta":
                    {
                        DayOfWeek = DayOfWeek.Friday;
                        return true;
                    }
                case "saturday":
				case "sabado":
                    {
                        DayOfWeek = DayOfWeek.Saturday;
                        return true;
                    }
                case "sunday":
				case "domingo":
                    {
                        DayOfWeek = DayOfWeek.Sunday;
                        return true;
                    }
                default:
                    {
                        DayOfWeek = DayOfWeek.Monday;
                        return false;
                    }
            }
        }

        /// <summary>
        /// Sends out a hotel alert based on the gamemode
        /// </summary>
        /// <param name="Mode"></param>
        public static void EventAlert (IGame Game)
        {
            int GameRoom;
            if (int.TryParse(RoleplayData.GetData("eventslobby", Game.GetName().Replace(" ", "").ToLower()), out GameRoom))
            {
                var Room = RoleplayManager.GenerateRoom(GameRoom);

                if (Room != null)
                    PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer(null, "event", "", 0, Game.GetGameMode(), Room));
            }
        }
    }
}
