using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;

namespace Plus.HabboRoleplay.Cooldowns.Types
{
    /// <summary>
    /// Default bot cooldown
    /// </summary>
    public class DefaultBotCooldown : BotCooldown
    {
        public DefaultBotCooldown(string Type, RoleplayBot CachedBot, int Time, int Amount) 
            : base(Type, CachedBot, Time, Amount)
        {
            TimeLeft = Amount * 1000;
        }
 
        /// <summary>
        /// Removes the cooldown
        /// </summary>
        public override void Execute()
        {
            if (base.Bot == null || base.Bot.DRoomUser == null)
            {
                base.EndCooldown();
                return;
            }

            TimeLeft -= 1000;

            if (TimeLeft > 0)
                return;

            base.EndCooldown();
        }
    }
}