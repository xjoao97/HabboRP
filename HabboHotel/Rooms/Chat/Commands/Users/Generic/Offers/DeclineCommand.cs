using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Farming;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class DeclineCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_decline"; }
        }

        public string Parameters
        {
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Recusa a oferta com base no tipo desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira um tipo de oferta, ':recusar (oferta)'. Verifique :ofertas para saber oque recusar!", 1);
                return;
            }

            string Type = Params[1];

            if (Session.GetRoleplay().OfferManager.ActiveOffers.Count <= 0)
            {
                Session.SendWhisper("Você não tem ofertas para recusar!", 1);
                return;
            }

            Weapon weapon = null;
            if (Type.ToLower() == "arma")
            {
                if (Session.GetRoleplay().OfferManager.ActiveOffers.Values.Where(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).ToList().Count > 0)
                    weapon = WeaponManager.getWeapon(Session.GetRoleplay().OfferManager.ActiveOffers.Values.FirstOrDefault(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).Type.ToLower());
            }

            if (Type.ToLower() == "cheques")
                Type = "poupanca";

            if (Session.GetRoleplay().OfferManager.ActiveOffers.ContainsKey(Type.ToLower()) || weapon != null)
            {
                RoleplayOffer Offer;
                if (weapon == null)
                    Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                else
                    Offer = Session.GetRoleplay().OfferManager.ActiveOffers[weapon.Name.ToLower()];

                if (Offer.Params != null && Offer.Params.Length > 0)
                {
                    if (Offer.Type.ToLower() == "sementes")
                    {
                        if (Offer.Params.Length > 1)
                        {
                            RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                            Session.Shout("*Recuso a oferta " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de " + Bot.GetBotRoleplay().Name + "*", 4);
                        }
                        else
                            Session.Shout("*Recuso a oferta " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de " + PlusEnvironment.GetHabboById(Offer.OffererId).Username + "*", 4);
                    }
                    else
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        Session.Shout("*Recuso a oferta " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de " + Bot.GetBotRoleplay().Name + "*", 4);
                    }
                }
                else
                    Session.Shout("*Recuso a oferta " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + " de " + PlusEnvironment.GetHabboById(Offer.OffererId).Username + "*", 4);

                RoleplayOffer Junk;
                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove(Offer.Type.ToLower(), out Junk);
            }
            else
            {
                Session.SendWhisper("Você não tem uma oferta de " + (Type.Substring(0, 1).ToUpper() + Type.Substring(1)) + "! Verifique ':ofertas' e veja todas ativas!", 1);
                return;
            }
        }
    }
}