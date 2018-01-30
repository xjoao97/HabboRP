using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class GuideEndSession : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetRoleplay() == null)
                return;

            GameClient requester = Session.GetRoleplay().GuideOtherUser;

            if (Session.GetRoleplay().IsWorking)
            {
                if (requester != null)
                {
                    requester.SendMessage(new GuideSessionEndedMessageComposer(2));

                    if (requester.GetRoleplay() != null && !requester.GetRoleplay().SentRealCall)
                    {
                        requester.GetRoleplay().Sent911Call = false;
                        requester.GetRoleplay().GuideOtherUser = null;
                    }
                }

                Session.SendMessage(new GuideSessionEndedMessageComposer(2));
            }
            else
            {
                Session.SendMessage(new GuideSessionEndedMessageComposer(2));
                Session.GetRoleplay().Sent911Call = false;
                Session.GetRoleplay().GuideOtherUser = null;

                if (requester != null)
                    requester.SendMessage(new GuideSessionEndedMessageComposer(0));
            }
        }
    }
}
