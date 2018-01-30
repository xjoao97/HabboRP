using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;

namespace Plus.HabboRoleplay.Cooldowns.Types
{
    /// <summary>
    /// Cooldown to let you shoot again
    /// </summary>
    public class GunCooldown : Cooldown
    {
        public GunCooldown(string Type, GameClient Client, int Time, int Amount) 
            : base(Type, Client, Time, Amount)
        {
            TimeLeft = 125;
        }
 
        /// <summary>
        /// Removes the cooldown
        /// </summary>
        public override void Execute()
        {
            if (base.Client == null || base.Client.GetRoleplay() == null || base.Client.GetHabbo() == null)
            {
                base.EndCooldown();
                return;
            }

            TimeLeft -= 125;

            if (TimeLeft > 0)
                return;

            base.EndCooldown();
        }
    }
}