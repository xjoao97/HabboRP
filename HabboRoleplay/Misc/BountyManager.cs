using System;
using System.Data;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using log4net;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Users;

namespace Plus.HabboRoleplay.Misc
{
    public class BountyManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Misc.BountyManager");

        /// <summary>
        /// Thread-safe dictionary containing bounties
        /// </summary>
        public static ConcurrentDictionary<int, Bounty> BountyUsers = new ConcurrentDictionary<int, Bounty>();

        /// <summary>
        /// Generates the lottery dictionary values from database
        /// </summary>
        public static void Initialize()
        {
            BountyUsers.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_bounties`");
                DataTable Table = dbClient.getTable();

                if (Table != null)
                {
                    foreach (DataRow Row in Table.Rows)
                    {
                        int UserId = Convert.ToInt32(Row["user_id"]);
                        int AddedBy = Convert.ToInt32(Row["added_by"]);
                        int Reward = Convert.ToInt32(Row["reward"]);
                        double TimeStamp = Convert.ToDouble(Row["timestamp"]);
                        double ExpiryTimeStamp = Convert.ToDouble(Row["timestamp_expire"]);

                        Bounty Bounty = new Bounty(UserId, AddedBy, Reward, TimeStamp, ExpiryTimeStamp);

                        if (!BountyUsers.ContainsKey(UserId))
                            BountyUsers.TryAdd(UserId, Bounty);
                    }
                }
            }

            log.Info("Carregado " + BountyUsers.Count + " usuários com recompensa.");
        }

        /// <summary>
        /// Checks if the bounty list have the specific user id.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static bool BountyExists(int UserId)
        {
            if (BountyUsers.ContainsKey(UserId))
                return true;

            return false;
        }

        /// <summary>
        /// Adds the chosen client to the bounty
        /// </summary>
        /// <param name="New"></param>
        public static void AddBounty(Bounty New)
        {
            if (BountyUsers.ContainsKey(New.UserId))
                return;

            BountyUsers.TryAdd(New.UserId, New);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `rp_bounties` (`user_id`, `added_by`, `reward`, `timestamp`, `timestamp_expire`) VALUES (@userid, @addedby, @reward, @timestamp, @expirytimestamp)");
                dbClient.AddParameter("userid", New.UserId);
                dbClient.AddParameter("addedby", New.AddedBy);
                dbClient.AddParameter("reward", New.Reward);
                dbClient.AddParameter("timestamp", New.TimeStamp);
                dbClient.AddParameter("expirytimestamp", New.ExpiryTimeStamp);
                dbClient.RunQuery();
            }
        }

        /// <summary>
        /// Removes the chosen client from the bounty
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Expired"></param>
        public static void RemoveBounty(int UserId, bool Expired = false)
        {
            if (!BountyUsers.ContainsKey(UserId))
                return;

            Bounty Junk;
            BountyUsers.TryRemove(UserId, out Junk);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `rp_bounties` WHERE `user_id` = '" + UserId + "'");
            }

            if (!Expired)
            {
                try
                {
                    Habbo BountyOwner = PlusEnvironment.GetHabboById(Convert.ToInt32(Junk.AddedBy));

                    if (BountyOwner == null || BountyOwner.GetClient() == null)
                        return;

                    BountyOwner.Credits += Junk.Reward;
                    BountyOwner.UpdateCreditsBalance();
                }
                catch { }
            }
        }

        public static void CheckBounty(GameClient Session, int UserId)
        {
            try
            {
                if (Session == null || !BountyUsers.ContainsKey(UserId))
                    return;

                Bounty Junk;
                BountyUsers.TryRemove(UserId, out Junk);

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("DELETE FROM `rp_bounties` WHERE `user_id` = '" + UserId + "'");
                }

                if (PlusEnvironment.GetUnixTimestamp() < Junk.ExpiryTimeStamp)
                {
                    Session.Shout("*Reinvindica R$" + String.Format("{0:N0}", Junk.Reward) + " da recompensa de " + PlusEnvironment.GetHabboById(UserId).Username + "*", 4);
                    Session.SendWhisper("Você reivindicou com sucesso R$" + String.Format("{0:N0}", Junk.Reward) + " da recompensa de " + PlusEnvironment.GetHabboById(UserId).Username + "!", 1);

                    Session.GetHabbo().Credits += Junk.Reward;
                    Session.GetHabbo().UpdateCreditsBalance();
                    return;
                }
                else
                {
                    Session.SendWhisper("Você reivindicou a recompensa de " + PlusEnvironment.GetHabboById(UserId).Username + ", mas a recompensa expirou!", 1);
                    return;
                }
            }
            catch { }
        }

        /// <summary>
        /// Clears the existing bounties from dictionary and database
        /// </summary>
        public static void ClearBounties()
        {
            BountyUsers.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE TABLE `rp_bounties`");
            }
        }
    }

    public class Bounty
    {
        /// <summary>
        /// User Id of bounty.
        /// </summary>
        public int UserId;

        /// <summary>
        /// Added By of bounty.
        /// </summary>
        public int AddedBy;

        /// <summary>
        /// Reward of bounty.
        /// </summary>
        public int Reward;

        /// <summary>
        /// Timestamp of bounty.
        /// </summary>
        public double TimeStamp;

        /// <summary>
        /// Timestamp expiry of bounty.
        /// </summary>
        public double ExpiryTimeStamp;

        /// <summary>
        /// The variables.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="AddedBy"></param>
        /// <param name="Reward"></param>
        /// <param name="TimeStamp"></param>
        /// <param name="ExpiryTimeStamp"></param>
        public Bounty(int UserId, int AddedBy, int Reward, double TimeStamp, double ExpiryTimeStamp)
        {
            this.UserId = UserId;
            this.AddedBy = AddedBy;
            this.Reward = Reward;
            this.TimeStamp = TimeStamp;
            this.ExpiryTimeStamp = ExpiryTimeStamp;
        }
    }
}
