using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Core;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Games.Modes.Brawl
{
    public class Brawl : IGame
    {
        private int MaxPlayers;
        private GameMode GameMode;
        private bool GameStarted;
        private BrawlManager Manager;
        private List<int> Players;
        public Dictionary<string, RoleplayTeam> Teams;
        bool GameIsStarting = false;

        public int Prize = 0;
        public bool finaltwo = false;
        public bool finished = false;

        public Brawl(int maxPlayers, GameMode gameMode)
        {
            this.MaxPlayers = maxPlayers;
            this.GameMode = gameMode;
            this.GameStarted = false;

            this.Players = new List<int>();
            this.Teams = new Dictionary<string, RoleplayTeam>();
            this.Manager = new BrawlManager();

            Manager.Initialize(this);
        }

        public bool Finished()
        {
            return finished;
        }

        public string GetName()
        {
            return "Brawl";
        }

        public void Start()
        {
            try
            {
                GameIsStarting = true;

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
                                    Player.SendWhisper("A [Briga] vai começar em alguns segundos!", 1);
                                else
                                    Player.SendWhisper("[ATENÇÃO] Começará em " + Counter + " segundos!", 1);
                            }
                            Counter--;
                            Thread.Sleep(1000);

                            if (Counter == 0)
                            {
                                if (Player != null)
                                {
                                    if (Player.GetRoleplay() != null)
                                    {
                                        Player.SendWhisper("EVENTO INICIADO [SE MATEM NA PORRADA]!", 1);
                                        Player.GetRoleplay().ReplenishStats();
                                        GameStarted = true;
                                        GameIsStarting = false;
                                    }
                                }
                            }
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
            try
            {
                finished = true;

                var Rooms = PlusEnvironment.GetGame().GetRoomManager().GetRooms().Where(x => x.RoleplayEvent != null && x.RoleplayEvent.GetGameMode() == GameMode.Brawl).ToList();

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

        public void Winner()
        {
            try
            {
                GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Players[0]);

                if (Player != null)
                {
                    Player.GetRoleplay().UpdateEventWins("brawl", 1);
                    if (Player.GetRoleplay() != null)
                        Player.GetRoleplay().ReplenishStats(true);
                    Player.GetHabbo().EventPoints += Prize;
                    Player.SendMessage(new ActivityPointsComposer(Player.GetHabbo().Duckets, Player.GetHabbo().Diamonds, Player.GetHabbo().EventPoints));
                    Player.SendNotification("Parabéns! Você ganhou o Evento [Briga]! Você foi premiado(a) com " + Prize + " Pontos de Eventos!");
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Player, "ACH_BrawlWins", 1);
                    RemovePlayerFromGame(Player);

                    foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null || client.GetHabbo() == null)
                            continue;

                        client.SendWhisper("[Alerta de EVENTO] " + Player.GetHabbo().Username + " ganhou o evento de [Briga]! Parabéns!", 33);
                    }
                }
                Stop();
            }
            catch (Exception e)
            {
                Logging.LogRPGamesError("Error in Winner() void: " + e);
            }
        }

        public void LastTwo()
        {
            try
            {
                if (finaltwo)
                    return;

                finaltwo = true;

                foreach (int playerid in Players)
                {
                    GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(playerid);
                    int Counter = 6;

                    Player.GetRoleplay().ReplenishStats();

                    if (Player.GetRoomUser() != null)
                    {
                        Player.GetRoomUser().Frozen = true;
                        Player.GetRoomUser().ClearMovement(true);
                    }
                    new Thread(() =>
                    {
                        while (Counter > 0)
                        {
                            if (Player != null)
                            {
                                if (Counter == 6)
                                    Player.SendWhisper("Você chegou aos dois últimos! Boa sorte!", 1);
                                else
                                    Player.SendWhisper("A briga irá continuar em " + Counter + " segundos!", 1);
                            }
                            Counter--;
                            Thread.Sleep(1000);

                            if (Counter == 0)
                            {
                                Player.GetRoomUser().Frozen = false;
                                Player.GetRoomUser().ClearMovement(true);
                                Player.SendWhisper("Briga iniciada!", 1);
                            }
                        }

                    }).Start();
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in LastTwo() void: " + e);
            }
        }

        public void Check()
        {
            try
            {
                if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.Brawl))
                {
                    #region Logged/Kicked Check
                    if (Players.Count > 0 && !GameIsStarting)
                    {
                        List<int> PlayersToRemove = Players.Where(x => RoleplayManager.OfflineCheck(x, true, this)).ToList();
                        foreach (int playerId in PlayersToRemove)
                        {
                            Players.Remove(playerId);
                        }
                    }
                    #endregion

                    if (GameStarted && !GameIsStarting && Players.Count <= 2)
                    {
                        if (Players.Count == 2)
                            LastTwo();
                        else if (Players.Count == 1)
                            Winner();
                        else
                            Stop();
                    }
                    else if (Players.Count == MaxPlayers && !GameStarted && !GameIsStarting)
                        Start();
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in Check() void: " + e);
            }
        }

        public bool AddPlayerToGame(GameClient player, RoleplayTeam team)
        {
            try
            {
                if (Players.Contains(player.GetHabbo().Id))
                    return false;

                Players.Add(player.GetHabbo().Id);
                player.GetHabbo().Motto = "[BRIGA]";
                player.GetHabbo().Poof(false);
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

        }

        public void NotifyPlayers(string Message)
        {

        }

        public bool CanJoinTeam(RoleplayTeam Team)
        {
            return true;
        }

        public void RemovePlayerFromGame(GameClient player, bool Winner = false)
        {
            try
            {
                if (player.GetRoleplay() == null)
                    return;

                if (player.GetRoleplay().Game == this)
                {
                    Players.Remove(player.GetHabbo().Id);

                    player.GetHabbo().Motto = player.GetRoleplay().Class;
                    player.GetHabbo().Poof(false);

                    player.GetRoleplay().Game = null;
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
            return GameIsStarting;
        }

        public GameMode GetGameMode()
        {
            return GameMode;
        }

        public RoleplayTeam GetTeam(string teamName)
        {
            return null;
        }

        public Dictionary<string, RoleplayTeam> GetTeams()
        {
            return null;
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