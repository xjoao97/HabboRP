using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using log4net;

namespace Plus.HabboRoleplay.Turfs
{
    class TurfManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Turfs.TurfManager");

        /// <summary>
        /// Thread-safe dictionary containing all roleplay turfs
        /// </summary>
        public static ConcurrentDictionary<int, Turf> TurfList = new ConcurrentDictionary<int, Turf>();

        /// <summary>
        /// Initializes the turf list dictionary
        /// </summary>
        public static void Initialize()
        {
            TurfList.Clear();

            DataTable TurfsTable;

            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * from `rp_gangs_turfs`");
                TurfsTable = DB.getTable();

                if (TurfsTable != null)
                {
                    foreach (DataRow Turfs in TurfsTable.Rows)
                    {
                        int RoomId = Convert.ToInt32(Turfs["room_id"]);
                        int GangId = Convert.ToInt32(Turfs["gang_id"]);
                        int BeginX = Convert.ToInt32(Turfs["begin_x"]);
                        int BeginY = Convert.ToInt32(Turfs["begin_y"]);
                        int EndX = Convert.ToInt32(Turfs["end_x"]);
                        int EndY = Convert.ToInt32(Turfs["end_y"]);
                        int FlagX = Convert.ToInt32(Turfs["flag_x"]);
                        int FlagY = Convert.ToInt32(Turfs["flag_y"]);

                        Turf newTurf = new Turf(RoomId, GangId, BeginX, BeginY, EndX, EndY, FlagX, FlagY);
                        TurfList.TryAdd(RoomId, newTurf);
                    }
                }
            }

            //log.Info("Carregado " + TurfList.Count + " territórios.");
        }

        /// <summary>
        /// Gets the food based on roomid
        /// </summary>
        /// <param name="roomid"></param>
        /// <returns></returns>
        public static Turf GetTurf(int roomid)
        {
            try
            {
                Turf theturf = null;

                foreach (Turf turf in TurfList.Values)
                {
                    if (turf.RoomId == roomid)
                    {
                        return turf;
                    }
                }

                return theturf;
            }
            catch
            {
                return null;
            }
        }
    }
}
