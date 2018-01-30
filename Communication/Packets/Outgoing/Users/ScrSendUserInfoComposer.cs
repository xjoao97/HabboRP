using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Users
{
    class ScrSendUserInfoComposer : ServerPacket
    {
        public ScrSendUserInfoComposer(GameClient Session)
            : base(ServerPacketHeader.ScrSendUserInfoMessageComposer)
        {
            int DisplayMonths = 0;
            int DisplayDays = 0;
            int DisplayHours = 0;

            /*if (Session.GetHabbo().GetSubscriptionManager().HasSubscription)
            {
                int i = (int)Math.Ceiling((PlusEnvironment.GetUnixTimestamp() - (double)Session.GetHabbo().GetSubscriptionManager().GetSubscription().ActivateTime) / 86400.0);
                double ExpireTime = Session.GetHabbo().GetSubscriptionManager().GetSubscription().ExpireTime;
                double CurrentTime = ExpireTime - PlusEnvironment.GetUnixTimestamp();
                int CurrentDay = (int)Math.Ceiling(CurrentTime / 86400.0);

                int Idk = CurrentDay / 31;
                if (Idk >= 1)
                    Idk--;

                DisplayMonths = Idk;
                DisplayDays = CurrentDay - Idk * 31;
                DisplayHours = (DisplayMonths * 30 + DisplayDays) * 24;

                base.WriteInteger(DisplayDays);
                base.WriteInteger(2);
                base.WriteInteger(DisplayMonths);
                base.WriteInteger(1);
                base.WriteBoolean(true); // hc
                base.WriteBoolean(true); // vip
                base.WriteInteger(i);
                base.WriteInteger(i);
                base.WriteInteger(DisplayHours);
            }*/

            DisplayMonths = 12;
            DisplayDays = 100;
            DisplayHours = (DisplayMonths * 30 + DisplayDays) * 24;

            base.WriteString("habbo_club");
            base.WriteInteger(DisplayDays);
            base.WriteInteger(0);
            base.WriteInteger(DisplayMonths);
            base.WriteInteger(0);
            base.WriteBoolean(true); // hc
            base.WriteBoolean(true); // vip
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(DisplayHours);

            //Session.SendMessage(new Handshake.UserRightsComposer(Session, Session.GetHabbo().Rank));
        }
    }
}
