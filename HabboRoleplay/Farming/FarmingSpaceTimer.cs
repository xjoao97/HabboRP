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
    /// Check if farmingspaces have expired
    /// </summary>
    public class FarmingSpaceTimer : SystemRoleplayTimer
    {
        public FarmingSpaceTimer(string Type, int Time, bool Forever, object[] Params) 
            : base(Type, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// Checks the expiration date
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (FarmingManager.FarmingSpaces == null)
                {
                    base.EndTimer();
                    return;
                }

                TimeCount++;

                if (TimeCount == 2)
                {
                    lock (FarmingManager.FarmingSpaces)
                    {
                        if (FarmingManager.FarmingSpaces.Count <= 0)
                            return;

                        foreach (FarmingSpace FarmingSpace in FarmingManager.FarmingSpaces.Values)
                        {
                            if (FarmingSpace == null || FarmingSpace.Item == null)
                                continue;

                            if (FarmingSpace.Item.RentableSpaceData == null || FarmingSpace.Item.RentableSpaceData.FarmingSpace == null)
                                continue;

                            if (FarmingSpace.Item.RentableSpaceData.FarmingSpace.OwnerId <= 0)
                                continue;

                            if (FarmingSpace.Item.RentableSpaceData.FarmingSpace.Expiration > 0)
                            {
                                FarmingSpace.Item.RentableSpaceData.FarmingSpace.Expiration--;
                                FarmingSpace.Item.RentableSpaceData.TimeLeft--;
                            }
                            else
                                FarmingSpace.Item.RentableSpaceData.FarmingSpace.ExpireSpace();
                        }
                    }
                    TimeCount = 0;
                }
            }
            catch(Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}