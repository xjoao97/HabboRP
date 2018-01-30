using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Items;
using System.Linq;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using System.Threading;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Pickup Delivery timer
    /// </summary>
    public class PickupDeliveryTimer : BotRoleplayTimer
    {

        private bool PickedCrate { get; set; }

        public PickupDeliveryTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            TimeCount = 0;
            TimeCount2 = 0;
            this.PickedCrate = false;
        }
 
        /// <summary>
        /// Picks up the delivered package
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
                Item Item = (Item)Params[1];

                if (base.CachedBot.DRoomUser.Coordinate != Point)
                    return;

                int Rot = Rotation.Calculate(Item.GetX, Item.GetY, base.CachedBot.DRoomUser.Coordinate.X, base.CachedBot.DRoomUser.Coordinate.Y);
                base.CachedBot.DRoomUser.SetRot(Rot, false);
                base.CachedBot.DRoom.GetRoomItemHandler().RemoveFurniture(null, Item.Id);


                if (!this.PickedCrate)
                {

                    if (RoleplayManager.DeliveryWeapon == null)
                    {
                        base.CachedBot.DRoom.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                        base.CachedBot.DRoomUser.Chat("Alguma coisa está errada com essa arma!", true);
                        base.CachedBot.DRoomUser.GetBotRoleplay().WalkingToItem = false;
                        InitiateGoHome();
                        base.EndTimer();
                    }

                    base.CachedBot.DRoomUser.Chat("Atualiza o novo estoque de " + RoleplayManager.DeliveryWeapon.PublicName + "!", true);

                    var weapon = RoleplayManager.DeliveryWeapon;
                    int NewStock = 50;

                    RoleplayManager.UserWhoCalledDelivery = 0;
                    RoleplayManager.DeliveryWeapon = null;

                    if (weapon != null)
                    {
                        WeaponManager.Weapons[weapon.Name.ToLower()].Stock = NewStock;

                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `rp_weapons` SET `stock` = '" + NewStock + "' WHERE `name` = '" + weapon.Name.ToLower() + "'");
                        }
                    }

                    base.CachedBot.DRoomUser.GetBotRoleplay().WalkingToItem = false;
                    this.PickedCrate = true;
                }

                InitiateGoHome();
                base.EndTimer();
            }
            catch
            {
                base.EndTimer();
            }
        }

        public void InitiateGoHome()
        {
            if (!base.CachedBot.DRoomUser.GetBotRoleplayAI().OnDuty)
                return;

            var Point = new System.Drawing.Point(base.CachedBot.DRoomUser.GetBotRoleplay().oX, base.CachedBot.DRoomUser.GetBotRoleplay().oY);

            if (base.CachedBot.DRoomUser.Coordinate != Point)
                base.CachedBot.DRoomUser.MoveTo(Point);
        }
    }
}