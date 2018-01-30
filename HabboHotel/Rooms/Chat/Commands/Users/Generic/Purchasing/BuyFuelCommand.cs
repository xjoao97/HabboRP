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
    class BuyFuelCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_purchasing_buy_fuel"; }
        }

        public string Parameters
        {
            get { return "%quantidade%"; }
        }

        public string Description
        {
            get { return "Compre combustível para o seu carro."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira a quantidade de combustível que deseja comprar!", 1);
                return;
            }

            int Amount;
            if (!int.TryParse(Params[1], out Amount))
            {
                Session.SendWhisper("Insira uma quantidade válida de combustível que você gostaria de comprar!", 1);
                return;
            }

            if (Amount < 10)
            {
                Session.SendWhisper("Você precisa comprar pelo menos 10 galões de combustível por vez!", 1);
                return;
            }

            Group Job = GroupManager.Jobs.Values.FirstOrDefault(x => x.Ranks.Count > 0 && x.Ranks.Values.FirstOrDefault().HasCommand("car"));

            if (Job == null || !Job.Ranks.Values.FirstOrDefault().CanWorkHere(Room.Id))
            {
                Session.SendWhisper("Você deve estar dentro da loja de carros para comprar combustível!", 1);
                return;
            }

            int Cost = Convert.ToInt32(Math.Floor((double)(Amount * 2) / 3));

            if (Session.GetHabbo().Credits < Cost)
            {
                Session.SendWhisper("Você não tem dinheiro suficiente para comprar " + String.Format("{0:N0}", Amount) + " galões de gasolina!", 1);
                return;
            }
            #endregion

            #region Execute
            Session.GetHabbo().Credits -= Cost;
            Session.GetHabbo().UpdateCreditsBalance();
            Session.GetRoleplay().CarFuel += Amount;

            Session.Shout("*Compra " + String.Format("{0:N0}", Amount) + " galões de gasolina por R$" + String.Format("{0:N0}", Cost) + "*", 4);
            return;
            #endregion
        }
    }
}