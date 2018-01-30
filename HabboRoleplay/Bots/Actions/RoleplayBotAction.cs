using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Bots.Actions
{
    public abstract class RoleplayBotAction : IRoleplayBotAction
    {

        /// <summary>
        /// RoleplayBotAction Timer
        /// </summary>
        private Timer ActionTimer { get; set; }

        /// <summary>
        /// Roleplay Bot executing the action
        /// </summary>
        public RoomUser RoleplayBot { get; set; }

        /// <summary>
        /// Dictionary containing the interacting users & their respective roles in the (string)
        /// </summary>
        public ConcurrentDictionary<string, GameClient> ActionUserInteractors { get; private set; }

        /// <summary>
        /// Dictionary containing the interacting items & their respective roles in the (string)
        /// </summary>
        public ConcurrentDictionary<string, Item> ActionItemInteractors { get; private set; }

        /// <summary>
        /// Initializer for a RoleplayBotAction
        /// </summary>
        /// <param name="RoleplayBot"></param>
        public RoleplayBotAction(RoomUser RoleplayBot)
        {
            this.RoleplayBot = RoleplayBot;
            this.ActionUserInteractors = new ConcurrentDictionary<string, GameClient>();
            this.ActionItemInteractors = new ConcurrentDictionary<string, Item>();
            this.StartAction();
        }

        /// <summary>
        /// Called on action tick every 1000 ms
        /// </summary>
        public abstract void TickAction();

        /// <summary>
        /// Starts the RoleplayBotAction Timer
        /// </summary>
        public void StartAction()
        {
            this.ActionTimer = new Timer(ContinueAction, null, 1000, 1000);
        }

        /// <summary>
        /// Stops the RoleplayBotAction Timer & Disposes of other variables
        /// </summary>
        public void StopAction()
        {
            if (this.ActionTimer != null)
            {
                this.ActionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.ActionTimer.Dispose();
                this.ActionTimer = null;
                this.Dispose();
            }
        }

        /// <summary>
        /// RoleplayBotAction Timer Tick Callback
        /// </summary>
        /// <param name="State"></param>
        public void ContinueAction(object State)
        {
            try
            {
                this.TickAction();

                if (this.ActionTimer != null)
                    this.ActionTimer.Change(1000, 1000);
            }
            catch(Exception ex)
            {
                this.StopAction();
            }
        }

        /// <summary>
        /// Disposes of all other RoleplayBotAction variables
        /// </summary>
        public void Dispose()
        {
            this.RoleplayBot = null;
            this.ActionUserInteractors.Clear();
            this.ActionItemInteractors.Clear();
        }
    }
}
