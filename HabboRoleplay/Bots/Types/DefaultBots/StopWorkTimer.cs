using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Stop work timer
    /// </summary>
    public class StopWorkTimer : BotRoleplayTimer
    {
        public StopWorkTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// starts work
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.CachedBot == null || base.CachedBot.DRoomUser== null || base.CachedBot.DRoomUser.GetBotRoleplay() == null || base.CachedBot.DRoom == null)
                {
                    base.EndTimer();
                    return;
                }

                Item Item;
                if (base.CachedBot.DRoomUser.GetBotRoleplay().GetStopWorkItem(base.CachedBot.DRoom, out Item))
                {
                    var Point = new System.Drawing.Point(Item.GetX, Item.GetY);
                    if (base.CachedBot.DRoomUser.Coordinate != Point)
                        return;

                    if (Item != null)
                    {
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(2, true);
                    }

                    base.CachedBot.DRoomUser.GetBotRoleplay().Invisible = true;
                    base.CachedBot.DRoom.SendMessage(new UserRemoveComposer(base.CachedBot.DRoomUser.VirtualId));
                }

                base.EndTimer();
            }
            catch
            {
                base.EndTimer();
            }
        }
    }
}