using System;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Turfs;
using Plus.Communication.Packets.Outgoing.Polls;
using Plus.HabboHotel.Polls;
using System.Linq;

namespace Plus.HabboRoleplay.Events.Methods
{
    /// <summary>
    /// Triggered when the user teleports
    /// </summary>
    public class OnTeleport : IEvent
    {
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;

            if (Client == null || Client.GetHabbo() == null)
                return;

            if (Client.GetHabbo()._disconnected)
                return;

            BotInteractionCheck(Client, Params);
        }

        #region Bot Interaction Check
        /// <summary>
        /// Checks for any possible interactions with bots in room
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Params"></param>
        public void BotInteractionCheck(GameClient Client, object[] Params)
        {
            Room Room = Client.GetHabbo().CurrentRoom;
            if (Room == null) return;

            List<RoomUser> Bots = Room.GetRoomUserManager().GetBotList().ToList();

            foreach (RoomUser Bot in Bots)
            {
                if (!Bot.IsBot)
                    continue;

                if (!Bot.IsRoleplayBot)
                    continue;

                if (!Bot.GetBotRoleplay().Deployed)
                    continue;

                Bot.GetBotRoleplayAI().OnUserUseTeleport(Client, Params);
            }
        }
        #endregion
    }
}