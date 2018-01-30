using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Events
{
    public interface IEvent
    {
        /// <summary>
        /// Responds to the event
        /// </summary>
        void Execute(object Source, object[] Params);
    }
}