using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Roleplay.Web;


namespace Plus.HabboRoleplay.Web.Outgoing.Statistics
{
    /// <summary>
    /// RetrieveUStatsWebEvent class.
    /// </summary>
    class RetrieveUStatsWebEvent : IWebEvent
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

            int UserID = Convert.ToInt32(Data);
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserID);

            if (TargetClient == null)
                return;

            string CachedTargetString = GetUserComponent.ReturnUserStatistics(TargetClient);
            
            if (String.IsNullOrEmpty(CachedTargetString))
                return;

            Socket.Send("compose_characterbar:" + CachedTargetString);
        }
    }
}
