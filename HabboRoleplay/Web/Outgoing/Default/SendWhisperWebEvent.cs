using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Default
{
    /// <summary>
    /// SendWhisperWebEvent class.
    /// </summary>
    class SendWhisperWebEvent : IWebEvent
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

            int colour = 0;
            if (!int.TryParse(parameters[1], out colour))
                return;

            Client.SendWhisper(parameters[0], colour);
        }
    }
}
