using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Items;
using System.Linq;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using System.Threading;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Death timer
    /// </summary>
    public class BotDeathTimer : BotRoleplayTimer
    {
        public BotDeathTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            TimeLeft = 1000 * 60;
            TimeCount = 0;
        }
 
        /// <summary>
        /// Begins death sequence
        /// </summary>
        public override void Execute()
        {
            try
            {

                if (base.CachedBot == null || base.CachedBot.DRoomUser == null || base.CachedBot.DRoom == null)
                {
                    base.EndTimer();
                    return;
                }

                if (!base.CachedBot.DRoomUser.GetBotRoleplay().Dead)
                {
                    base.EndTimer();
                    return;
                }

                if (base.CachedBot.DRoomUser == null)
                    return;

                if (!base.CachedBot.DRoomUser.Frozen)
                    base.CachedBot.DRoomUser.Frozen = true;

                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;

                if (base.CachedBot.DRoomUser.Frozen)
                    base.CachedBot.DRoomUser.Frozen = false;

                base.CachedBot.DRoomUser.Chat("*Recupera a consciência *", true);
                RoleplayManager.SpawnChairs(null, "val14_wchair", base.CachedBot.DRoomUser);

                base.CachedBot.DRoomUser.GetBotRoleplay().Dead = false;

                if (base.CachedBot.DRoom != null)
                    base.CachedBot.DRoom.SendMessage(new UsersComposer(base.CachedBot.DRoomUser));

                if (base.CachedBot.DRoomUser.GetBotRoleplay().RoamBot)
                    base.CachedBot.MoveRandomly();

                base.EndTimer();
                return;
            }
            catch
            {
                base.EndTimer();
            }
        }
    }
}