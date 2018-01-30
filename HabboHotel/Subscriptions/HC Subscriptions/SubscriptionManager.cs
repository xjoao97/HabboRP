using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Subscriptions.HC_Subscriptions;
using Plus.HabboHotel.Users.UserDataManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Subscriptions.HC_Subscriptions
{
    public class SubscriptionManager
    {
        private readonly int UserID;
        private SubscriptionData _subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionManager"/> class.
        /// </summary>
        /// <param name="UserID">The user identifier.</param>
        /// <param name="userData">The user data.</param>
        internal SubscriptionManager(int userID, UserData userData)
        {
            UserID = userID;
            _subscription = userData.Subscriptions;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has subscription.
        /// </summary>
        /// <value><c>true</c> if this instance has subscription; otherwise, <c>false</c>.</value>
        internal bool HasSubscription
        {
            get { return _subscription != null && _subscription.IsValid; }
        }

        /// <summary>
        /// Gets the subscription.
        /// </summary>
        /// <returns>Subscription.</returns>
        internal SubscriptionData GetSubscription()
        {
            return _subscription;
        }

        /// <summary>
        /// Adds the subscription.
        /// </summary>
        /// <param name="dayLength">Length of the day.</param>
        internal void AddSubscription(double dayLength)
        {
            Console.WriteLine("Olá, sou eu!");
            int dayTime = Convert.ToInt32(Math.Round(dayLength));
            GameClient Session = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserID);

            DateTime Target;
            Int64 Expire;
            Int64 LastGift;
            if (_subscription != null)
            {
                Target = PlusEnvironment.UnixTimeStampToDateTime(_subscription.ExpireTime).AddDays(dayTime);
                Expire = _subscription.ActivateTime;
                LastGift = _subscription.LastGiftTime;
            }
            else
            {
                Target = DateTime.Now.AddDays(dayTime);
                Expire = (long)PlusEnvironment.GetUnixTimestamp();
                LastGift = (long)PlusEnvironment.GetUnixTimestamp();
            }

            long UnixTimestamp = PlusEnvironment.DateTimeToUnixTimeStamp(Target);

            _subscription = new SubscriptionData(2, Expire, UnixTimestamp, LastGift);

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("REPLACE INTO `user_subscriptions` VALUES (@userid, '2', @expiry, @timestamp, @lastgift)");
                dbClient.AddParameter("userid", UserID);
                dbClient.AddParameter("expiry", Expire);
                dbClient.AddParameter("timestamp", UnixTimestamp);
                dbClient.AddParameter("lastgift", LastGift);
                dbClient.RunQuery();
            }

            Session.SendMessage(new ScrSendUserInfoComposer(Session));
        }
    }
}
