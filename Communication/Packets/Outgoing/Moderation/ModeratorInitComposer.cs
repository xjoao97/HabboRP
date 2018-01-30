using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Support;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    class ModeratorInitComposer : ServerPacket
    {
        public ModeratorInitComposer(ICollection<string> UserPresets, ICollection<string> RoomPresets, Dictionary<string, List<ModerationPresetActionMessages>> UserActionPresets, ICollection<SupportTicket> Tickets)
            : base(ServerPacketHeader.ModeratorInitMessageComposer)
        {
            base.WriteInteger(Tickets.Count);
            foreach (SupportTicket ticket in Tickets.ToList())
            {
                base.WriteInteger(ticket.Id);
                base.WriteInteger(ticket.TabId);
                base.WriteInteger(1); // Type
                base.WriteInteger(114); // Category
                base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - Convert.ToInt32(ticket.Timestamp)) * 1000);
                base.WriteInteger(ticket.Score);
                base.WriteInteger(0);
                base.WriteInteger(ticket.SenderId);
                base.WriteString(ticket.SenderName);
                base.WriteInteger(ticket.ReportedId);
                base.WriteString(ticket.ReportedName);
                base.WriteInteger((ticket.Status == TicketStatus.PICKED) ? ticket.ModeratorId : 0);
                base.WriteString(ticket.ModName);
                base.WriteString(ticket.Message);
                base.WriteInteger(0);
                base.WriteInteger(0);
            }

            base.WriteInteger(UserPresets.Count);
            foreach (string pre in UserPresets)
            {
                base.WriteString(pre);
            }

            base.WriteInteger(0);
            {
                //Push a string, maybe locale for the new shit?
            }

            /*base.WriteInteger(UserActionPresets.Count);
            foreach (KeyValuePair<string, List<ModerationPresetActionMessages>> Cat in UserActionPresets.ToList())
            {
               base.WriteString(Cat.Key);
                base.WriteBoolean(true);
                base.WriteInteger(Cat.Value.Count);
                foreach (ModerationPresetActionMessages Preset in Cat.Value.ToList())
                {
                   base.WriteString(Preset.Caption);
                   base.WriteString(Preset.MessageText);
                    base.WriteInteger(Preset.BanTime); // Account Ban Hours
                    base.WriteInteger(Preset.IPBanTime); // IP Ban Hours
                    base.WriteInteger(Preset.MuteTime); // Mute in Hours
                    base.WriteInteger(0);//Trading lock duration
                   base.WriteString(Preset.Notice + "\n\nPlease Note: Avatar ban is an IP ban!");
                    base.WriteBoolean(false);//Show HabboWay
                }
            }*/

            base.WriteBoolean(true); // Ticket right
            base.WriteBoolean(true); // Chatlogs
            base.WriteBoolean(true); // User actions alert etc
            base.WriteBoolean(true); // Kick users
            base.WriteBoolean(true); // Ban users
            base.WriteBoolean(true); // Caution etc
            base.WriteBoolean(true); // Love you, Tom

            base.WriteInteger(RoomPresets.Count);
            foreach (string pre in RoomPresets)
            {
                base.WriteString(pre);
            }
        }
    }
}