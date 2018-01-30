using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Games.Modes.TeamBrawl
{
    public class TeamBrawlManager
    {
        public TeamBrawl Game;

        public void Initialize(TeamBrawl game)
        {
            this.Game = game;
            GenerateTeams();
            SetPrize();
        }

        public void GenerateTeams()
        {
            string pinkTeamData = RoleplayData.GetData("teambrawl", "pink").ToString();
            string greenTeamData = RoleplayData.GetData("teambrawl", "green").ToString();
            string blueTeamData = RoleplayData.GetData("teambrawl", "blue").ToString();
            string yellowTeamData = RoleplayData.GetData("teambrawl", "yellow").ToString();

            RoleplayTeam pink = new RoleplayTeam("Rosa", pinkTeamData.Split(';'));
            RoleplayTeam green = new RoleplayTeam("Verde", greenTeamData.Split(';'));
            RoleplayTeam blue = new RoleplayTeam("Azul", blueTeamData.Split(';'));
            RoleplayTeam yellow = new RoleplayTeam("Amarelo", yellowTeamData.Split(';'));

            Game.Teams.Add(pink.Name, pink);
            Game.Teams.Add(green.Name, green);
            Game.Teams.Add(blue.Name, blue);
            Game.Teams.Add(yellow.Name, yellow);
        }

        public void SetPrize()
        {
            Game.Prize = Convert.ToInt32(RoleplayData.GetData("teambrawl", "prize"));
        }

        public RoleplayTeam GetTeam(string name)
        {
            if (!Game.Teams.ContainsKey(name))
                return null;

            return Game.Teams[name];
        }

        public bool TeamCanBeJoined(RoleplayTeam team)
        {
            if (team.Members.Count >= Convert.ToInt32(RoleplayData.GetData("teambrawl", "maxusersperteam")))
                return false;

            return true;
        }
    }
}