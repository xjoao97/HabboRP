using System;
using Plus.HabboHotel.GameClients;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Games
{
    public interface IGame
    {
        bool Finished();
        string GetName();
        void Start();
        void Stop();
        void Check();
        bool AddPlayerToGame(GameClient player, RoleplayTeam team);
        void RemovePlayerFromGame(GameClient player, bool Winner = false);
        void RemoveTeamMembers(RoleplayTeam Team);
        bool CanJoinTeam(RoleplayTeam Team);
        void NotifyPlayers(string Message);
        int GetPlayerCount();
        int GetMaxPlayers();
        bool HasGameStarted();
        bool IsGameStarting();
        GameMode GetGameMode();
        RoleplayTeam GetTeam(string teamName);
        Dictionary<string, RoleplayTeam> GetTeams();
        List<int> GetPlayers();
        List<RoomUser> GetBots();
    }
}
