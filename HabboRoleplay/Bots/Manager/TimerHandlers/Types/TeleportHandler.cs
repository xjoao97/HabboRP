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
    public class TeleportHandler : IBotHandler
    {

        public bool Active { get; set; }
        public GameClient InteractingUser { get; set; }
        public RoleplayBot InteractingBot { get; set; }
        public ConcurrentDictionary<string, string> Values { get; set; }
        
        public bool OnTeleport { get; private set; }
        public bool UsedTeleport { get; private set; }
        public Item EnteringTeleport { get; private set; }
        public Item LeavingTeleport { get; private set; }

        public TeleportHandler(params object[] Params)
        {
            this.InteractingBot = (RoleplayBot)Params[0];
            this.OnTeleport = false;
            this.UsedTeleport = false;
            this.EnteringTeleport = (Item)Params[1];
            this.Active = true;

            if (this.InteractingBot.GetLinkedTeleport(this.EnteringTeleport) == null)
            {
                this.AbortHandler();
                return;
            }

            this.LeavingTeleport = this.InteractingBot.GetLinkedTeleport(this.EnteringTeleport);

          //  Bot.DRoomUser.Chat("Begun teleport!", false);
        }

        public bool ExecuteHandler(params object[] Params)
        {

            #region Conditions

            if (!this.Active)
                return false;

            if (this.UsedTeleport)
                return this.AbortHandler();

            if (this.EnteringTeleport == null || this.InteractingBot == null)
                return this.AbortHandler();

            if (this.InteractingBot.DRoomUser == null)
                return this.AbortHandler();
            #endregion

            if (!this.OnTeleport)
                this.WalkToTeleport();
            else
                this.UseTeleport();

            return true;
        }

        public bool AbortHandler(params object[] Params)
        {

           // this.InteractingBot.DRoomUser.Chat("Stopped teleport", true);
            this.Active = false;

            return true;
        }

        public bool RestartHandler(params object[] Params)
        {
            this.OnTeleport = false;
            this.UsedTeleport = false;
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

        private void UseTeleport()
        {
            RoomUser BotUserInstance = this.InteractingBot.DRoomUser;


            this.InteractingBot.X = this.LeavingTeleport.GetX;
            this.InteractingBot.Y = this.LeavingTeleport.GetY;
            this.InteractingBot.Z = this.LeavingTeleport.GetZ;
            this.InteractingBot.SpawnRot = this.LeavingTeleport.Rotation;

            this.InteractingBot.DRoomUser.ClearMovement(true);

            this.InteractingBot.SpawnId = this.LeavingTeleport.RoomId;
            this.InteractingBot.LastTeleport = this.LeavingTeleport;

            Room TeleRoom = RoleplayManager.GenerateRoom(this.LeavingTeleport.RoomId);
            RoleplayBotManager.TransportDeployedBot(BotUserInstance, TeleRoom.Id, true);

            this.UsedTeleport = true;
        }

        private void WalkToTeleport()
        {
            RoomUser BotUserInstance = this.InteractingBot.DRoomUser;
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(BotUserInstance.Coordinate, this.EnteringTeleport.Coordinate);

            if (Distance <= 3.5)
            {
                this.OnTeleport = true;
            }
            else
            {
                BotUserInstance.MoveTo(this.EnteringTeleport.Coordinate);
            }
        }

    }
}
