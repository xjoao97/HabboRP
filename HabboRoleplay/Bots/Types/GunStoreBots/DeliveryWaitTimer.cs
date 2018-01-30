using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Items;
using System.Linq;
using Plus.HabboHotel.Pathfinding;
using System.Threading;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Rooms;
using System.Drawing;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Delivery Wait timer
    /// </summary>
    public class DeliveryWaitTimer : BotRoleplayTimer
    {
        public bool DeliveryArrived { get; set; }

        public DeliveryWaitTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            TimeCount = 0;
            TimeCount2 = 0;
            this.DeliveryArrived = false;
        }
 
        /// <summary>
        /// Waits for delivery bot to arrive with package
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.CachedBot == null || base.CachedBot.DRoomUser == null || base.CachedBot.DRoom == null || !RoleplayManager.CalledDelivery || RoleplayManager.DeliveryWeapon == null)
                {
                    base.EndTimer();
                    return;
                }

                TimeCount++;
                
                if (TimeCount < 1000)
                    return;

                if (!this.DeliveryArrived)
                {
                    RoleplayBot Bot = RoleplayBotManager.GetCachedBotByAI(RoleplayBotAIType.DELIVERY);

                    if (Bot == null)
                    {
                        base.CachedBot.DRoomUser.Chat("O bot de entrega está ocupado agora, desculpe por isso!", true);
                        base.EndTimer();
                        return;
                    }
                    else
                    {
                        if (!this.DeliveryArrived)
                        {
                            Item Item = null;
                           
                            Bot.GetStopWorkItem(base.CachedBot.DRoom, out Item);
                            
                            if (Item == null)
                            {
                                base.CachedBot.DRoomUser.Chat("O bot de entrega está ocupado agora, desculpe por isso! ", true);
                                base.EndTimer();
                                return;
                            }

                            RoleplayBotManager.DeployBotByAI(RoleplayBotAIType.DELIVERY, "workitem", base.CachedBot.DRoom.Id);

                            this.DeliveryArrived = true;
                        }
                    }
                }
                

                if (base.CachedBot.DRoom.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.DELIVERY_BOX).ToList().Count <= 0)
                    return;

                TimeCount2++;

                if (TimeCount2 < 200)
                    return;

                RoleplayManager.CalledDelivery = false;
                HandleDelivery();
                base.EndTimer();
            }
            catch
            {
                base.EndTimer();
            }
        }

        public void HandleDelivery()
        {
            if (!base.CachedBot.DRoomUser.GetBotRoleplayAI().OnDuty)
                return;

            if (base.CachedBot.DRoom == null)
                return;

            var Item = base.CachedBot.DRoom.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().Id == 8029);
            var Point = new System.Drawing.Point(Item.SquareBehind.X, Item.SquareBehind.Y);

            if (Item == null)
                return;

            object[] Params = { Point, Item };

            base.CachedBot.DRoomUser.Chat("Tempo para pegar a entrega!", true);
            base.CachedBot.DRoomUser.GetBotRoleplay().WalkingToItem = true;
            base.CachedBot.DRoomUser.MoveTo(Point);
            base.CachedBot.DRoomUser.GetBotRoleplay().TimerManager.CreateTimer("pickupdelivery", CachedBot, 10, true, Params);
        }
    }
}