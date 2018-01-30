using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Polls;
using Plus.Communication.Packets.Outgoing.Polls;

namespace Plus.Communication.Packets.Incoming.Polls
{
    class AcceptPollMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int key = Packet.PopInt();
            Poll poll = null;

            if (key == 500000)
            {
                string PollName = "HabboRPG Caixa";
                string PollInvitation = "HabboRPG Caixa";
                string PollThanks = "Obrigado por usar o Caixa HabboRPG!";

                poll = new Poll(500000, 0, PollName, PollInvitation, PollThanks, "", 1, null);
            }
            else
                poll = PlusEnvironment.GetGame().GetPollManager().Polls[key];

            Session.SendMessage(new PollQuestionsMessageComposer(Session, poll));
        }
    }
}
