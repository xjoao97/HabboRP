using System;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Weapons;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Roleplay.Web.Incoming.Interactions;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Timers;

namespace Plus.HabboRoleplay.Misc
{
    public class BlackListManager
    {
        /// <summary>
        /// Thread-safe list containing blacklisted users
        /// </summary>
        public static List<int> BlackList = new List<int>();

        /// <summary>
        /// Gets the blacklisted users from the database
        /// </summary>
        public static void Initialize()
        {
            BlackList.Clear();
            DataTable Table;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * from `rp_blacklist`");
                Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                        BlackList.Add(Convert.ToInt32(Row["id"]));
                }
            }
        }

        /// <summary>
        /// Adds the chosen client to the blacklist
        /// </summary>
        /// <param name="Client"></param>
        public static void AddBlackList(int Id)
        {
            if (BlackList.Contains(Id))
                return;

            BlackList.Add(Id);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO `rp_blacklist` (`id`) VALUES ('" + Id + "')");
            }
        }

        /// <summary>
        /// Removes the chosen client from the blacklist
        /// </summary>
        /// <param name="Client"></param>
        public static void RemoveBlackList(int Id)
        {
            if (!BlackList.Contains(Id))
                return;

            BlackList.Remove(Id);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `rp_blacklist` WHERE `id` = '" + Id + "'");
            }
        }
    }
}