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
using Plus.HabboRoleplay.Games;
using System.Collections.Concurrent;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboRoleplay.Bots;

namespace Plus.HabboRoleplay.Minigames.Modes.MafiaWars
{
    public class MafiaWars : IGame
    {
        private int MaxPlayers;
        private GameMode GameMode;
        private bool GameStarted;
        public MafiaWarsManager Manager;
        private List<int> Players;
        public Dictionary<string, RoleplayTeam> Teams;
        public int Prize = 0;
        private int Counter = 0;

        private bool finished = false;
        private bool GameIsStarting = false;

        private ConcurrentDictionary<string, RoomUser> MafiaBots;

        public MafiaWars(int maxPlayers, GameMode gameMode)
        {

            this.MaxPlayers = maxPlayers;
            this.GameMode = gameMode;
            this.GameStarted = false;

            this.Players = new List<int>();
            this.Teams = new Dictionary<string, RoleplayTeam>();
            this.Manager = new MafiaWarsManager();

            this.MafiaBots = new ConcurrentDictionary<string, RoomUser>();
            this.InitializeMafiaBots();

            Manager.Initialize(this);

        }

        public bool Finished()
        {
            return finished;
        }

        public string GetName()
        {
            return "Mafia Wars";
        }

        public void Start()
        {
            try
            {

                this.InitializeMafiaBots();

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
                                        Player.SendWhisper("[Guerra de Máfias] vai começar em alguns segundos!", 1);
                                    else
                                        Player.SendWhisper("[ATENÇÃO] Começará em " + Counter + " segundos!", 1);
                                }
                                Counter--;
                                Thread.Sleep(1000);

                                if (Counter == 0)
                                {
                                    if (Player != null && Player.GetRoleplay() != null)
                                    {
                                        RoleplayManager.SendUser(Player, Player.GetRoleplay().Team.SpawnRoom);
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

                var Rooms = PlusEnvironment.GetGame().GetRoomManager().GetRooms().Where(x => x.RoleplayEvent != null && x.RoleplayEvent.GetGameMode() == GameMode.MafiaWars).ToList();

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

                this.ClearMafiaBots();
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in Stop() void: " + e);
            }
        }

        public void Check()
        {
            try
            {
                if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.MafiaWars))
                {
                    #region Room Notification
                    if (1 == 1 /*!GameIsStarting && GameStarted*/)
                    {
                        Counter++;

                        if (Counter >= 150)
                        {
                            Counter = 0;
                            int RoomId = Convert.ToInt32(RoleplayData.GetData("mafiawars", "lobbyid"));
                            int MaxUsersPerTeam = Convert.ToInt32(RoleplayData.GetData("mafiawars", "maxusersperteam"));

                            var Room = RoleplayManager.GenerateRoom(RoomId);

                            if (Room != null && Room.GetRoomUserManager() != null && Room.GetRoomUserManager().GetRoomUsers() != null)
                            {
                                RoleplayTeam Green = null;
                                RoleplayTeam Blue = null;

                                if (Teams.ContainsKey("Green"))
                                    Green = Teams["Verde"];
                                if (Teams.ContainsKey("Blue"))
                                    Blue = Teams["Azul"];

                                lock (Room.GetRoomUserManager().GetRoomUsers())
                                {
                                    foreach (var User in Room.GetRoomUserManager().GetRoomUsers())
                                    {
                                        if (User == null || User.IsBot || User.GetClient() == null)
                                            continue;

                                        string Message = "Agora temos: " + (Green == null ? "" : Green.Members.Count + "/" + MaxUsersPerTeam + " Membros [VERDE], ") + (Blue == null ? "" : Blue.Members.Count + "/" + MaxUsersPerTeam + " Membro [AZUL]").TrimEnd(',', ' ');
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
                    if (GameStarted && !GameIsStarting && Teams.Count > 1)
                        TeamCheck();
                    else if (GameStarted && !GameIsStarting && Teams.Count <= 1)
                    {
                        if (Teams.Count < 1)
                            Stop();
                        else
                            Winner(Teams.Values.FirstOrDefault());
                    }

                    if (GameStarted && !GameIsStarting && Players.Count <= 0)
                        Stop();
                    if (Players.Count == MaxPlayers && !GameStarted && !GameIsStarting)
                        Start();
                    #endregion
                }
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in Check() void: " + e);
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
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in Check() void: " + e);
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
                                client.SendWhisper("[Alerta de EVENTO] O Time " + Team.Name + " ganhou o evento [Guerra de Máfias]! Parabéns!", 33);
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
                player.GetHabbo().Motto = "[Guerra de Máfias] - [Time: " + team.Name + "]";
                player.GetHabbo().Poof(false);
                Players.Add(player.GetHabbo().Id);
                team.Members.Add(player.GetHabbo().Id);
                return true;
            }
            catch (Exception e)
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
            catch (Exception e)
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
                }

                player.GetHabbo().Look = player.GetRoleplay().OriginalOutfit;
                player.GetHabbo().Motto = player.GetRoleplay().Class;
                player.GetHabbo().Poof(false);

                player.GetRoleplay().GameSpawned = false;
                player.GetRoleplay().Game = null;
                player.GetRoleplay().Team = null;

                if (player.GetRoomUser() != null)
                    player.GetRoomUser().CanWalk = true;

                RoleplayManager.SendUser(player, Convert.ToInt32(RoleplayData.GetData("mafiawars", "lobbyid")));

                if (GameStarted)
                {
                    if (Winner)
                    {
                        player.GetRoleplay().UpdateEventWins("mw", 1);
                        player.GetRoleplay().ReplenishStats(true);
                        player.GetHabbo().EventPoints += Prize;
                        player.GetHabbo().UpdateEventPointsBalance();
                        player.SendNotification("Parabéns! Sua equipe ganhou, você foi premiado com " + Prize + " pontos de eventos!");
                    }
                    else
                    {
                        player.GetHabbo().EventPoints++;
                        player.GetHabbo().UpdateEventPointsBalance();
                        player.SendNotification("Que merda hein, sua equipe perdeu no evento [Guerra de Máfias]! Você ganhou 1 ponto de evento por participar!");
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
            return this.MafiaBots.Values.ToList();
        }

        private void ClearMafiaBots()
        {
            lock (this.MafiaBots)
            {
                if (this.MafiaBots.Count > 0)
                {

                    foreach (RoomUser User in this.MafiaBots.Values)
                    {
                        if (User == null)
                            continue;

                        RoleplayBotManager.EjectDeployedBot(User, User.GetRoom(), false);
                    }

                    this.MafiaBots.Clear();
                }
            }
        }

        private void InitializeMafiaBots()
        {

            this.ClearMafiaBots();

            #region Get Bots from Cache 

            List<RoleplayBot> MafiaBots = RoleplayBotManager.CachedRoleplayBots.Values.Where(RoleplayBot => RoleplayBot != null && RoleplayBot.AIType == RoleplayBotAIType.MAFIAWARS).ToList();

            RoleplayBot GreenBoss, BlueBoss, GreenThug1, GreenThug2, BlueThug1, BlueThug2;

            GreenBoss = MafiaBots.Where(MafiaWarsBot => MafiaWarsBot.Motto.ToLower().Contains("green") && MafiaWarsBot.Motto.ToLower().Contains("boss")).FirstOrDefault();
            BlueBoss = MafiaBots.Where(MafiaWarsBot => MafiaWarsBot.Motto.ToLower().Contains("blue") && MafiaWarsBot.Motto.ToLower().Contains("boss")).FirstOrDefault();

            GreenThug1 = MafiaBots.Where(MafiaWarsBot => MafiaWarsBot.Motto.ToLower().Contains("green") && !MafiaWarsBot.Motto.ToLower().Contains("boss")).First();
            GreenThug2 = MafiaBots.Where(MafiaWarsBot => MafiaWarsBot.Motto.ToLower().Contains("green") && !MafiaWarsBot.Motto.ToLower().Contains("boss")).Last();

            BlueThug1 = MafiaBots.Where(MafiaWarsBot => MafiaWarsBot.Motto.ToLower().Contains("blue") && !MafiaWarsBot.Motto.ToLower().Contains("boss")).First();
            BlueThug2 = MafiaBots.Where(MafiaWarsBot => MafiaWarsBot.Motto.ToLower().Contains("blue") && !MafiaWarsBot.Motto.ToLower().Contains("boss")).Last();

            GreenBoss.Dead = false;
            GreenBoss.Invisible = false;
            GreenThug1.Dead = false;
            GreenThug1.Invisible = false;
            GreenThug2.Dead = false;
            GreenThug2.Invisible = false;

            BlueBoss.Dead = false;
            BlueBoss.Invisible = false;
            BlueThug1.Dead = false;
            BlueThug1.Invisible = false;
            BlueThug2.Dead = false;
            BlueThug2.Invisible = false;

            #endregion

            #region Deploy bots
            RoleplayBotManager.DeployBotByID(GreenBoss.Id);
            RoleplayBotManager.DeployBotByID(BlueBoss.Id);

            RoleplayBotManager.DeployBotByID(GreenThug1.Id);
            RoleplayBotManager.DeployBotByID(GreenThug2.Id);

            RoleplayBotManager.DeployBotByID(BlueThug1.Id);
            RoleplayBotManager.DeployBotByID(BlueThug2.Id);
            #endregion

            #region Insert deployed bots into list
            new Thread(() => {
                Thread.Sleep(2000);
                this.MafiaBots.TryAdd("greenboss", RoleplayBotManager.GetDeployedBotById(GreenBoss.Id));
                this.MafiaBots.TryAdd("greenthug1", RoleplayBotManager.GetDeployedBotById(GreenBoss.Id));
                this.MafiaBots.TryAdd("greenthug2", RoleplayBotManager.GetDeployedBotById(GreenBoss.Id));

                this.MafiaBots.TryAdd("blueboss", RoleplayBotManager.GetDeployedBotById(GreenBoss.Id));
                this.MafiaBots.TryAdd("bluethug1", RoleplayBotManager.GetDeployedBotById(GreenBoss.Id));
                this.MafiaBots.TryAdd("bluethug2", RoleplayBotManager.GetDeployedBotById(GreenBoss.Id));
            }).Start();
            #endregion

        }

    }
}