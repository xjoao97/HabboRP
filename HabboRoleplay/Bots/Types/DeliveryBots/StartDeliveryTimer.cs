using System;
using System.Drawing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Start delivery timer
    /// </summary>
    public class StartDeliveryTimer : BotRoleplayTimer
    {
        public StartDeliveryTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// Starts the delivery bot
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

                Point Point = (Point)Params[0];

                if (base.CachedBot.DRoomUser.Coordinate != Point)
                    return;

                HandleDelivery();
                base.CachedBot.DRoomUser.GetBotRoleplayAI().StopActivities();
                base.EndTimer();
            }
            catch
            {
                base.EndTimer();
            }
        }

        public void HandleDelivery()
        {
            var SquareInFront = new Point(base.CachedBot.DRoomUser.SquareInFront.X, base.CachedBot.DRoomUser.SquareInFront.Y);

            double MaxHeight = 0.0;

            Item ItemInFront;
            if (base.CachedBot.DRoom.GetGameMap().GetHighestItemForSquare(SquareInFront, out ItemInFront))
            {
                if (ItemInFront != null)
                    MaxHeight = ItemInFront.TotalHeight;
            }

            RoleplayManager.PlaceItemToRoom(null, 8029, 0, SquareInFront.X, SquareInFront.Y, MaxHeight, base.CachedBot.DRoomUser.RotBody, false, base.CachedBot.DRoom.Id, false, "0", false, "weapon");
        }
    }
}