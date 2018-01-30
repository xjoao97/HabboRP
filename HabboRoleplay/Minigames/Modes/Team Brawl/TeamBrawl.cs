using System;
using System.Linq;
using System.Threading;
using Plus.Utilities;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Core;
using Plus.HabboHotel.Pathfinding;
using System.Drawing;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Games.Modes.TeamBrawl
{
    public class TeamBrawl : IGame
    {
        CryptoRandom Random = new CryptoRandom();
        private int MaxPlayers;
        private GameMode GameMode;
        private bool GameStarted;
        public TeamBrawlManager Manager;
        private List<int> Players;
        public Dictionary<string, RoleplayTeam> Teams;
        public int Prize = 0;

        private bool finished = false;
        private bool GameIsStarting = false;
        private bool finaltwo = false;
        private int Counter = 0;

        private int ArenaStartX = Convert.ToInt32(RoleplayData.GetData("teambrawlarena", "startx"));
        private int ArenaStartY = Convert.ToInt32(RoleplayData.GetData("teambrawlarena", "starty"));
        private int ArenaFinishX = Convert.ToInt32(RoleplayData.GetData("teambrawlarena", "finishx"));
        private int ArenaFinishY = Convert.ToInt32(RoleplayData.GetData("teambrawlarena", "finishy"));

        public TeamBrawl(int maxPlayers, GameMode gameMode)
        {
            this.MaxPlayers = maxPlayers;
            this.GameMode = gameMode;
            this.GameStarted = false;

            this.Players = new List<int>();
            this.Teams = new Dictionary<string, RoleplayTeam>();
            this.Manager = new TeamBrawlManager();

            Manager.Initialize(this);
        }

        public bool Finished()
        {
            return finished;
        }

        public string GetName()
        {
            return "Team Brawl";
        }

        public void Start()
        {
            try
            {
                GameIsStarting = true;

                int UserCounter = 0;

                lock (Players)
                {
                    foreach (int playerid in Players)
                    {
                        GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(playerid);
                        int Counter = 11;
                        UserCounter++;

                        new Thread(() =>
                        {
                            while (Counter > 0)
                            {
                                if (Player != null)
                                {
                                    if (Counter == 11)
                                        Player.SendWhisper("[Briga de Times] vai começar em alguns segundos!", 1);
                                    else
                                        Player.SendWhisper("[ATENÇÃO] Começará em " + Counter + " segundos!", 1);
                                }
                                Counter--;
                                Thread.Sleep(1000);

                                if (Counter == 0)
                                {
                                    if (Player.GetRoomUser() != null)
                                    {
                                        List<ThreeDCoord> Squares = RoleplayManager.GenerateMap(ArenaStartX, ArenaStartY, ArenaFinishX, ArenaFinishY);
                                        ThreeDCoord RandomSquare = Squares[Random.Next(0, Squares.Count)] == null ? Squares.FirstOrDefault() : Squares[Random.Next(0, Squares.Count)];

                                        var RoomUser = Player.GetRoomUser();
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

                                        RoomUser.ClearMovement(true);
                                        var Room = RoleplayManager.GenerateRoom(Player.GetRoomUser().RoomId);
                                        if (Room != null)
                                            Room.GetGameMap().UpdateUserMovement(new Point(Player.GetRoomUser().X, Player.GetRoomUser().Y), new Point(RandomSquare.X, RandomSquare.Y), Player.GetRoomUser());

                                        Player.GetRoomUser().X = RandomSquare.X;
                                        Player.GetRoomUser().Y = RandomSquare.Y;
                                        Player.GetRoomUser().UpdateNeeded = true;
                                    }

                                    if (Player.GetRoleplay() != null)
                                    {
                                        Player.GetRoleplay().ReplenishStats();
                                    }
                                    GameStarted = true;
                                    GameIsStarting = false;
                                }
                            }
                        }).Start();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in Start() void: " + e);
            }
        }

        public void Stop()
        {
            try
            {
                finished = true;

                var Rooms = PlusEnvironment.GetGame().GetRoomManager().GetRooms().Where(x => x.RoleplayEvent != null && x.RoleplayEvent.GetGameMode() == GameMode.TeamBrawl).ToList();

                if (Rooms.Count > 0)
                {
                    lock (Rooms)
                    {
                        foreach (var Room in Rooms)
                        {
                            Room.RoleplayEvent = null;
                            Room.RoomData.RoleplayEvent = null;
                            PlusEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in Stop() void: " + e);
            }
        }

        public void Check()
        {
            try
            {
                if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.TeamBrawl))
                {
                    #region Room Notification
                    if (1==1 /*!GameIsStarting && GameStarted*/)
                    {
                        Counter++;

                        if (Counter >= 150)
                        {
                            Counter = 0;
                            int RoomId = Convert.ToInt32(RoleplayData.GetData("teambrawl", "lobbyid"));
                            int MaxUsersPerTeam = Convert.ToInt32(RoleplayData.GetData("teambrawl", "maxusersperteam"));

                            var Room = RoleplayManager.GenerateRoom(RoomId);

                            if (Room != null && Room.GetRoomUserManager() != null && Room.GetRoomUserManager().GetRoomUsers() != null)
                            {
                                RoleplayTeam Pink = null;
                                RoleplayTeam Green = null;
                                RoleplayTeam Blue = null;
                                RoleplayTeam Yellow = null;

                                if (Teams.ContainsKey("Pink"))
                                    Pink = Teams["Rosa"];
                                if (Teams.ContainsKey("Verde"))
                                    Green = Teams["Green"];
                                if (Teams.ContainsKey("Azul"))
                                    Blue = Teams["Blue"];
                                if (Teams.ContainsKey("Yellow"))
                                    Yellow = Teams["Amarelo"];

                                lock (Room.GetRoomUserManager().GetRoomUsers())
                                {
                                    foreach (var User in Room.GetRoomUserManager().GetRoomUsers())
                                    {
                                        if (User == null || User.IsBot || User.GetClient() == null)
                                            continue;

                                        string Message = "Agora temos: " + (Pink == null ? "" : Pink.Members.Count + "/" + MaxUsersPerTeam + " Membros [ROSA], ") + (Green == null ? "" : Green.Members.Count + "/" + MaxUsersPerTeam + " Membros [VERDE], ") + (Blue == null ? "" : Blue.Members.Count + "/" + MaxUsersPerTeam + " Membros [AZUL], ") + (Yellow == null ? "" : Yellow.Members.Count + "/" + MaxUsersPerTeam + " Membros [AMARELO]").TrimEnd(',', ' ');
                                        User.GetClient().SendWhisper(Message, 34);
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region Logged/Kicked Check
                    if (Players.Count > 0 && !GameIsStarting)
                    {
                        List<int> PlayersToRemove = Players.Where(x => RoleplayManager.OfflineCheck(x, true, this)).ToList();
                        foreach (int playerId in PlayersToRemove)
                        {
                            List<RoleplayTeam> AllTeams = Teams.Values.ToList();
                            bool HasTeam = AllTeams.Where(x => x.Members.Contains(playerId)).ToList().Count > 0;
                            if (HasTeam)
                            {
                                RoleplayTeam Team = AllTeams.FirstOrDefault(x => x.Members.Contains(playerId));
                                if (Team != null)
                                    Team.Members.Remove(playerId);
                            }

                            Players.Remove(playerId);
                        }
                    }
                    #endregion

                    #region Main Checks
                    if (GameStarted && !GameIsStarting)
                    {
                        if (Teams.Count > 1)
                            TeamCheck();

                        List<RoleplayTeam> TeamsInGame = Teams.Values.Where(x => x.InGame == true).ToList();
                        if (TeamsInGame.Count <= 2)
                        {
                            if (TeamsInGame.Count == 2)
                            {
                                while (Teams.Count > TeamsInGame.Count)
                                {
                                    foreach (var LostTeam in Teams.Values)
                                    {
                                        if (!LostTeam.InGame)
                                        {
                                            RemoveTeamMembers(LostTeam);
                                            break;
                                        }
                                    }
                                }
                                LastTwo(TeamsInGame[0], TeamsInGame[1]);
                            }
                            else if (TeamsInGame.Count == 1)
                            {
                                if (Teams.Values.Where(x => !x.InGame).ToList().Count > 0)
                                    RemoveTeamMembers(Teams.Values.Where(x => !x.InGame).FirstOrDefault());
                                Winner(TeamsInGame.FirstOrDefault());
                            }
                            else
                                Stop();
                        }
                    }

                    if (GameStarted && !GameIsStarting && Players.Count <= 0)
                        Stop();
                    else if (Players.Count == MaxPlayers && !GameStarted && !GameIsStarting)
                        Start();
                    #endregion
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in Check() void: " + e);
            }
        }

        public void LastTwo(RoleplayTeam First, RoleplayTeam Second)
        {
            try
            {
                if (finaltwo)
                    return;

                finaltwo = true;

                First.LostMembers.Clear();
                Second.LostMembers.Clear();

                #region First Team
                foreach (int playerid in First.Members)
                {
                    GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(playerid);
                    int Counter = 6;

                    Player.GetRoleplay().ReplenishStats();

                    if (Player.GetRoomUser() != null)
                    {
                        List<ThreeDCoord> Squares = RoleplayManager.GenerateMap(ArenaStartX, ArenaStartY, ArenaFinishX, ArenaFinishY);
                        ThreeDCoord RandomSquare = Squares[Random.Next(0, Squares.Count)] == null ? Squares.FirstOrDefault() : Squares[Random.Next(0, Squares.Count)];

                        Player.GetRoomUser().Frozen = true;
                        Player.GetRoomUser().ClearMovement(true);
                        var Room = RoleplayManager.GenerateRoom(Player.GetRoomUser().RoomId);
                        if (Room != null)
                            Room.GetGameMap().UpdateUserMovement(new Point(Player.GetRoomUser().X, Player.GetRoomUser().Y), new Point(RandomSquare.X, RandomSquare.Y), Player.GetRoomUser());

                        Player.GetRoomUser().X = RandomSquare.X;
                        Player.GetRoomUser().Y = RandomSquare.Y;
                        Player.GetRoomUser().UpdateNeeded = true;
                    }
                    new Thread(() =>
                    {
                        while (Counter > 0)
                        {
                            if (Player != null)
                            {
                                if (Counter == 6)
                                    Player.SendWhisper("Sua equipe chegou às semi finais! Boa sorte!", 1);
                                else
                                    Player.SendWhisper("[Briga de Times] vai continuar em " + Counter + " segundos!", 1);
                            }
                            Counter--;
                            Thread.Sleep(1000);

                            if (Counter == 0)
                            {
                                Player.GetRoomUser().Frozen = false;
                                Player.GetRoomUser().ClearMovement(true);
                                Player.SendWhisper("Evento iniciado, mete a porrada neles!", 1);
                            }
                        }

                    }).Start();
                }
                #endregion

                #region Second Team
                foreach (int playerid in Second.Members)
                {
                    GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(playerid);
                    int Counter = 6;

                    Player.GetRoleplay().ReplenishStats();

                    if (Player.GetRoomUser() != null)
                    {
                        List<ThreeDCoord> Squares = RoleplayManager.GenerateMap(ArenaStartX, ArenaStartY, ArenaFinishX, ArenaFinishY);
                        ThreeDCoord RandomSquare = Squares[Random.Next(0, Squares.Count)] == null ? Squares.FirstOrDefault() : Squares[Random.Next(0, Squares.Count)];

                        Player.GetRoomUser().Frozen = true;
                        Player.GetRoomUser().ClearMovement(true);
                        var Room = RoleplayManager.GenerateRoom(Player.GetRoomUser().RoomId);
                        if (Room != null)
                            Room.GetGameMap().UpdateUserMovement(new Point(Player.GetRoomUser().X, Player.GetRoomUser().Y), new Point(RandomSquare.X, RandomSquare.Y), Player.GetRoomUser());

                        Player.GetRoomUser().X = RandomSquare.X;
                        Player.GetRoomUser().Y = RandomSquare.Y;
                        Player.GetRoomUser().UpdateNeeded = true;
                    }
                    new Thread(() =>
                    {
                        while (Counter > 0)
                        {
                            if (Player != null)
                            {
                                if (Counter == 6)
                                    Player.SendWhisper("Sua equipe chegou a final! Boa sorte!", 1);
                                else
                                    Player.SendWhisper("[Briga de Times] vai continuar em " + Counter + " segundos!", 1);
                            }
                            Counter--;
                            Thread.Sleep(1000);

                            if (Counter == 0)
                            {
                                Player.GetRoomUser().Frozen = false;
                                Player.GetRoomUser().ClearMovement(true);
                                Player.SendWhisper("Evento iniciado! Que comece a briga!", 1);
                            }
                        }

                    }).Start();
                }
                #endregion

            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in LastTwo() void: " + e);
            }
        }

        public void TeamCheck()
        {
            try
            {
                lock (Teams)
                {
                    foreach (var Team in Teams.Values)
                    {
                        if (Team.Members.Count <= 0)
                        {
                            Teams.Remove(Team.Name);
                            break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in TeamCheck() void: " + e);
            }
        }

        public void Winner(RoleplayTeam Team)
        {
            try
            {
                if (Team != null && Team.Members != null)
                {
                    while (Team.Members.Count > 0)
                    {
                        foreach (var Member in Team.Members)
                        {
                            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);

                            if (Client != null && Client.GetHabbo() != null && Client.GetRoleplay() != null)
                                RemovePlayerFromGame(Client, true);

                            if (Players.Contains(Member))
                                Players.Remove(Member);

                            if (Team.Members.Contains(Member))
                                Team.Members.Remove(Member);
                            break;
                        }

                        if (Team.Members.Count <= 0)
                        {
                            if (Teams.ContainsKey(Team.Name))
                                Teams.Remove(Team.Name);
                            break;
                        }
                    }

                    lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            if (client != null && client.GetHabbo() != null && client.GetRoleplay() != null)
                                client.SendWhisper("[Alerta de EVENTO] O Time [" + Team.Name + "] acabou de vencer o evento [Briga de Times]! Parabéns!", 33);
                        }
                    }
                }
                Stop();
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in Winner() void: " + e);
            }
        }

        public bool CanJoinTeam(RoleplayTeam Team)
        {
            if (Manager.TeamCanBeJoined(Team))
                return true;
            else
                return false;
        }

        public bool AddPlayerToGame(GameClient player, RoleplayTeam team)
        {
            try
            {
                if (player == null || player.GetHabbo() == null)
                    return false;

                if (team.Members.Contains(player.GetHabbo().Id))
                    return false;

                player.GetHabbo().Look = RoleplayManager.SplitFigure(player.GetHabbo().Look, team.Uniform);
                player.GetHabbo().Motto = "[Briga de Times] - [Time:" + team.Name + "]";
                player.GetHabbo().Poof(false);
                Players.Add(player.GetHabbo().Id);
                team.Members.Add(player.GetHabbo().Id);
                return true;
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in AddPlayerToGame() void: " + e);
                return false;
            }
        }

        public void RemoveTeamMembers(RoleplayTeam Team)
        {
            try
            {
                if (!Teams.ContainsKey(Team.Name))
                    return;

                if (Team != null && Team.Members != null)
                {
                    while (Team.Members.Count > 0)
                    {
                        foreach (var Member in Team.Members)
                        {
                            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);

                            if (Client != null && Client.GetHabbo() != null && Client.GetRoleplay() != null)
                            {
                                RemovePlayerFromGame(Client);
                                break;
                            }
                            else
                            {
                                Players.Remove(Member);
                                Team.Members.Remove(Member);
                                if (Team.LostMembers.Contains(Member))
                                    Team.LostMembers.Remove(Member);
                                break;
                            }
                        }

                        if (Team.Members.Count <= 0)
                        {
                            if (Teams.ContainsKey(Team.Name))
                                Teams.Remove(Team.Name);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in RemoveTeamMembers() void: " + e);
            }
        }

        public void NotifyPlayers(string Message)
        {
            try
            {
                lock (Players)
                {
                    foreach (var player in Players)
                    {
                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(player);

                        if (Client != null && Client.GetHabbo() != null && Client.GetRoleplay() != null)
                            Client.SendWhisper(Message, 34);
                    }
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in NotifyPlayers() void: " + e);
            }
        }

        public void RemovePlayerFromGame(GameClient player, bool Winner = false)
        {
            try
            {
                if (player == null || player.GetHabbo() == null || player.GetRoleplay() == null)
                    return;

                if (Players.Contains(player.GetHabbo().Id))
                    Players.Remove(player.GetHabbo().Id);
                else
                    return;

                if (Teams.Values.Where(x => x.Members.Contains(player.GetHabbo().Id)).ToList().Count > 0)
                {
                    RoleplayTeam Team = Teams.Values.FirstOrDefault(x => x.Members.Contains(player.GetHabbo().Id));
                    Team.Members.Remove(player.GetHabbo().Id);

                    if (Team.LostMembers.Contains(player.GetHabbo().Id))
                        Team.LostMembers.Remove(player.GetHabbo().Id);
                }

                player.GetHabbo().Look = player.GetRoleplay().OriginalOutfit;
                player.GetHabbo().Motto = player.GetRoleplay().Class;
                player.GetHabbo().Poof(false);

                player.GetRoleplay().GameSpawned = false;
                player.GetRoleplay().Game = null;
                player.GetRoleplay().Team = null;

                if (player.GetRoomUser() != null)
                    player.GetRoomUser().ClearMovement(true);
                RoleplayManager.SpawnChairs(player, "es_bench");

                if (GameStarted)
                {
                    if (Winner)
                    {
                        player.GetRoleplay().UpdateEventWins("brawl", 1);
                        player.GetRoleplay().ReplenishStats(true);
                        player.GetHabbo().EventPoints += Prize;
                        player.GetHabbo().UpdateEventPointsBalance();
                        player.SendNotification("Prabéns! Sua equipe ganhou, você foi premiado " + Prize + " pontos de eventos!");
                    }
                    else
                    {
                        player.GetHabbo().EventPoints++;
                        player.GetHabbo().UpdateEventPointsBalance();
                        player.SendNotification("Que merda hein, sua equipe perdeu no evento [Briga de Times]! Você ganhou 1 ponto de evento por participar!");
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in RemovePlayerFromGame() void: " + e);
            }
        }

        public int GetPlayerCount()
        {
            return Players.Count;
        }

        public int GetMaxPlayers()
        {
            return MaxPlayers;
        }

        public bool HasGameStarted()
        {
            return GameStarted;
        }

        public bool IsGameStarting()
        {
            return GameIsStarting;
        }

        public GameMode GetGameMode()
        {
            return GameMode;
        }

        public RoleplayTeam GetTeam(string teamName)
        {
            try
            {
                RoleplayTeam theteam = null;

                foreach (RoleplayTeam team in Teams.Values)
                {
                    if (team.Name.ToLower() == teamName.ToLower())
                    {
                        return team;
                    }
                }

                return theteam;
            }
            catch
            {
                return null;
            }
        }

        public Dictionary<string, RoleplayTeam> GetTeams()
        {
            return Teams;
        }

        public List<int> GetPlayers()
        {
            return Players;
        }

        public List<RoomUser> GetBots()
        {
            return new List<RoomUser>();
        }
    }
}