using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Purchasing
{
    class BuyBulletsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_purchasing_buy_bullets"; }
        }

        public string Parameters
        {
            get { return "%quantidade%"; }
        }

        public string Description
        {
            get { return "Compre balas para suas armas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite a quantidade de balas que você gostaria de comprar!", 1);
                return;
            }

            int Amount;
            if (!int.TryParse(Params[1], out Amount))
            {
                Session.SendWhisper("Digite a quantidade de balas que você gostaria de comprar!", 1);
                return;
            }

            if (Amount < 10)
            {
                Session.SendWhisper("Você precisa comprar pelo menos 10 balas por vez!", 1);
                return;
            }

            Group Job = GroupManager.Jobs.Values.FirstOrDefault(x => x.Ranks.Count > 0 && x.Ranks.Values.FirstOrDefault().HasCommand("weapon"));

            if (Job == null || !Job.Ranks.Values.FirstOrDefault().CanWorkHere(Room.Id))
            {
                Session.SendWhisper("Você deve estar dentro da loja de armas para comprar balas!", 1);
                return;
            }

            int Cost = Convert.ToInt32(Math.Floor((double)Amount / 1));

            if (Session.GetHabbo().Credits < Cost)
            {
                Session.SendWhisper("Você não tem dinheiro suficiente para comprar " + String.Format("{0:N0}", Amount) + " balas!", 1);
                return;
            }
            #endregion

            #region Execute
            Session.GetHabbo().Credits -= Cost;
            Session.GetHabbo().UpdateCreditsBalance();
            Session.GetRoleplay().Bullets += Amount;

            Session.Shout("*Compra " + String.Format("{0:N0}", Amount) + " balas por R$" + String.Format("{0:N0}", Cost) + "*", 4);
            return;
            #endregion
        }
    }
}