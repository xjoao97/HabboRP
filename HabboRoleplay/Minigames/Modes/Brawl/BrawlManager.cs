using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Games.Modes.Brawl
{
    public class BrawlManager
    {
        public Brawl Game;

        public void Initialize(Brawl game)
        {
            this.Game = game;
            SetPrize();
        }

        public void SetPrize()
        {
            Game.Prize = Convert.ToInt32(RoleplayData.GetData("brawl", "prize"));
        }
    }
}