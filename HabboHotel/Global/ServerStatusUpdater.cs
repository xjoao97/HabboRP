using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using log4net;
using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Global
{
    public class ServerStatusUpdater : IDisposable
    {
        private static ILog log = LogManager.GetLogger("Mango.Global.ServerUpdater");

        private const int UPDATE_IN_SECS = 30;

        private Timer _timer;
        
        public ServerStatusUpdater()
        {
        }

        public void Init()
        {
            this._timer = new Timer(new TimerCallback(this.OnTick), null, TimeSpan.FromSeconds(UPDATE_IN_SECS), TimeSpan.FromSeconds(UPDATE_IN_SECS));

            Console.Title = "Plus Emulador - 0 usuários onlines - 0 quartos carregados - 0 dia(s) 0 hora(s) online";

            log.Info("Status do servidor atualizado.");
        }

        public void OnTick(object Obj)
        {
            this.UpdateOnlineUsers();
        }

        private void UpdateOnlineUsers()
        {
            TimeSpan Uptime = DateTime.Now - PlusEnvironment.ServerStarted;

            if (PlusEnvironment.GetGame() == null)
                return;
            if (PlusEnvironment.GetGame().GetClientManager() == null)
                return;

            int UsersOnline = PlusEnvironment.GetGame().GetClientManager().Count;
            int PlayersOnline = PlusEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null).ToList().Count;
            int RoomCount = PlusEnvironment.GetGame().GetRoomManager().Count;

            Console.Title = "Plus emulador - " + UsersOnline + (HabboRoleplay.Misc.RoleplayManager.AccurateUserCount ? "/" + PlayersOnline.ToString() : "") + " usuários online - " + RoomCount + " quartos carregados - " + Uptime.Days + " dia(s) " + Uptime.Hours + " hora(s) online";

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                PlusEnvironment.GetGame().GetWebEventManager().BroadCastWebEvent("event_updateonlinecount", (!HabboRoleplay.Misc.RoleplayManager.AccurateUserCount ? UsersOnline.ToString() : PlayersOnline.ToString()));
                dbClient.SetQuery("UPDATE `server_status` SET `users_online` = @users, `loaded_rooms` = @loadedRooms, `environment_status` = '1' LIMIT 1;");
                dbClient.AddParameter("users", (!HabboRoleplay.Misc.RoleplayManager.AccurateUserCount ? UsersOnline : PlayersOnline));
                dbClient.AddParameter("loadedRooms", RoomCount);
                dbClient.RunQuery();
            }
        }


        public void Dispose()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0', `environment_status` = '0'");
            }

            this._timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
