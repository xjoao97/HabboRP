using Fleck;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Roleplay.Web.Outgoing
{
    class LiveFeedEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string[] parameters = Data.Split(',');

            if (parameters.Length < 1)
                return;

            Socket.Send("compose_livefeedevent:" + parameters[0]);
        }
    }
}
