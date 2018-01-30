using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Items;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to blow up dynamite
    /// </summary>
    public class DynamiteTimer : SystemRoleplayTimer
    {
        public DynamiteTimer(string Type, int Time, bool Forever, object[] Params) 
            : base(Type, Time, Forever, Params)
        {
            // 10 seconds converted to miliseconds
            TimeLeft = 10 * 1000;
            TimeCount = 0;
        }
 
        /// <summary>
        /// Blows up the dynamite
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (!JailbreakManager.JailbreakActivated)
                {
                    base.EndTimer();
                    return;
                }

                GameClient UserJailbreaking = (GameClient)Params[0];
                Item Item = (Item)Params[1];
                Item Item2 = (Item)Params[2];

                TimeLeft -= 500;

                if (TimeLeft > 500)
                    return;

                if (TimeCount == 0 && Item2 != null)
                {
                    Item2.ExtraData = "1";
                    Item2.UpdateState();
                }

                if (TimeLeft > 0)
                    return;

                JailbreakManager.InitiateJailbreak(UserJailbreaking);
                base.EndTimer();
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}