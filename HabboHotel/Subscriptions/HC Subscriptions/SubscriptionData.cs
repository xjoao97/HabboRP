using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Subscriptions.HC_Subscriptions
{
    public class SubscriptionData
    {
        public int SubscriptionId { get; set; }
        public Int64 ExpireTime { get; set; }
        public Int64 ActivateTime { get; set; }
        public Int64 LastGiftTime { get; set; }
        public bool IsValid { get { return ExpireTime > PlusEnvironment.GetUnixTimestamp(); } }
        public SubscriptionData(int id, Int64 activated, Int64 timeExpire, Int64 timeLastGift)
        {
            SubscriptionId = id;
            ActivateTime = activated;
            ExpireTime = timeExpire;
            LastGiftTime = timeLastGift;
        }
    }
}
