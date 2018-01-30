using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Users;

namespace Plus.Communication.Packets.Incoming.Users
{
    class GetRelationshipsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int UserId = Packet.PopInt();

            if (UserId > 1000000)
            {
                int BotId = UserId - 1000000;
                var Bot = RoleplayBotManager.GetCachedBotById(BotId);

                if (Bot != null)
                    Session.SendMessage(new GetRelationshipsComposer(null, Bot));
            }
            else
            {
                Habbo Habbo = PlusEnvironment.GetHabboById(UserId);

                if (Habbo == null)
                    return;


                #region Open user statistics dialogue (sockets)

                if (Habbo.GetClient() != null)
                {
                    if (Habbo.GetClient() != Session)
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_characterbar", "" + Habbo.Id);
                    }
                }

                #endregion

                Session.SendMessage(new GetRelationshipsComposer(Habbo));
            }
        }
    }
}
