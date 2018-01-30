using Plus.HabboRoleplay.Games;
using Plus.HabboRoleplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Minigames.Modes.MafiaWars
{
    public class MafiaWarsManager
    {
        public MafiaWars Game;

        public void Initialize(MafiaWars game)
        {
            this.Game = game;
            GenerateTeams();
            SetPrize();
        }

        public void GenerateTeams()
        {
            string greenTeamData = RoleplayData.GetData("mafiawars", "green").ToString();
            string blueTeamData = RoleplayData.GetData("mafiawars", "blue").ToString();

            RoleplayTeam green = new RoleplayTeam("Verde", greenTeamData.Split(';'));
            RoleplayTeam blue = new RoleplayTeam("Azul", blueTeamData.Split(';'));

            Game.Teams.Add(green.Name, green);
            Game.Teams.Add(blue.Name, blue);
        }

        public void SetPrize()
        {
            Game.Prize = Convert.ToInt32(RoleplayData.GetData("mafiawars", "prize"));
        }

        public RoleplayTeam GetTeam(string name)
        {
            if (!Game.Teams.ContainsKey(name))
                return null;

            return Game.Teams[name];
        }

        public bool TeamCanBeJoined(RoleplayTeam team)
        {
            if (team.Members.Count >= Convert.ToInt32(RoleplayData.GetData("mafiawars", "maxusersperteam")))
                return false;

            return true;
        }


    }
}
