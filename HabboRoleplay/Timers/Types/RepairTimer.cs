using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Pathfinding;
using Plus.Database.Interfaces;
using System.Linq;
using Plus.HabboHotel.Rooms;
using System.Drawing;
using Plus.HabboHotel.Items;
using Plus.Core;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Fence Repair timer
    /// </summary>
    public class RepairTimer : RoleplayTimer
    {
        public RepairTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            // 30 seconds convert to milliseconds
            TimeLeft = 30000;
        }

        /// <summary>
        /// Fence Repair timer
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetRoleplay() == null || base.Client.GetRoomUser() == null || base.Client.GetRoomUser().GetRoom() == null || !JailbreakManager.FenceBroken)
                {
                    base.EndTimer();
                    return;
                }

                int ItemId = (int)Params[0];
                Item BTile = base.Client.GetRoomUser().GetRoom().GetRoomItemHandler().GetItem(ItemId);
                Room Room = RoleplayManager.GenerateRoom(base.Client.GetHabbo().CurrentRoomId);

                if (Room == null)
                    return;

                if (BTile == null || BTile.Coordinate != base.Client.GetRoomUser().Coordinate || Room.RoomId != Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")))
                {
                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    RoleplayManager.Shout(base.Client, "*Para de reparar a cerca*", 4);
                    base.EndTimer();
                    return;
                }

                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;

                if (base.Client.GetRoomUser().CurrentEffect == EffectsList.SunnyD)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                RoleplayManager.Shout(base.Client, "*Repara com sucesso a cerca*", 4);
                JailbreakManager.FenceBroken = false;
                JailbreakManager.GenerateFence(Room);
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