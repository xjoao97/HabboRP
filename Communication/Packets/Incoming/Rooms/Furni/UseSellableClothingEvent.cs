using System;
using System.Linq;
using System.Text;
using System.Threading;

using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Catalog.Clothing;

using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Database.Interfaces;
using Plus.HabboRoleplay.RoleplayUsers.Offers;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    class UseSellableClothingEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null|| !Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;

            if (Room == null)
                return;

            int ItemId = Packet.PopInt();

            Item Item = Room.GetRoomItemHandler().GetItem(ItemId);

            if (Item == null)
            {
                Session.GetRoleplay().PurchasingClothing = true;
                return;
            }

            if (Item.Data == null)
            {
                Session.GetRoleplay().PurchasingClothing = true;
                return;
            }

            if (Item.Data.InteractionType != InteractionType.PURCHASABLE_CLOTHING)
            {
                Session.SendNotification("Opa, este item não está configurado como um item de roupa!");
                Session.GetRoleplay().PurchasingClothing = true;
                return;
            }

            if (Item.Data.ClothingId == 0)
            {
                Session.SendNotification("Opa, este item não tem uma configuração de roupa de ligação, por favor, informe à Equipe!");
                Session.GetRoleplay().PurchasingClothing = true;
                return;
            }

            ClothingItem Clothing = null;
            if (!PlusEnvironment.GetGame().GetCatalog().GetClothingManager().TryGetClothing(Item.Data.ClothingId, out Clothing))
            {
                Session.SendNotification("Opa, não conseguimos encontrar esta peça de roupa!");
                Session.GetRoleplay().PurchasingClothing = true;
                return;
            }

            if (Session.GetRoleplay().PurchasingClothing)
                return;

            if (Session.GetRoleplay().Clothing != Clothing)
            {
                string Discount = "";
                if (Session.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("clothing"))
                {
                    int Bonus = Convert.ToInt32((double)Clothing.Cost * 0.01);
                    int NewCost = Clothing.Cost - Bonus;

                    Discount = " (R$" + String.Format("{0:N0}", NewCost) + " devido ao desconto de roupa de 5%)";
                }

                Session.SendWhisper("Essa roupa vai custar R$" + String.Format("{0:N0}", Clothing.Cost) + "" + (Discount == null ? "" : Discount) + "! Clique em 'Comprar Roupa' se você realmente quer comprar!", 1);
                Session.GetRoleplay().Clothing = Clothing;
                Session.GetRoleplay().PurchasingClothing = true;
                return;
            }

            if (Session.GetHabbo().Credits < Clothing.Cost)
            {
                Session.SendWhisper("Desculpe, você não tem dinheiro suficiente para comprar esta roupa!", 1);
                Session.GetRoleplay().PurchasingClothing = true;
                return;
            }

            Session.GetRoleplay().Clothing = null;
            Session.GetHabbo().GetClothing().AddClothing(Clothing.ClothingName, Clothing.PartIds);
            Session.SendMessage(new FigureSetIdsComposer(Session.GetHabbo().GetClothing().GetClothingAllParts));
            Session.Shout("*Comprou " + Item.GetBaseItem().PublicName + " por R$" + String.Format("{0:N0}", Clothing.Cost) + "*", 4);
            //Session.SendMessage(new RoomNotificationComposer("figureset.redeemed.success"));
            Session.SendMessage(new RoomNotificationComposer("purchased_clothing", "message", "Ual! Você comprou com sucesso o '" + Item.GetBaseItem().PublicName + "' por R$" + String.Format("{0:N0}", Clothing.Cost) + "!"));
            Session.SendWhisper("Se, por algum motivo, não puder ver sua nova roupa, recarregue o RP!", 1);

            #region Clothing Discount Check
            if (Session.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("clothing"))
            {
                if (Clothing.Cost > 0)
                {
                    RoleplayOffer Offer = Session.GetRoleplay().OfferManager.ActiveOffers["clothing"];
                    int Bonus = Convert.ToInt32((double)Clothing.Cost * 0.05);

                    if (Offer.Params == null)
                    {
                        var Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);

                        if (Offerer != null && Offerer.GetHabbo() != null)
                        {
                            Offerer.GetHabbo().Credits += Bonus;
                            Offerer.GetHabbo().UpdateCreditsBalance();
                            Offerer.SendWhisper("Você recebeu um bônus de R$" + String.Format("{0:N0}", Bonus) + ", " + Session.GetHabbo().Username + " quando comprou " + Item.GetBaseItem().PublicName + "!", 1);
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_Discounting", 1);
                        }
                    }

                    RoleplayOffer Junk;
                    Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("clothing", out Junk);

                    int NewCost = Clothing.Cost - Bonus;

                    if (NewCost > 0)
                    {
                        Session.GetHabbo().Credits -= NewCost;
                        Session.GetHabbo().UpdateCreditsBalance();
                    }
                    return;
                }
                else
                {
                    Session.SendWhisper("Você ainda tem o desconto para a próxima vez que você comprar um item de vestuário!", 1);
                    return;
                }
            }
            else
            {
                if (Clothing.Cost > 0)
                {
                    Session.GetHabbo().Credits -= Clothing.Cost;
                    Session.GetHabbo().UpdateCreditsBalance();
                }
            }
            #endregion

            return;
        }
    }
}
