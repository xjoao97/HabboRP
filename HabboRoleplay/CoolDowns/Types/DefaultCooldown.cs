using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Cooldowns.Types
{
    /// <summary>
    /// Default cooldown
    /// </summary>
    public class DefaultCooldown : Cooldown
    {
        public DefaultCooldown(string Type, GameClient Client, int Time, int Amount) 
            : base(Type, Client, Time, Amount)
        {
            TimeLeft = Amount * 1000;
        }
 
        /// <summary>
        /// Removes the cooldown
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetRoleplay() == null || base.Client.GetHabbo() == null)
                {
                    base.EndCooldown();
                    return;
                }

                TimeLeft -= 1000;

                if (Type.ToLower() == "recarregar" && TimeLeft > 0)
                    base.Client.SendWhisper("Recarregando: " + (TimeLeft / 1000) + "/" + Amount, 1);

                if (TimeLeft > 0)
                    return;

                if (Type.ToLower() == "maconha")
                {
                    RoleplayManager.Shout(base.Client, "*Sinta o desgaste elevado da maconha*", 4);
                    base.Client.GetRoleplay().HighOffWeed = false;
                }
                else if (Type.ToLower() == "cocaina")
                {
                    RoleplayManager.Shout(base.Client, "*Parece que o consumo de cocaína é alto*", 4);
                    base.Client.GetRoleplay().HighOffCocaine = false;
                }
                else if (Type.ToLower() == "recarregar")
                    base.Client.SendWhisper("Carregamento feito!", 1);

                if (base.Client.GetRoleplay().SpecialCooldowns.ContainsKey(Type.ToLower()))
                    base.Client.GetRoleplay().SpecialCooldowns.TryUpdate(Type.ToLower(), TimeLeft, base.Client.GetRoleplay().SpecialCooldowns[Type.ToLower()]);

                base.EndCooldown();
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Error in Execute() void: " + e);
            }
        }
    }
}