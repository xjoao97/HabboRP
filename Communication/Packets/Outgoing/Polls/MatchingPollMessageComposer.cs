using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Polls.Enums;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Polls
{
    class MatchingPollMessageComposer : ServerPacket
    {
        public MatchingPollMessageComposer(Poll poll)
            : base(ServerPacketHeader.MatchingPollMessageComposer)
        {
            base.WriteString("MATCHING_POLL");
            base.WriteInteger(poll.Id);
            base.WriteInteger(poll.Id);
            base.WriteInteger(15580);
            base.WriteInteger(poll.Id);
            base.WriteInteger(29);
            base.WriteInteger(5);
            base.WriteString(poll.PollName);
        }
    }
}
