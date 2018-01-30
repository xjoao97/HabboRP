using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    class BuddyRequestsComposer : ServerPacket
    {
        public BuddyRequestsComposer(ICollection<MessengerRequest> Requests)
            : base(ServerPacketHeader.BuddyRequestsMessageComposer)
        {
            base.WriteInteger(Requests.Count);
            base.WriteInteger(Requests.Count);

            foreach (MessengerRequest Request in Requests)
            {
                base.WriteInteger(Request.From);
               base.WriteString(Request.Username);

                using (UserCache User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Request.From))
                {
                    base.WriteString(User != null ? User.Look : "");
                }
            }
        }
    }
}
