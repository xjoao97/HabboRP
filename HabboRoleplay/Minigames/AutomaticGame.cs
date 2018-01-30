using System;
using System.Linq;
using System.Text;

namespace Plus.HabboRoleplay.Games
{
    public class AutomaticGame
    {
        public GameMode Mode;
        public int Hour;
        public int Minute;
        public bool Activated;

        public AutomaticGame(GameMode Mode, int Hour, int Minute)
        {
            this.Mode = Mode;
            this.Hour = Hour;
            this.Minute = Minute;
            this.Activated = false;
        }
    }
}
