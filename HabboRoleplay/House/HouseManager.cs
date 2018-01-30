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

namespace Plus.HabboRoleplay.Houses
{
    public class HouseManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Houses.HouseManager");

        /// <summary>
        /// Static int used to generate item ids
        /// </summary>
        public int SignMultiplier = 1500000;

        /// <summary>
        /// Thread-safe dictionary containing all houses
        /// </summary>
        public ConcurrentDictionary<int, House> HouseList = new ConcurrentDictionary<int, House>();

        /// <summary>
        /// Initializes the house list dictionary
        /// </summary>
        public void Init()
        {
            HouseList.Clear();
            DataTable Houses;

            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * from `rp_houses`");
                Houses = DB.getTable();

                if (Houses != null)
                {
                    foreach (DataRow Row in Houses.Rows)
                    {
                        int RoomId = Convert.ToInt32(Row["room_id"]);
                        int OwnerId = Convert.ToInt32(Row["owner_id"]);
                        int Cost = Convert.ToInt32(Row["cost"]);
                        bool ForSale = PlusEnvironment.EnumToBool(Row["for_sale"].ToString());
                        string[] Upgrades = Row["upgrades"].ToString().Split(',');
                        bool IsLocked = PlusEnvironment.EnumToBool(Row["is_locked"].ToString());
                        int SignRoomId = Convert.ToInt32(Row["sign_room_id"]);
                        int SignX = Convert.ToInt32(Row["sign_x"]);
                        int SignY = Convert.ToInt32(Row["sign_y"]);
                        int SignZ = Convert.ToInt32(Row["sign_z"]);

                        HouseSign newSign = new HouseSign(SignRoomId, SignX, SignY, SignZ); 
                        House newHouse = new House(RoomId, OwnerId, Cost, ForSale, Upgrades, IsLocked, newSign);
                        HouseList.TryAdd(RoomId, newHouse);
                    }
                }
            }

            //log.Info("Carregado " + HouseList.Count + " casas.");
        }

        /// <summary>
        /// Gets house based on the owner id
        /// </summary>
        public House GetHouseByOwnerId(int OwnerId)
        {
            if (OwnerId == 0)
                return null;

            if (HouseList.Values.Where(x => x.OwnerId == OwnerId).ToList().Count > 0)
                return HouseList.Values.FirstOrDefault(x => x.OwnerId == OwnerId);
            else
                return null;
        }

        /// <summary>
        /// Gets house based on the signs item id
        /// </summary>
        public House GetHouseBySignItem(Item Item)
        {
            if (Item == null)
                return null;

            if (HouseList.Values.Where(x => x.Sign.Item != null && x.Sign.Item.Id == Item.Id).ToList().Count > 0)
                return HouseList.Values.FirstOrDefault(x => x.Sign.Item != null && x.Sign.Item.Id == Item.Id);
            else
                return null;
        }

        /// <summary>
        /// Gets all houses in the room based on sign roomids
        /// </summary>
        public List<House> GetHousesBySignRoomId(int roomid)
        {
            if (HouseList.Values.Where(x => x.Sign.RoomId == roomid).ToList().Count > 0)
                return HouseList.Values.Where(x => x.Sign.RoomId == roomid).ToList();
            else
                return new List<House>();
        }
    }
}
