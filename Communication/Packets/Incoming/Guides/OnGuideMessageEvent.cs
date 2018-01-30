using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.Communication.Packets.Outgoing.Guides;
using Plus.Utilities;
using Plus.HabboHotel.Rooms.Chat.Commands;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class OnGuideMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int UserId = Packet.PopInt();
            string Message = Packet.PopString();

            GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
            List<GameClient> HandlingCalls = guideManager.HandlingCalls();

            if (HandlingCalls.Count < 1)
            {
                Session.SendMessage(new OnGuideSessionError());
                return;
            }

            CryptoRandom Random = new CryptoRandom();
            GameClient RandomPolice = null;

            if (HandlingCalls.Count > 1)
                RandomPolice = HandlingCalls[Random.Next(0, HandlingCalls.Count)];
            else
                RandomPolice = HandlingCalls[0];

            if (RandomPolice == null)
            {
                Session.SendMessage(new OnGuideSessionError());
                return;
            }

            Session.SendMessage(new OnGuideSessionAttachedComposer(Session.GetHabbo().Id, Message, 30));
            RandomPolice.SendMessage(new OnGuideSessionAttachedComposer(Session.GetHabbo().Id, Message, 15));

            Session.GetRoleplay().SentRealCall = true;
            Session.GetRoleplay().Sent911Call = true;
            Session.GetRoleplay().CallMessage = Message;
            Session.GetRoleplay().GuideOtherUser = RandomPolice;
            RandomPolice.GetRoleplay().GuideOtherUser = Session;
        }
    }
}
