using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboRoleplay.Misc;
using Plus.Core;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Games.Modes.SoloQueue
{
    public class SoloQueue : IGame
    {
        private int MaxPlayers;
        private GameMode GameMode;
        private bool GameStarted;
        private bool GameIsStarting;
        private List<int> Players;
        public bool finished = false;

        public SoloQueue(int maxPlayers, GameMode gameMode)
        {
            this.MaxPlayers = maxPlayers;
            this.GameMode = gameMode;
            this.GameStarted = false;
            this.GameIsStarting = false;
            this.Players = new List<int>();
        }

        public bool Finished()
        {
            return finished;
        }

        public string GetName()
        {
            return "SoloQueue";
        }

        public void Start()
        {
            try
            {
                GameIsStarting = true;

                lock (Players)
                {
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
                                        Player.SendWhisper("[SoloQueue] vai começar em alguns segundos!", 1);
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
                                            Player.SendWhisper("Evento iniciado! Mete a porrada!", 1);
                                            Player.GetRoleplay().ReplenishStats(true);
                                        }
                                    }
                                    GameStarted = true;
                                }
                            }

                        }).Start();
                    }
                }
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in Start() void: " + e);
            }
        }

        public void Stop()
        {
            finished = true;
        }

        public void Winner()
        {
            try
            {
                GameClient Player = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Players[0]);

                if (Player != null)
                {
                    Player.GetRoleplay().UpdateEventWins("sq", 1);
                    if (Player.GetRoleplay() != null)
                        Player.GetRoleplay().ReplenishStats(true);
                    Player.SendNotification("Parabéns! Você ganhou o SoloQueue!");
                    RoleplayManager.SpawnChairs(Player, "es_bench");
                    RemovePlayerFromGame(Player);

                    if (Player.GetHabbo() != null && Player.GetHabbo().CurrentRoom != null && Player.GetHabbo().CurrentRoom.GetRoomUserManager() != null && Player.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers() != null)
                    {
                        lock (Player.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                        {
                            foreach (var User in Player.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUsers())
                            {
                                if (User == null || User.IsBot || User.GetClient() == null)
                                    continue;

                                User.GetClient().SendWhisper("[Alerta de EVENTO] " + Player.GetHabbo().Username + " acabou de ganhar o evento [SoloQueue]! Parabéns!", 33);
                            }
                        }
                    }
                }
                Stop();
            }
            catch(Exception e)
            {
                Logging.LogRPGamesError("Error in Winner() void: " + e);
            }
        }

        public void Check()
        {
            try
            {
                if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.SoloQueue))
                {
                    #region Logged/Kicked Check
                    if (Players.Count > 0)
                    {
                        foreach (int playerId in Players)
                        {
                            var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(playerId);
                            if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null || Client.GetRoomUser() == null || Client.GetRoomUser().RoomId <= 0 || Client.GetHabbo().CurrentRoom == null || Client.GetHabbo().CurrentRoomId <= 0)
                            {
                                Players.Remove(playerId);
                                break;
                            }
                            else if (Client.GetRoleplay().Game != this)
                            {
                                Players.Remove(playerId);
                                break;
                            }
                        }
                    }
                    #endregion

                    #region Main Checks
                    if (GameStarted && Players.Count <= 1)
                    {
                        if (Players.Count == 1)
                            Winner();
                        else
                            Stop();
                    }
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

        public bool AddPlayerToGame(GameClient player, RoleplayTeam team)
        {
            try
            {
                if (Players.Contains(player.GetHabbo().Id))
                    return false;

                Players.Add(player.GetHabbo().Id);
                player.GetHabbo().Motto = "[SoloQueue]";
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