using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Polls.Enums;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Polls
{
    class SuggestPollMessageComposer : ServerPacket
    {
        public SuggestPollMessageComposer(Poll poll)
            : base(ServerPacketHeader.SuggestPollMessageComposer)
        {
            base.WriteInteger(poll.Id);
            base.WriteString(poll.PollName); // ?
            base.WriteString(poll.Thanks); // ?
            base.WriteString(poll.PollInvitation);
        }
    }
}
