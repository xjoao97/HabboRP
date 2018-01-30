using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Apartment
{
    class BuyApartmentCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_apartment_buy"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Compra o apartamento que você está atualmente."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            HabboRoleplay.Apartments.Apartment Apartment;

            if (!Room.TryGetApartment(out Apartment))
            {
                Session.SendWhisper("Desculpe, mas este quarto não é um apartamento!", 1);
                return;
            }

            if (!Apartment.ForSale)
            {
                Session.SendWhisper("Desculpe, mas este apartamento não está à venda!", 1);
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Tem certeza de que quer comprar este apartamento por R$" + Apartment.Cost + "? Digite ':comprarap sim' para confirmar!", 1);
                return;
            }
            else
            {
                if (Params[1].ToLower() != "sim")
                {
                    Session.SendWhisper("Tem certeza de que quer comprar este apartamento por R$" + Apartment.Cost + "? Digite ':comprarap sim' para confirmar!", 1);
                    return;
                }
                else
                {
                    if (Session.GetHabbo().Credits < Apartment.Cost)
                    {
                        Session.SendWhisper("You do not have the $" + Apartment.Cost + " to purchase this apartment!", 1);
                        return;
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "*Compra este apartamento por R$" + Apartment.Cost + "*", 4);
                        Session.GetHabbo().Credits -= Apartment.Cost;
                        Session.GetHabbo().UpdateCreditsBalance();

                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Apartment.OwnerId);

                        if (Client == null)
                        {
                            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `credits` = `credits` + @cost WHERE `id` = @userid");
                                dbClient.AddParameter("cost", Apartment.Cost);
                                dbClient.AddParameter("userid", Apartment.OwnerId);
                                dbClient.RunQuery();
                            }
                        }
                        else
                        {
                            Client.SendNotification("Seu apartamento em [Quarto ID: " + Apartment.RoomId + "] acabou de ser vendido para " + Session.GetHabbo().Id + " por R$" + Apartment.Cost + "!\n\nParabéns.");
                            Client.GetHabbo().Credits += Apartment.Cost;
                            Client.GetHabbo().UpdateCreditsBalance();
                        }

                        Apartment.BuyApartment(Session);

                        Room R;
                        if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Room.Id, out R))
                        {
                            List<RoomUser> UsersToReturn = Room.GetRoomUserManager().GetRoomUsers().ToList();
                            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Room, true);
                            foreach (RoomUser User in UsersToReturn)
                            {
                                if (User == null || User.GetClient() == null)
                                    continue;

                                RoleplayManager.SendUser(User.GetClient(), Room.Id, "O apartamento acabou de ser comprado por " + Session.GetHabbo().Username + "!");
                            }
                        }
                        return;
                    }
                }
            }
        }
    }
}
