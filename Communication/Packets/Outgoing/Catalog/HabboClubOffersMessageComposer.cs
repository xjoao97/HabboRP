using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    class HabboClubOffersMessageComposer : ServerPacket
    {
        public HabboClubOffersMessageComposer(GameClient Session, int WindowId) 
            : base(ServerPacketHeader.HabboClubOffersMessageComposer)
        {
            base.WriteInteger(PlusEnvironment.GetGame().GetCatalog().ClubItems().Count);

            foreach (CatalogItem Item in PlusEnvironment.GetGame().GetCatalog().ClubItems())
            {
                base.WriteInteger(Item.Id);
                base.WriteString(Item.Name);
                base.WriteBoolean(false);
                base.WriteInteger(Item.CostCredits);

                if (Item.CostDiamonds > 0)
                {
                    base.WriteInteger(Item.CostDiamonds);
                    base.WriteInteger(105);
                }
                else
                {
                    base.WriteInteger(Item.CostPixels);
                    base.WriteInteger(0);
                }

                base.WriteBoolean(true);

                string[] data = Item.Name.Split('_');
                double dayTime = 31;

                int Amount;
                if (int.TryParse(data[3], out Amount))
                {
                    if (data[4].ToLower() == "day")
                        dayTime = Amount;
                    else if (data[4].ToLower() == "month")
                        dayTime = Amount * 31;
                    else if (data[4].ToLower() == "year")
                        dayTime = Amount * 12 * 31;
                }

                DateTime newExpiryDate = DateTime.Now.AddDays(dayTime);

                if (Session.GetHabbo().GetSubscriptionManager().HasSubscription)
                    newExpiryDate = PlusEnvironment.UnixTimeStampToDateTime(Session.GetHabbo().GetSubscriptionManager().GetSubscription().ExpireTime).AddDays(dayTime);

                base.WriteInteger((int)dayTime / 31);
                base.WriteInteger((int)dayTime);
                base.WriteBoolean(false);
                base.WriteInteger((int)dayTime);
                base.WriteInteger(newExpiryDate.Year);
                base.WriteInteger(newExpiryDate.Month);
                base.WriteInteger(newExpiryDate.Day);
            }
            base.WriteInteger(WindowId);
        }
    }
}
