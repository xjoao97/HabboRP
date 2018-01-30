using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Bounties
{
    class BountyListCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_bounty_list"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Fornece uma lista de todos os usuários de recompensas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder().Append("<----- Usuários com Recompensas ----->\n\n");

            if (BountyManager.BountyUsers.Count <= 0)
                Message.Append("Ninguém está em uma recompensa agora!\n");

            lock (BountyManager.BountyUsers.Values)
            {
                foreach (Bounty Bounty in BountyManager.BountyUsers.Values)
                {
                    if (PlusEnvironment.GetUnixTimestamp() > Bounty.ExpiryTimeStamp)
                    {
                        BountyManager.RemoveBounty(Bounty.UserId, true);
                        Habbo BountyOwner = PlusEnvironment.GetHabboById(Convert.ToInt32(Bounty.AddedBy));

                        if (BountyOwner == null || BountyOwner.GetClient() == null)
                            continue;

                        BountyOwner.GetClient().SendWhisper("A recompensa que você definiu " + PlusEnvironment.GetHabboById(Convert.ToInt32(Bounty.UserId)).Username + " expirou!", 1);
                    }

                    TimeSpan Difference = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Bounty.ExpiryTimeStamp).Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(PlusEnvironment.GetUnixTimestamp()));

                    Message.Append("Usuário: " + PlusEnvironment.GetHabboById(Convert.ToInt32(Bounty.UserId)).Username + " - Expira em " + ((int)Difference.TotalMinutes < 0 ? 0 : (int)Difference.TotalMinutes) + " minutos\n");
                    Message.Append("Recompensa: R$" + String.Format("{0:N0}", Bounty.Reward) + "\n\n");
                }
            }
            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}