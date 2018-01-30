using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using System.Linq;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Farming;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Check if day and night is operating
    /// </summary>
    public class DayNightCycleTimer : SystemRoleplayTimer
    {
        public DayNightCycleTimer(string Type, int Time, bool Forever, object[] Params) 
            : base(Type, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// Executes the day and night process
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (PlusEnvironment.GetGame() == null)
                    return;

                if (PlusEnvironment.GetGame().GetRoomManager() == null)
                    return;

                if (PlusEnvironment.GetGame().GetRoomManager().GetRooms().Count <= 0)
                    return;

                DayNightManager.SetTime();
            }
            catch(Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}