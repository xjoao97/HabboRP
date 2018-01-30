using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Purchasing
{
    class BuyTicketCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_purchasing_buy_ticket"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite-lhe comprar um bilhete de loteria."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int Cost = LotteryManager.Cost;
            int LotteryRoom = Convert.ToInt32(RoleplayData.GetData("bank", "room")); //bank = lottery
            #endregion

            #region Conditions
            if (Room.Id != LotteryRoom)
            {
                Session.SendWhisper("Você deve estar na loja da loteria para comprar um bilhete!", 1);
                return;
            }

            if (Session.GetHabbo().Credits < Cost)
            {
                Session.SendWhisper("Você não tem dinheiro suficiente para comprar um bilhete!", 1);
                return;
            }

            if (LotteryManager.LotteryTickets.ContainsKey(Session.GetHabbo().Id))
            {
                Session.SendWhisper("Você já comprou um bilhete para esta loteria!", 1);
                return;
            }

            if (LotteryManager.LotteryFull())
            {
                Session.SendWhisper("Desculpe, mas não há tickets para venda!", 1);
                return;
            }
            #endregion

            #region Execute
            int TicketId = LotteryManager.LotteryTickets.Count + 1;

            LotteryManager.LotteryTickets.TryAdd(Session.GetHabbo().Id, TicketId);
            Session.Shout("*Compra um bilhete de loteria da loja*", 4);

            Session.GetHabbo().Credits -= Cost;
            Session.GetHabbo().UpdateCreditsBalance();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `rp_lottery` VALUES (@user, @ticket)");
                dbClient.AddParameter("user", Session.GetHabbo().Id);
                dbClient.AddParameter("ticket", TicketId);
                dbClient.RunQuery();
            }
            #endregion
        }
    }
}