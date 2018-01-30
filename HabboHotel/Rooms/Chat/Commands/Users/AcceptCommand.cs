using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers.Offers;
using Plus.HabboRoleplay.Weapons;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Farming;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Messenger;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class AcceptCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_accept"; }
        }

        public string Parameters
        {
            get { return "%tipo%"; }
        }

        public string Description
        {
            get { return "Aceita a oferta com base no tipo desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Introduza um tipo de oferta! Digite :ofertas Para ver suas ofertas atuais!", 1);
                return;
            }

            if (Session.GetRoleplay().OfferManager.ActiveOffers.Count <= 0)
            {
                Session.SendWhisper("Você não tem nenhuma oferta ativa para aceitar!", 1);
                return;
            }

            string Type = Params[1];
            if (CommandManager.MergeParams(Params, 1).ToLower() == "seed satchel")
                Type = "sacodesementes";
            else if (CommandManager.MergeParams(Params, 1).ToLower() == "plant satchel")
                Type = "sacodeplantas";

            Weapon weapon = null;
            if (Type.ToLower() == "arma")
            {
                if (Session.GetRoleplay().OfferManager.ActiveOffers.Values.Where(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).ToList().Count > 0)
                    weapon = WeaponManager.getWeapon(Session.GetRoleplay().OfferManager.ActiveOffers.Values.FirstOrDefault(x => WeaponManager.getWeapon(x.Type.ToLower()) != null).Type.ToLower());
            }

            if (Session.GetRoleplay().OfferManager.ActiveOffers.ContainsKey(Type.ToLower()) || Type.ToLower() == "arma" || Type.ToLower() == "cheques")
            {
                #region Weapons
                if (Type.ToLower() == "arma" || WeaponManager.Weapons.ContainsKey(Type.ToLower()))
                {
                    if (weapon != null)
                    {
                        var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[weapon.Name.ToLower()];

                        if (Offer.Params != null && Offer.Params.Length > 0)
                        {
                            RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                            if (Bot.GetRoomUser().RoomId != Room.Id)
                            {
                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                                Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " aceitar a oferta de armas!", 1);
                                return;
                            }
                            else if (weapon.Stock < 1)
                            {
                                Session.SendWhisper("Desculpe, mas esta arma ficou sem estoque!", 1);
                                return;
                            }
                            else if (Session.GetHabbo().Credits < Offer.Cost)
                            {
                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                                Session.SendWhisper("Desculpe você não pode ter recursos para um " + weapon.PublicName + "!", 1);
                                return;
                            }
                            else
                            {
                                if (!Session.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name))
                                {
                                    Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra o/a " + weapon.PublicName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                                    Session.GetHabbo().Credits -= Offer.Cost;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    WeaponManager.Weapons[weapon.Name].Stock--;
                                    RoleplayManager.AddWeapon(Session, weapon);
                                }
                                else
                                {
                                    Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra o/a " + weapon.PublicName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                                    Session.GetHabbo().Credits -= Offer.Cost;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    WeaponManager.Weapons[weapon.Name].Stock--;

                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        DB.SetQuery("UPDATE `rp_weapons_owned` SET `can_use` = '1' WHERE `user_id` = @userid AND `base_weapon` = @baseweapon LIMIT 1");
                                        DB.AddParameter("userid", Session.GetHabbo().Id);
                                        DB.AddParameter("baseweapon", weapon.Name.ToLower());
                                        DB.RunQuery();
                                    }

                                    Session.GetRoleplay().OwnedWeapons = null;
                                    Session.GetRoleplay().OwnedWeapons = Session.GetRoleplay().LoadAndReturnWeapons();
                                }

                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                                Bot.GetRoomUser().Chat("Obrigado por adquirir um(a) " + weapon.PublicName + " " + Session.GetHabbo().Username + "!", true);
                            }
                            return;
                        }
                        else
                        {
                            GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                            if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                            {
                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                                Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                                return;
                            }
                            else if (weapon.Stock < 1)
                            {
                                Session.SendWhisper("Desculpe, mas esta arma ficou sem estoque!", 1);
                                return;
                            }
                            else if (Session.GetHabbo().Credits < Offer.Cost)
                            {
                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                                Session.SendWhisper("Desculpe, você não pode ter recursos para um " + weapon.PublicName + "!", 1);
                                return;
                            }
                            else
                            {
                                if (!Session.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name))
                                {
                                    Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra o/a " + weapon.PublicName + " por  R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                                    Session.GetHabbo().Credits -= Offer.Cost;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    WeaponManager.Weapons[weapon.Name].Stock--;
                                    RoleplayManager.AddWeapon(Session, weapon);
                                }
                                else
                                {
                                    Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra o/a " + weapon.PublicName + " por  R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                                    Session.GetHabbo().Credits -= Offer.Cost;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    WeaponManager.Weapons[weapon.Name].Stock--;

                                    using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        DB.SetQuery("UPDATE `rp_weapons_owned` SET `can_use` = '1' WHERE `user_id` = @userid AND `base_weapon` = @baseweapon LIMIT 1");
                                        DB.AddParameter("userid", Session.GetHabbo().Id);
                                        DB.AddParameter("baseweapon", weapon.Name.ToLower());
                                        DB.RunQuery();
                                    }

                                    Session.GetRoleplay().OwnedWeapons = null;
                                    Session.GetRoleplay().OwnedWeapons = Session.GetRoleplay().LoadAndReturnWeapons();
                                }

                                int Bonus = 200;
                                if (Offer.Cost / 20 > 500)
                                    Bonus = 500;
                                else if (Offer.Cost / 20 > 200)
                                    Bonus = 200;
                                else
                                    Bonus = Offer.Cost / 20;

                                Offerer.GetHabbo().Credits += Bonus;
                                Offerer.GetHabbo().UpdateCreditsBalance();

                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove(weapon.Name.ToLower(), out Junk);
                                Offerer.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", Bonus) + " ao vender para " + Session.GetHabbo().Username + " um(a)" + weapon.PublicName + "!");
                                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingWeapon", 1);
                                return;
                            }
                        }
                    }
                    else
                    {
                        Session.SendWhisper("Você não tem uma oferta de arma!", 1);
                        return;
                    }
                }
                #endregion

                #region Phone
                else if (Type.ToLower() == "celular")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a oferta de telefone!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Session.SendWhisper("Desculpe, você não pode ter recursos para um Nokia Tijolão!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType > 0)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Você já tem um telefone!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " E compra um Nokia Tijolão por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType = 1;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por adquirir um Nokia Tijolão! " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para um Nokia Tijolão!!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType > 0)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Você já tem um telefone!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra um Nokia Tijolão! por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType = 1;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Offerer.SendWhisper("Você receber R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " à venda " + Session.GetHabbo().Username + " um(a) " + Offer.Type + "!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingPhone", 1);
                            return;
                        }
                    }
                }
                #endregion

                #region Phone Upgrade
                else if (Type.ToLower() == "uparcel")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    string PhoneName = RoleplayManager.GetPhoneName(Session, true);
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " Para aceitar a atualização do telefone!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para um " + PhoneName + "!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType >= 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Você já tem o melhor telefone possível!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e adquire a atualização do telefone para o " + PhoneName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType += 1;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por adquirir uma atualização do telefone " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Lamentamos que este utilizador tenha sessão iniciada ou não esteja na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Desculpe, você não pode ter recursos para um " + PhoneName + "!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType >= 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Você já tem o melhor telefone possível!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e adquire a atualização do telefone para o " + PhoneName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType += 1;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Offerer.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " por atualizar o telefone de " + Session.GetHabbo().Username + "!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingPhone", 1);
                            return;
                        }
                    }
                }
                #endregion

                #region Phone Credit
                else if (Type.ToLower() == "creditos")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " aceitar os créditos de telefone!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 2)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar créditos de telefone!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra " + String.Format("{0:N0}", Offer.Cost) + " créditos de telefone por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)Offer.Cost / 2))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)Offer.Cost / 2));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetHabbo().Duckets += Offer.Cost;
                            Session.GetHabbo().UpdateDucketsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por adquirir " + String.Format("{0:N0}", Offer.Cost) + " créditos de telefone " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 2)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para créditos do telefone!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra " + String.Format("{0:N0}", Offer.Cost) + " créditos de telefone por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)Offer.Cost / 2))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)Offer.Cost / 2));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetHabbo().Duckets += Offer.Cost;
                            Session.GetHabbo().UpdateDucketsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            return;
                        }
                    }
                }
                #endregion

                #region Bullets
                else if (Type.ToLower() == "balas")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " Para aceitar as balas! ", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 1)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar balas!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra " + String.Format("{0:N0}", Offer.Cost) + " balas por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)Offer.Cost / 1))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)Offer.Cost / 1));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().Bullets += Offer.Cost;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por adquirir " + String.Format("{0:N0}", Offer.Cost) + " balas " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 1)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            Session.SendWhisper("Desculpe, você não pode ter recursos para balas!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra " + String.Format("{0:N0}", Offer.Cost) + " balas por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)Offer.Cost / 1))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)Offer.Cost / 1));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().Bullets += Offer.Cost;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            return;
                        }
                    }
                }
                #endregion

                #region Seeds
                else if (Type.ToLower() == "sementes")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params.ToList().Count > 1)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        FarmingItem Item = (FarmingItem)Offer.Params[1];

                        ItemData Furni;
                        if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Session.SendWhisper("Desculpe, este item não existe!", 1);
                            return;
                        }

                        int Amount = Offer.Cost;
                        int Cost = (Amount * Item.BuyPrice);

                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " Comprar as sementes!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar " + String.Format("{0:N0}", Amount) + " " + Furni.PublicName + " sementes!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra " + String.Format("{0:N0}", Amount) + " " + Furni.PublicName + " sementes*", 4);
                            Session.GetHabbo().Credits -= Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            FarmingManager.IncreaseSatchelCount(Session, Item, Amount, false);

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado pela sua compra " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        FarmingItem Item = (FarmingItem)Offer.Params[0];
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);

                        ItemData Furni;
                        if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Session.SendWhisper("Desculpe, este item não existe!", 1);
                            return;
                        }

                        int Amount = Offer.Cost;
                        int Cost = (Amount * Item.BuyPrice);

                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar " + Amount + " " + Furni.PublicName + " sementes!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra " + String.Format("{0:N0}", Amount) + " " + Furni.PublicName + " sementes*", 4);
                            Session.GetHabbo().Credits -= Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            FarmingManager.IncreaseSatchelCount(Session, Item, Amount, false);

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            return;
                        }
                    }
                }
                #endregion

                #region Seed Satchel
                else if (Type.ToLower() == "sacodesementes")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Session.GetRoleplay().FarmingStats.HasSeedSatchel)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Você já tem uma Sacola de Sementes!", 1);
                            return;
                        }
                        else if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " Para aceitar a oferta de Sacola de Sementes!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, você não pode tem recusos para comrar uma Sacola de Sementes!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra uma Sacola de Sementes por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasSeedSatchel = true;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por ter adquirido uma Sacola de Sementes " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Session.GetRoleplay().FarmingStats.HasSeedSatchel)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Você já tem uma Sacola de Sementes!", 1);
                            return;
                        }
                        else if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para uma Sacola de Sementes!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra uma Sacola de Sementes por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasSeedSatchel = true;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Offerer.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " à venda " + Session.GetHabbo().Username + " uma Sacola de Sementes!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingSatchel", 1);
                            return;
                        }
                    }
                }
                #endregion

                #region Plant Satchel
                else if (Type.ToLower() == "sacodeplantas")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Session.GetRoleplay().FarmingStats.HasPlantSatchel)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Você já tem uma Sacola de Plantas!", 1);
                            return;
                        }
                        else if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " Para aceitar a oferta de uma Sacola de Plantas!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para uma Sacola de Plantas!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra uma Sacola de Plantas por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasPlantSatchel = true;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por ter adquirido uma Sacola de Plantas " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Session.GetRoleplay().FarmingStats.HasPlantSatchel)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Você já tem uma Sacola de Plantas!", 1);
                            return;
                        }
                        else if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Lamentamos que este utilizador tenha sessão iniciada ou não esteja na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Desculpe você não pode ter recursos para uma planta Satchel!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra uma Sacola de Plantas por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasPlantSatchel = true;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Offerer.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " à venda " + Session.GetHabbo().Username + " uma Sacola de Plantas!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingSatchel", 1);
                            return;
                        }
                    }
                }
                #endregion

                #region Car
                else if (Type.ToLower() == "carro")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " Para aceitar a oferta de carro!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Session.SendWhisper("Desculpe, você não pode ter recursos para um Toyota Corolla!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " E comprar um Toyota Corolla por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarType = 1;
                            Session.GetRoleplay().CarFuel = 300;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por adquirir um Toyota Corolla " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para um Toyota Corolla!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra um Toyota Corolla por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarType = 1;
                            Session.GetRoleplay().CarFuel = 300;

                            Offerer.GetHabbo().Credits += Offer.Cost / 20;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Offerer.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", (Offer.Cost / 20)) + " à venda " + Session.GetHabbo().Username + " um(a) " + Offer.Type + "!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingCar", 1);
                            return;
                        }
                    }
                }
                #endregion

                #region Car Upgrade
                else if (Type.ToLower() == "uparcarro")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    string CarName = RoleplayManager.GetCarName(Session, true);
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a atualização de carro!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Session.SendWhisper("Desculpe, você não pode ter recursos para um " + CarName + "!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().CarType >= 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Session.SendWhisper("Você já tem o melhor carro possível!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra a atualização do carro para o " + CarName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarType += 1;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por adquirir uma atualização de carro " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um " + CarName + "!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().CarType >= 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Session.SendWhisper("Você já tem o melhor carro possível!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra A atualização do carro para o " + CarName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarType += 1;

                            Offerer.GetHabbo().Credits += Offer.Cost / 20;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Offerer.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", (Offer.Cost / 20)) + " à venda de uma atualização de carro de " + Session.GetHabbo().Username + "!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingCar", 1);
                            return;
                        }
                    }
                }
                #endregion

                #region Fuel
                else if (Type.ToLower() == "gasolina")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " Aceitar a gasolina!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar gasolina!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra " + String.Format("{0:N0}", Offer.Cost) + " gallons of fuel for $" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarFuel += Offer.Cost;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por adquirir " + String.Format("{0:N0}", Offer.Cost) + " galões de gasolina " + Session.GetHabbo().Username + "!", true);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Session.SendWhisper("Desculpe, você não pode ter recursos para a gasolina!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra " + String.Format("{0:N0}", Offer.Cost) + " Galões de gasolina por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarFuel += Offer.Cost;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            return;
                        }
                    }
                }
                #endregion

                #region Job
                if (Type.ToLower() == "emprego")
                {
                    int OriginalJob = Session.GetRoleplay().JobId;
                    var OldJob = GroupManager.GetJob(OriginalJob);

                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("emprego", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar este trabalho!", 1);
                            return;
                        }
                        else
                        {
                            var Job = GroupManager.GetJob(Offer.Cost);
                            var JobRank = GroupManager.GetJobRank(Offer.Cost, 1);

                            if (Job != null)
                            {
                                if (Job.Members.Count < JobRank.Limit || JobRank.Limit <= 0)
                                {
                                    Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " aceita e junta-se '" + Job.Name + "' corporação como '" + JobRank.Name + "'*", 4);
                                    Bot.GetRoomUser().Chat("Bem vindo à Corporação " + Job.Name + "" + Session.GetHabbo().Username + "!", true);

                                    Session.GetRoleplay().TimeWorked = 0;
                                    Session.GetRoleplay().JobId = Job.Id;
                                    Session.GetRoleplay().JobRank = 1;
                                    Session.GetRoleplay().JobRequest = 0;

                                    Job.AddNewMember(Session.GetHabbo().Id);
                                    Session.SendMessage(new FriendListUpdateComposer(-Job.Id, Job.Id));
                                    Session.SendMessage(new FriendListUpdateComposer(-OldJob.Id));
                                    Job.SendPackets(Session);
                                }
                                else
                                    Session.SendWhisper("Desculpe, mas esta empresa de emprego está atualmente cheia!", 1);
                            }
                            else
                                Session.SendWhisper("Por alguma razão estranha, este trabalho não pôde ser encontrado!", 1);

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("emprego", out Junk);
                            return;
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser() == null || Session.GetRoomUser() == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("emprego", out Junk);
                            Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                            return;
                        }
                        else
                        {
                            var Job = GroupManager.GetJob(Offer.Cost);
                            var JobRank = GroupManager.GetJobRank(Offer.Cost, 1);

                            if (Job != null)
                            {
                                Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e se junta a Corporação '" + Job.Name + " como " + JobRank.Name + "'*", 4);
                                Offerer.SendWhisper(Session.GetHabbo().Username + " Acaba de aderir à sua empresa!", 1);

                                Session.GetRoleplay().TimeWorked = 0;
                                Session.GetRoleplay().JobId = Job.Id;
                                Session.GetRoleplay().JobRank = 1;
                                Session.GetRoleplay().JobRequest = 0;

                                Job.AddNewMember(Session.GetHabbo().Id);
                                Session.SendMessage(new FriendListUpdateComposer(-Job.Id, Job.Id));
                                Session.SendMessage(new FriendListUpdateComposer(-OldJob.Id));
                                Job.SendPackets(Session);
                            }
                            else
                                Session.SendWhisper("Por alguma razão estranha, este trabalho não pôde ser encontrado!", 1);

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("emprego", out Junk);
                            return;
                        }
                    }
                }
                #endregion

                #region Gang
                if (Type.ToLower() == "gangue")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                    if (Offerer == null || Offerer.GetRoomUser() == null || Session.GetRoomUser() == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId)
                    {
                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gangue", out Junk);
                        Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                        return;
                    }
                    else
                    {
                        if (GroupManager.Jobs.Values.Where(x => x.CreatorId == Session.GetHabbo().Id).ToList().Count > 0)
                        {
                            Session.SendWhisper("Por favor, exclua sua gangue antes de tentar se juntar a outra!", 1);
                            return;
                        }

                        var Gang = GroupManager.GetGang(Offer.Cost);
                        var GangRank = GroupManager.GetGangRank(Offer.Cost, 1);

                        if (Gang != null)
                        {
                            Session.GetRoleplay().GangId = Gang.Id;
                            Session.GetRoleplay().GangRank = 1;
                            Session.GetRoleplay().GangRequest = 0;

                            Gang.AddNewMember(Session.GetHabbo().Id);
                            Gang.SendPackets(Session);
                        }
                        else
                            Session.SendWhisper("Por alguma razão estranha, este grupo não poderia ser encontrado, poderia ter sido excluído depois de convidá-lo para ele!", 1);

                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gangue", out Junk);
                        Session.Shout("*Aceita a oferta de gangues de " + Offerer.GetHabbo().Username + " e se junta a gangue " + Gang.Name + "*", 4);
                        return;
                    }
                }
                #endregion

                #region Marriage
                if (Type.ToLower() == "casamento")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                    if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                    {
                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("casamento", out Junk);
                        Session.SendWhisper("Desculpe, este usuário desconectou ou não está no mesmo quarto que você!", 1);
                        return;
                    }
                    else if (Session.GetRoleplay().MarriedTo > 0)
                    {
                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("casamento", out Junk);
                        Session.SendWhisper("Desculpe você já é casado!", 1);
                        return;
                    }
                    else if (Offerer.GetRoleplay().MarriedTo > 0)
                    {
                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("casamento", out Junk);
                        Session.SendWhisper("Desculpe, este usuário já é casado!", 1);
                        return;
                    }
                    else
                    {
                        if (Offerer.GetRoleplay().OfferManager.ActiveOffers.ContainsKey("casamento"))
                        {
                            RoleplayOffer OffererJunk;
                            Offerer.GetRoleplay().OfferManager.ActiveOffers.TryRemove("casamento", out OffererJunk);
                        }

                        Session.GetRoleplay().MarriedTo = Offerer.GetHabbo().Id;
                        Offerer.GetRoleplay().MarriedTo = Session.GetHabbo().Id;

                        if (!Session.GetHabbo().GetBadgeComponent().HasBadge("WD0"))
                            Session.GetHabbo().GetBadgeComponent().GiveBadge("WD0", true, Session);

                        if (!Offerer.GetHabbo().GetBadgeComponent().HasBadge("WD0"))
                            Offerer.GetHabbo().GetBadgeComponent().GiveBadge("WD0", true, Offerer);

                        if (PlusEnvironment.GetGame().GetCacheManager().ContainsUser(Session.GetHabbo().Id))
                            PlusEnvironment.GetGame().GetCacheManager().TryUpdateUser(Session);

                        if (PlusEnvironment.GetGame().GetCacheManager().ContainsUser(Offerer.GetHabbo().Id))
                            PlusEnvironment.GetGame().GetCacheManager().TryUpdateUser(Offerer);

                        using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `rp_stats` SET `married_to` = '" + Session.GetHabbo().Id + "' WHERE `id` = '" + Offerer.GetHabbo().Id + "'");
                            dbClient.RunQuery("UPDATE `rp_stats` SET `married_to` = '" + Offerer.GetHabbo().Id + "' WHERE `id` = '" + Session.GetHabbo().Id + "'");
                        }

                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("casamento", out Junk);
                        Session.Shout("*Ajoelha " + Offerer.GetHabbo().Username + " e aceita o casamento*", 16);
                    }
                }
                #endregion

                #region Savings Account
                if (Type.ToLower() == "poupanca")
                {
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a conta poupança!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para abrir uma conta de poupança!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e abre uma conta poupança*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankAccount = 2;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por abrir uma conta de poupança " + Session.GetHabbo().Username + "!", true);
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Session.SendWhisper("Lamentamos que este utilizador tenha sessão iniciada ou não esteja na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Session.SendWhisper("Desculpe, você não tem recursos para abrir uma conta de poupança!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e abre uma conta poupança*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankAccount = 2;

                            Offerer.GetHabbo().Credits += Offer.Cost / 25;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Offerer.SendWhisper("Você recebeu R$" + String.Format("{0:N0}", (Offer.Cost / 25)) + " à venda " + Session.GetHabbo().Username + " um(a) " + Offer.Type + "!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingBankAccount", 1);
                            return;
                        }
                    }
                }
                #endregion
            }
            else
            {
                Session.SendWhisper("Você não tem uma " + Type + " oferta!", 1);
                return;
            }
        }
    }
}