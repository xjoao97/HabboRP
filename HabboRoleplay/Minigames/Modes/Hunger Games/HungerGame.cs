using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Core;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Games.Modes.HungerGames
{
    public class HungerGames : IGame
    {
        private int MaxPlayers;
        private GameMode GameMode;
        private bool GameStarted;
        private HungerGameManager Manager;
        private List<int> Players;
        public Dictionary<string, RoleplayTeam> Teams;

        private List<RoleplayTeam> LosingTeams = new List<RoleplayTeam>();
        private RoleplayTeam WinningTeam = null;
        public int Prize = 0;
        public bool finished = false;

        public HungerGames(int maxPlayers, GameMode gameMode)
        {
            this.MaxPlayers = maxPlayers;
            this.GameMode = gameMode;
            this.GameStarted = false;

            this.Players = new List<int>();
            this.Teams = new Dictionary<string, RoleplayTeam>();
            this.Manager = new HungerGameManager();

            Manager.Initialize(this);
        }

        public bool Finished()
        {
            if (finished == true)
                return true;

            return false;
        }

        public string GetName()
        {
            return "Hunger Games";
        }

        public void Check()
        {

        }

        public void Start()
        {
            try
            {
                GameStarted = true;

                foreach (int playerid in Players)
                {
                    GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(playerid);
                    int Counter = 11;

                    new Thread(() =>
                    {
                        while (Counter > 0)
                        {
                            if (Player != null)
                            {
                                if (Counter == 11)
                                    Player.SendWhisper("[Jogos Vorazes] vai começar em alguns segundos!", 1);
                                else
                                    Player.SendWhisper("[ATENÇÃO] Começará em " + Counter + " segundos!", 1);
                            }
                            Counter--;
                            Thread.Sleep(1000);

                            if (Counter == 0)
                                RoleplayManager.SendUser(Player, Player.GetRoleplay().Team.SpawnRoom);
                        }

                    }).Start();
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in Start() void: " + e);
            }
        }

        public void Stop()
        {

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
                player.GetHabbo().Motto = "[JOGOS VORAZES] - [Time: " + team.Name + "] ";
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
                if (Team == null || !Team.InGame)
                    return;

                Team.InGame = false;

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
                }

                if (Teams.ContainsKey(Team.Name))
                    Teams.Remove(Team.Name);
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in RemoveTeamMembers() void: " + e);
            }
        }

        public void NotifyPlayers(string Message)
        {

        }

        public bool CanJoinTeam(RoleplayTeam Team)
        {
            if (Manager.TeamCanBeJoined(Team))
                return true;
            else
                return false;
        }

        public void RemovePlayerFromGame(GameClient player, bool Winner = false)
        {
            try
            {
                if (player.GetRoleplay().Team != null && player.GetRoleplay() != null)
                {
                    player.GetRoleplay().Team.Members.Remove(player.GetHabbo().Id);
                    Players.Remove(player.GetHabbo().Id);

                    player.GetHabbo().Look = player.GetRoleplay().OriginalOutfit;
                    player.GetHabbo().Motto = player.GetRoleplay().Class;
                    player.GetHabbo().Poof(false);

                    player.GetRoleplay().GameSpawned = false;
                    player.GetRoleplay().Game = null;
                    player.GetRoleplay().Team = null;
                }
            }
            catch(Exception e)
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
            return false;
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
                    if (team.Name == teamName)
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