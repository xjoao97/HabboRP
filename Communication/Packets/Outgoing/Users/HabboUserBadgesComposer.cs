using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Badges;

namespace Plus.Communication.Packets.Outgoing.Users
{
    class HabboUserBadgesComposer : ServerPacket
    {
        public HabboUserBadgesComposer(Habbo Habbo, RoleplayBot Bot = null)
            : base(ServerPacketHeader.HabboUserBadgesMessageComposer)
        {
            if (Bot != null)
            {
                base.WriteInteger(Bot.Id + 1000000);
                base.WriteInteger(1);
                base.WriteInteger(1);
                base.WriteString("BOT");
            }
            else
            {
                base.WriteInteger(Habbo.Id);
                base.WriteInteger(Habbo.GetBadgeComponent().EquippedCount);

                foreach (Badge Badge in Habbo.GetBadgeComponent().GetBadges().ToList())
                {
                    if (Badge.Slot <= 0)
                        continue;

                    base.WriteInteger(Badge.Slot);
                    base.WriteString(Badge.Code);
                }
            }
        }
    }
}
