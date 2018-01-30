using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Cache;
using log4net;
using Plus.HabboRoleplay.Timers.Types;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Roleplay.Web.Incoming.Interactions
{
    /// <summary>
    /// WalkWebEvent class.
    /// </summary>
    class WalkWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (Data.Contains("stop"))
            {
                Client.GetRoleplay().WalkDirection = WalkDirections.None;
                Client.GetRoomUser().GoalX = Client.GetRoomUser().SetX; 
                Client.GetRoomUser().GoalY = Client.GetRoomUser().SetY;
                return;
            }

            if (Client.GetRoleplay().Game != null)
            {
                Client.SendWhisper("Você não pode usar as setas enquanto estiver dentro de um evento!", 1);

                Client.GetRoleplay().ArrowEnabled = false;
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "compose_arrowmovement:no");
                return;
            }

            WalkDirections Direction = WalkDirections.None;

            Direction = (WalkDirections)Enum.Parse(typeof(WalkDirections), Data);
         
            if (Direction == WalkDirections.None)
                return;
            
            Client.GetRoleplay().WalkDirection = Direction;

            if (Client.GetRoomUser() != null)
            {
                Client.GetRoomUser().GoalX = Client.GetRoomUser().SetX;
                Client.GetRoomUser().GoalY = Client.GetRoomUser().SetY;
            }

            Point Point = RoleplayManager.GetDirectionDeviation(Client.GetRoomUser());

            if (Point != new Point(0,0))
                 Client.GetRoomUser().MoveTo(Point);  
                      
            return;
        }
    }

    public enum WalkDirections
    {
        Down,
        Up,
        Left,
        Right,
        None
    }
}
