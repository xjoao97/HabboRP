using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Polls.Enums;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Polls
{
    class MatchingPollResultMessageComposer : ServerPacket
    {
        public MatchingPollResultMessageComposer(Poll poll)
            : base(ServerPacketHeader.MatchingPollResultMessageComposer)
        {
            base.WriteInteger(poll.Id);
            base.WriteInteger(2);
            base.WriteString("0");
            base.WriteInteger(poll.AnswersNegative);
            base.WriteString("1");
            base.WriteInteger(poll.AnswersPositive);
        }
    }
}
