using System;
using System.Collections.Generic;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Guides;
using Plus.Communication.Packets.Outgoing.Guides;

namespace Plus.Communication.Packets.Incoming.Guides
{
    class OnGuideSessionDetachedMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var state = Packet.PopBoolean();
            var requester = Session.GetRoleplay().GuideOtherUser;

            if (requester == null || requester.GetHabbo() == null || requester.GetRoleplay() == null)
            {
                Session.GetRoleplay().GuideOtherUser = null;
                Session.SendMessage(new OnGuideSessionDetachedComposer(0));
                Session.SendMessage(new OnGuideSessionDetachedComposer(1));
                return;
            }

            if (!state)
            {
                Session.GetRoleplay().GuideOtherUser = null;
                Session.SendMessage(new OnGuideSessionDetachedComposer(0));
                Session.SendMessage(new OnGuideSessionDetachedComposer(1));

                requester.SendWhisper("Um policial recusou seu pedido de ajuda!", 1);

                GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();
                List<GameClient> Handlers = null;

                Handlers = guideManager.HandlingCalls();

                if (Handlers.Count < 2)
                {
                    requester.SendMessage(new OnGuideSessionError());

                    requester.GetRoleplay().Sent911Call = false;
                    requester.GetRoleplay().CallMessage = "";
                    requester.GetRoleplay().GuideOtherUser = null;
                    return;
                }

                GameClient RandomPolice = Handlers[new CryptoRandom().Next(0, Handlers.Count)];

                while (RandomPolice == Session)
                {
                    RandomPolice = Handlers[new CryptoRandom().Next(0, Handlers.Count)];
                }

                if (RandomPolice == null)
                {
                    requester.SendMessage(new OnGuideSessionError());

                    requester.GetRoleplay().Sent911Call = false;
                    requester.GetRoleplay().CallMessage = "";
                    requester.GetRoleplay().GuideOtherUser = null;
                    return;
                }

                requester.SendWhisper("Sua nova chamada para ajuda foi enviada!", 1);
                requester.GetRoleplay().Sent911Call = true;
                requester.GetRoleplay().GuideOtherUser = RandomPolice;

                RandomPolice.SendMessage(new OnGuideSessionAttachedComposer(requester.GetHabbo().Id, requester.GetRoleplay().CallMessage, 15));
                return;
            }

            requester.SendWhisper(Session.GetHabbo().Username + " está vindo ajudar você, espere um instante!", 1);
            requester.SendMessage(new OnGuideSessionStartedMessageComposer(Session, requester));
            Session.SendMessage(new OnGuideSessionStartedMessageComposer(Session, requester));
            Session.SendMessage(new OnGuideSessionMsgComposer(Session, "Vá ajudar esta pessoa, clique em Visitar rapidamente!"));

            requester.GetRoleplay().CallMessage = "";
            requester.GetRoleplay().Sent911Call = false;
        }
    }
}
