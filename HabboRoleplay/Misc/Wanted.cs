using System;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Weapons;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Misc
{
    public class Wanted
    {
        /// <summary>
        /// User ID of the user who is wanted
        /// </summary>
        public uint UserId;

        /// <summary>
        /// Last seen room name of the wanted user
        /// </summary>
        public string LastSeenRoom;

        /// <summary>
        /// Wanted level assigned to the wanted user
        /// </summary>
        public int WantedLevel;

        /// <summary>
        /// Wanted variable
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="LastSeenRoom"></param>
        /// <param name="WantedLevel"></param>
        public Wanted(uint UserId, string LastSeenRoom, int WantedLevel)
        {
            this.UserId = UserId;
            this.LastSeenRoom = LastSeenRoom;
            this.WantedLevel = WantedLevel;
        }
    }
}