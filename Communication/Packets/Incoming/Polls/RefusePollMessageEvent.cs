using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;

namespace Plus.Communication.Packets.Incoming.Polls
{
    class RefusePollMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int num = Packet.PopInt();

            if (num == 500000)
            {
                if (!Session.GetRoomUser().CanWalk)
                    Session.GetRoomUser().CanWalk = true;

                if (Session.GetRoleplay().ATMFailed)
                    Session.SendNotification("Erro... O ATM Machine não respondeu ao seu pedido.");

                Session.GetRoleplay().ATMAccount = "";
                Session.GetRoleplay().ATMAction = "";
                Session.GetRoleplay().ATMFailed = false;
                return;
            }

            if (!Session.GetHabbo().AnsweredPolls.Contains(num))
            {
                Session.GetHabbo().AnsweredPolls.Add(num);
                Session.SendNotification("Você recusou a enquete! Para poder respondê-lo mais tarde, recarregue o RP!");
            }
        }
    }
}
