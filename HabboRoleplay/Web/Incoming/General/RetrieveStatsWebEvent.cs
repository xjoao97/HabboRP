using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;
using Plus.HabboHotel.Cache;
using Plus.HabboRoleplay.Web.Outgoing.Statistics;

namespace Plus.HabboHotel.Roleplay.Web.Incoming.General
{
    /// <summary>
    /// RetrieveStatsWebEvent class.
    /// </summary>
    class RetrieveStatsWebEvent : IWebEvent
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

            if (Client != null)
            {
                string CachedDataString = GetUserComponent.ReturnUserStatistics(Client);

                if (String.IsNullOrEmpty(CachedDataString))
                    return;

                Socket.Send("compose_characterbar:" + CachedDataString);
            }
            else
            {
                using (UserCache CachedClient = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Convert.ToInt32(Socket.ConnectionInfo.Path.Trim().Split('/')[1])))
                {
                    if (CachedClient == null)
                        return;

                    string CachedDatString2 = GetUserComponent.ReturnUserStatistics(CachedClient);

                    if (String.IsNullOrEmpty(CachedDatString2))
                        return;

                    Socket.Send("compose_characterbar:" + GetUserComponent.ReturnUserStatistics(CachedClient));
                }
            }
        }
    }
}
