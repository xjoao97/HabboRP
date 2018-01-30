using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Bots.Manager.TimerHandlers.Types
{
    public class DeliveryHandler : IBotHandler
    {

        public bool Active { get; set; }
        public GameClient InteractingUser { get; set; }
        public RoleplayBot InteractingBot { get; set; }
        public ConcurrentDictionary<string, string> Values { get; set; }

        public bool Delivered { get; private set; }
        public bool DroppedCrate { get; private set; }
        public bool LeftRoom { get; private set; }

        public DeliveryHandler(params object[] Params)
        {
            this.InteractingBot = (RoleplayBot)Params[0];
            this.Delivered = false;
            this.DroppedCrate = false;
            this.LeftRoom = false;
            this.Active = true;
        }

        public bool ExecuteHandler(params object[] Params)
        {

            #region Conditions
            if (!this.Active) return false;

            if (this.InteractingBot.DRoomUser == null)
                return this.AbortHandler();
            #endregion

            if (!this.DroppedCrate)
            {
                this.DropCrate();
                return false;
            }

            if (!this.LeftRoom)
            {
                this.LeaveRoom();
                return false;
            }

            this.StopDelivery();

            return true;
        }

        private bool DropCrate()
        {

            Point DeliveryDropPoint = new Point(this.InteractingBot.oX, this.InteractingBot.oY);

            if (this.InteractingBot.DRoomUser.Coordinate != DeliveryDropPoint)
                return false;
            else
            {
                if (!this.DroppedCrate)
                {
                    var SquareInFront = new Point(this.InteractingBot.DRoomUser.SquareInFront.X, this.InteractingBot.DRoomUser.SquareInFront.Y);
                    double MaxHeight = 0.0;
                    Item ItemInFront;
                    if (this.InteractingBot.DRoom.GetGameMap().GetHighestItemForSquare(SquareInFront, out ItemInFront))
                    {
                        if (ItemInFront != null)
                            MaxHeight = ItemInFront.TotalHeight;
                    }
                    RoleplayManager.PlaceItemToRoom(null, 8029, 0, SquareInFront.X, SquareInFront.Y, MaxHeight, this.InteractingBot.DRoomUser.RotBody, false, this.InteractingBot.DRoom.Id, false, "0", false, "weapon");
                    this.DroppedCrate = true;
                }
            }
            return true;
        }

        private bool LeaveRoom()
        {

            Item Item;
            
            if (!this.LeftRoom)
            {
                this.InteractingBot.GetStopWorkItem(this.InteractingBot.DRoom, out Item);
                if (Item != null)
                {
                    if (this.InteractingBot.DRoomUser.Coordinate != Item.Coordinate)
                    {
                        this.InteractingBot.DRoomUser.MoveTo(Item.Coordinate);
                    }
                    else
                    {
                        this.LeftRoom = true;
                        Item.ExtraData = "2";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(2, true);
                        this.AbortHandler();
                        return true;
                    }
                }
                else
                    this.StopDelivery();
            }

            return false;
        }

        private bool StopDelivery()
        {
            this.AbortHandler();
            return true;
        }

        public bool AbortHandler(params object [] Params)
        {

            Room Room = this.InteractingBot.DRoom;

            RoleplayBotManager.EjectDeployedBot(this.InteractingBot.DRoomUser, Room);
            this.InteractingBot.Invisible = true;
            this.Active = false;

            return true;
        }

        public bool RestartHandler(params object[] Params)
        {

            this.Delivered = false;
            this.Active = true;

            return true;
        }

        public void SetValues(string Key, string Value)
        {
            if (this.Values.ContainsKey(Key))
            {
                string OldValue = this.Values[Key];
                this.Values.TryUpdate(Key, Value, OldValue);
            }
            else
                this.Values.TryAdd(Key, Value);
        }

        public void AssignInteractingUser(GameClient InteractingUser)
        {
            this.InteractingUser = InteractingUser;
        }
    }
}
