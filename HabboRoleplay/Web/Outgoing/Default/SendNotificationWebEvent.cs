using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Default
{
    /// <summary>
    /// SendNotificationWebEvent class.
    /// </summary>
    class SendNotificationWebEvent : IWebEvent
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

            Socket.Send("compose_jsalert:" + parameters[0]);
        }
    }
}
