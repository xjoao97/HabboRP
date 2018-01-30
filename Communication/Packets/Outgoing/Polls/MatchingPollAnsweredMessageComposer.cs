using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Polls;
using Plus.HabboHotel.Polls.Enums;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Polls
{
    class MatchingPollAnsweredMessageComposer : ServerPacket
    {
        public MatchingPollAnsweredMessageComposer(GameClient Session, string text)
            : base(ServerPacketHeader.MatchingPollAnsweredMessageComposer)
        {
            base.WriteInteger(Session.GetHabbo().Id);
            base.WriteString(text);
            base.WriteInteger(0);
        }
    }
}
