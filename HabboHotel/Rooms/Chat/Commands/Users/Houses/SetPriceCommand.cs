using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Apartment
{
    class SetPriceommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_house_set_price"; }
        }

        public string Parameters
        {
            get { return "%quantidade%"; }
        }

        public string Description
        {
            get { return "Define o preço de venda da sua casa para o valor desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            House House = PlusEnvironment.GetGame().GetHouseManager().GetHouseByOwnerId(Session.GetHabbo().Id);

            if (House == null)
            {
                Session.SendWhisper("Desculpe, mas você não possui uma casa!", 1);
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite um preço de venda para sua casa!", 1);
                return;
            }

            int Price;
            if (int.TryParse(Params[1], out Price))
            {
                House.UpdateCost(Price);
                Session.SendNotification("Você estabeleceu com sucesso o preço de venda da sua casa por R$" + String.Format("{0:N0}", Price) + "!");
                return;
            }
            else
            {
                Session.SendWhisper("Digite um preço de venda para sua casa!", 1);
                return;
            }
        }
    }
}
