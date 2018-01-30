using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboRoleplay.Combat;

namespace Plus.Communication.Packets.Incoming.Users
{
    class GetSelectedBadgesEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {

            int UserId = Packet.PopInt();

            if (UserId > 1000000)
            {
                int BotId = UserId - 1000000;
                var Bot = RoleplayBotManager.GetCachedBotById(BotId);

                if (Bot != null)
                    Session.SendMessage(new HabboUserBadgesComposer(null, Bot));
            }
            else
            {
                Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                if (Habbo == null)
                    return;

                if (Session.GetRoleplay().CombatMode)
                {
                    if (Session.GetRoleplay().InCombat)
                    {
                        Session.GetRoleplay().InCombat = false;

                        if (Habbo.GetClient() != null && Habbo.GetClient().GetRoomUser() != null && !Habbo.GetClient().GetRoomUser().IsBot)
                        {
                            if (Session.GetRoleplay().EquippedWeapon == null)
                                CombatManager.GetCombatType("fist").Execute(Session, Habbo.GetClient());
                            else
                                CombatManager.GetCombatType("gun").Execute(Session, Habbo.GetClient());
                        }
                    }
                }

                Session.SendMessage(new HabboUserBadgesComposer(Habbo));
            }
        }
    }
}