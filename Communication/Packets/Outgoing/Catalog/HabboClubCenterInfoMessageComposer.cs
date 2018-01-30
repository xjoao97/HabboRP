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
    class HabboClubCenterInfoMessageComposer : ServerPacket
    {
        public HabboClubCenterInfoMessageComposer(GameClient Session) 
            : base(ServerPacketHeader.HabboClubCenterInfoMessageComposer)
        {
            // Streak duration.
            DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Session.GetHabbo().AccountCreated).ToLocalTime();
            TimeSpan Difference = DateTime.Now - Origin;

            base.WriteInteger((int)Difference.TotalDays); // Streak duration
            base.WriteString(Origin.ToString("dd-MM-yyyy")); // First joined HC
            base.WriteInteger(1069128089); // Long Value
            base.WriteInteger(-1717986918); // Long Value
            base.WriteInteger(0); // Idk
            base.WriteInteger(61); // Idk
            base.WriteInteger(0); // Credits Spent
            base.WriteInteger(15); // Streak Bonus
            base.WriteInteger(0); // Bonus Amount (Due to 10% bonus spent) --- (Streak Bonus + This = Total Credits)
            base.WriteInteger(40320); // Idk
        }
    }
}
