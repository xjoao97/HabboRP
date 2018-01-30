using System;
using System.Data;
using System.Linq;
using System.Collections.Concurrent;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using log4net;
using System.Collections.Generic;

namespace Plus.HabboRoleplay.Misc
{
    public class LotteryManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Misc.LotteryManager");

        /// <summary>
        /// Generic Values
        /// </summary>
        public static int TicketLimit;
        public static int Prize;
        public static int Cost;

        /// <summary>
        /// Thread-safe dictionary containing lottery tickets
        /// </summary>
        public static ConcurrentDictionary<int, int> LotteryTickets = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Generates the lottery dictionary values from database
        /// </summary>
        public static void Initialize()
        {
            TicketLimit = Convert.ToInt32(RoleplayData.GetData("lottery", "limit"));
            Prize = Convert.ToInt32(RoleplayData.GetData("lottery", "prize"));
            Cost = Convert.ToInt32(RoleplayData.GetData("lottery", "cost"));

            LotteryTickets.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_lottery`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["user_id"]);
                        int Ticket = Convert.ToInt32(Row["ticket"]);

                        if (!LotteryTickets.ContainsKey(UserId))
                            LotteryTickets.TryAdd(UserId, Ticket);
                    }
                }
            }

            //log.Info("Carregado " + LotteryTickets.Count + " bilhetes de loteria.");
        }

        /// <summary>
        /// Checks if the lottery tickets have all been bought
        /// </summary>
        /// <returns></returns>
        public static bool LotteryFull()
        {
		
		 
            return LotteryTickets.Count == TicketLimit;
        }

        /// <summary>
        /// Generates a winner randomly
        /// </summary>
        /// <returns></returns>
        public static int GetWinner()
        {
            CryptoRandom Random = new CryptoRandom();

            List<int> Contendents = LotteryTickets.Keys.ToList();

            int RandomWinner = Random.Next(0, Contendents.Count);

            return Contendents[RandomWinner];
        }

        /// <summary>
        /// Gives prize to winner
        /// </summary>
        /// <param name="Winner"></param>
        public static void GivePrize(int Winner)
        {
            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Winner);

            if (Client != null && Client.GetHabbo() != null)
            {
                Client.GetHabbo().Credits += Prize;
                Client.GetHabbo().UpdateCreditsBalance();
                SendWinnerAlert(Client.GetHabbo().Username);
            }
            else
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `users` SET `credits` = (credits + @prize) WHERE `id` = @winner LIMIT 1");
                    dbClient.AddParameter("prize", Prize);
                    dbClient.AddParameter("winner", Winner);
                    dbClient.RunQuery();
                }

                var User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Winner);
                SendWinnerAlert(User.Username);
            }
        }

        /// <summary>
        /// Sends alert to all users of who won the lottery
        /// </summary>
        /// <param name="Winner"></param>
        public static void SendWinnerAlert(string Winner)
        {
            lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;

                    client.SendWhisper("[Alerta do HOTEL] [LOTERIA] " + Winner + " has just won the lottery!", 33);
                }
            }
        }

        /// <summary>
        /// Clears the existing lottery tickets from dictionary and database
        /// </summary>
        public static void ClearLottery()
        {
            LotteryTickets.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE TABLE `rp_lottery`");
            }
        }
    }
}