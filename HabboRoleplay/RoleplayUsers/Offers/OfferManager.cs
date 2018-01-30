using System;
using System.Linq;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Bots;

namespace Plus.HabboRoleplay.RoleplayUsers.Offers
{
    public class OfferManager
    {
        /// <summary>
        /// The client
        /// </summary>
        public GameClient Client;

        /// <summary>
        /// Contains all active offers
        /// </summary>
        public ConcurrentDictionary<string, RoleplayOffer> ActiveOffers;

        /// <summary>
        /// Constructs our manager
        /// </summary>
        public OfferManager(GameClient Client)
        {
            this.Client = Client;
            ActiveOffers = new ConcurrentDictionary<string, RoleplayOffer>();
        }

        /// <summary>
        /// Creates an offer
        /// </summary>
        public void CreateOffer(string Type, int OffererId, int Cost, params object[] Params)
        {
            if (ActiveOffers.ContainsKey(Type))
                return;

            RoleplayOffer Offer = new RoleplayOffer(Type, OffererId, Cost, Params);

            if (Offer == null)
                return;

            ActiveOffers.TryAdd(Type, Offer);
        }

        /// <summary>
        /// Removes all of the offers
        /// </summary>
        public void EndAllOffers()
        {
            foreach (RoleplayOffer Offer in ActiveOffers.Values.ToList())
            {
                RoleplayOffer Junk;
                ActiveOffers.TryRemove(Offer.Type.ToLower(), out Junk);
            }
        }
    }
}