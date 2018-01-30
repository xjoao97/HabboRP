using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Farming;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class OffersCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_offers_list"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Vê as ofertas que você recebeu, se houver."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder().Append("<----- Ofertas ativas atualmente ----->\n\n");

            if (Session.GetRoleplay().OfferManager.ActiveOffers.Count <= 0)
                Message.Append("Você atualmente não tem ofertas ativas!\n");
            else
                Message.Append("Digite ':aceitar OFERTA' (As OFERTAS são [celular/casamento/gangue/roupa, e outras]!\n\n");

            lock (Session.GetRoleplay().OfferManager.ActiveOffers.Values)
            {
                foreach (var Offer in Session.GetRoleplay().OfferManager.ActiveOffers.Values)
                {
                    if (Offer == null)
                        continue;

                    string Name = "";
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    { 
                        if (Offer.Type.ToLower() == "sementes" && Offer.Params.ToList().Count == 1)
                        {
                            var OffererCache = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Offer.OffererId);
                            Name = OffererCache.Username;
                        }
                        else
                        {
                            RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                            Name = "[BOT] " + Bot.GetBotRoleplay().Name;
                        }
                    }
                    else
                    {
                        var OffererCache = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(Offer.OffererId);
                        Name = OffererCache.Username;
                    }

                    string PhoneName = RoleplayManager.GetPhoneName(Session, true);
                    string CarName = RoleplayManager.GetCarName(Session, true);

                    if (Offer.Type.ToLower() == "casamento")
                        Message.Append("Casamento: " + Name + " pediu sua mão em casamento!\n\n");
                    else if (Offer.Type.ToLower() == "sacodesementes")
                        Message.Append("Saco de Sementes: Um Saco de Sementes por R$" + String.Format("{0:N0}", Offer.Cost) + " de " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "sacodeplantas")
                        Message.Append("Saco de Plantas: Um Saco de Plantas por R$" + String.Format("{0:N0}", Offer.Cost) + " de " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "sementes")
                    {
                        FarmingItem Item = (FarmingItem)Offer.Params[1];

                        ItemData Furni;
                        if (PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni))
                            Message.Append("Sementes: " + Name + " ofereceu para você " + Offer.Cost + " " + Furni.PublicName + " sementes por R$" + String.Format("{0:N0}", (Offer.Cost * Item.BuyPrice)) + "!\n\n");
                    }
                    else if (Offer.Type.ToLower() == "desconto")
                        Message.Append("Roupas: Desconto de 5% para comprar roupas de " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "celular")
                        Message.Append("Celular: Um Nokia Tijolão por " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "carro")
                        Message.Append("Carro: Um Toyota Corolla por $1,000 from " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "cheques")
                        Message.Append("Cheques: Uma conta de Cheques no branco grátis de " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "poupanca")
                        Message.Append("Poupança: Uma conta Poupança por R$" + String.Format("{0:N0}", Offer.Cost) + " de " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "uparcel")
                        Message.Append("Atualização de Celular: Uma atualização para o seu " + PhoneName + " por R$" + String.Format("{0:N0}", Offer.Cost) + " de " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "uparcarro")
                        Message.Append("Atualização de Carro: Uma atualização para o seu " + CarName + " por R$" + String.Format("{0:N0}", Offer.Cost) + " de " + Name + "!\n\n");
                    else if (Offer.Type.ToLower() == "gangue")
                    {
                        var Gang = Groups.GroupManager.GetGang(Offer.Cost);

                        if (Gang != null)
                            Message.Append("Gangue: Convidado para se juntar à gangue '" + Gang.Name + "' por " + Name + "!\n\n");
                    }
                    else if (Offer.Type.ToLower() == "emprego")
                    {
                        var Job = Groups.GroupManager.GetJob(Offer.Cost);
                        var JobRank = Groups.GroupManager.GetJobRank(Job.Id, 1);

                        if (Job != null)
                            Message.Append("Emprego: Convidado para se juntar a Empresa '" + Job.Name + "' como um '" + JobRank.Name + "' por " + Name + "!\n\n");
                    }
                    else if (WeaponManager.Weapons.ContainsKey(Offer.Type.ToLower()))
                    {
                        Weapon weapon = WeaponManager.Weapons[Offer.Type.ToLower()];

                        if (weapon != null)
                            Message.Append("Armas: Uma " + weapon.PublicName + " por R$" + String.Format("{0:N0}", weapon.Cost) + " de " + Name + "!\n\n");
                    }
                }
            }
            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}