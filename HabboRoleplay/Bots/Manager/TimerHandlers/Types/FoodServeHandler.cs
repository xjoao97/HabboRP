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
using Plus.HabboRoleplay.Food;

namespace Plus.HabboRoleplay.Bots.Manager.TimerHandlers.Types
{
    public class FoodServeHandler : IBotHandler
    {

        public bool Active { get; set; }
        public GameClient InteractingUser { get; set; }
        public RoleplayBot InteractingBot { get; set; }
        public ConcurrentDictionary<string, string> Values { get; set; }

        private bool CurrentlyServing { get; set; }
        private Food.Food FoodServing { get; set; }
        private Point FoodServePoint { get; set; }
        private Point UserServePoint { get; set; }
        private string FoodServingName { get; set; }

        public FoodServeHandler(params object[] Params)
        {

            this.InteractingBot = (RoleplayBot)Params[0];
            this.InteractingUser = (GameClient)Params[1];
            this.FoodServing = (Food.Food)Params[2];
            this.FoodServePoint = (Point)Params[3];
            this.UserServePoint = (Point)Params[4];
            this.FoodServingName = (string)Params[5];
            this.CurrentlyServing = true;
            this.Active = true;
        }

        public bool ExecuteHandler(params object[] Params)
        {

            #region Conditions
            if (!this.Active) return false;

            if (this.InteractingBot.DRoomUser == null)
                return this.AbortHandler();
            #endregion

            ConcurrentDictionary<GameClient, ConcurrentDictionary<object, object>> 
                ServeList = (ConcurrentDictionary<GameClient, ConcurrentDictionary<object, object>>)Params[0];

            if (ServeList.Count <= 0)
                this.AbortHandler();
            else
            {
                if (this.CurrentlyServing)
                {
                    this.ServeUser();
                }
            }

            return true;
        }

        private void ServeUser()
        {

        }

        public bool AbortHandler(params object[] Params)
        {

            this.Active = false;
            return true;

        }

        public bool RestartHandler(params object[] Params)
        {
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
