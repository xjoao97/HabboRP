using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Roleplay.Web;

namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// TimerDialogueWebEvent class.
    /// </summary>
    class TimerDialogueWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string Action = Data.Split(',')[0].Split(':')[1];
            string Timer = Data.Split(',')[1].Split(':')[1];
            string Value = Data.Split(',')[2].Split(':')[1];

            Socket.Send("compose_timer:" + Timer + ",action:" + Action + ":,value:" + Value);
        }
    }
}
