using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;

namespace Plus.HabboRoleplay.Cooldowns.Types
{
    /// <summary>
    /// Cooldown to let you farm again
    /// </summary>
    public class FarmingCooldown : Cooldown
    {
        public FarmingCooldown(string Type, GameClient Client, int Time, int Amount) 
            : base(Type, Client, Time, Amount)
        {
            TimeLeft = Convert.ToInt32(RoleplayData.GetData("farming", "cooldown"));
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

            TimeLeft -= Convert.ToInt32(RoleplayData.GetData("farming", "cooldown"));

            if (TimeLeft > 0)
                return;

            base.EndCooldown();
        }
    }
}