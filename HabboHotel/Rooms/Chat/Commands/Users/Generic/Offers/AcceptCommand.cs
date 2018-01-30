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
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Aceita a oferta com base no tipo desejado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Insira um tipo de oferta! Verifique :ofertas para ver todas ofertas atuais!", 1);
                return;
            }

            if (Session.GetRoleplay().OfferManager.ActiveOffers.Count <= 0)
            {
                Session.SendWhisper("Você não possui nenhuma oferta ativa para aceitar!", 1);
                return;
            }

            string Type = Params[1];
            if (CommandManager.MergeParams(Params, 1).ToLower() == "Saco de Sementes")
                Type = "sacodesementes";
            else if (CommandManager.MergeParams(Params, 1).ToLower() == "Saco de Plantas")
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
                                Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a oferta de armas!", 1);
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
                                Session.SendWhisper("Desculpe, você não pode pagar uma " + weapon.PublicName + "!", 1);
                                return;
                            }
                            else
                            {
                                if (!Session.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name))
                                {
                                    Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra um(a) " + weapon.PublicName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                                    Session.GetHabbo().Credits -= Offer.Cost;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    WeaponManager.Weapons[weapon.Name].Stock--;
                                    RoleplayManager.AddWeapon(Session, weapon);
                                }
                                else
                                {
                                    Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra um(a) " + weapon.PublicName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
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
                                Bot.GetRoomUser().Chat("Obrigado por comprar um(a) " + weapon.PublicName + ", Bom uso " + Session.GetHabbo().Username + "!", true);
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
                                Session.SendWhisper("Desculpe, este usuário desconectou-se ou não está na mesma sala que você!", 1);
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
                                Session.SendWhisper("Desculpe, você não pode pagar um(a) " + weapon.PublicName + "!", 1);
                                return;
                            }
                            else
                            {
                                if (!Session.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name))
                                {
                                    Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra um(a) " + weapon.PublicName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                                    Session.GetHabbo().Credits -= Offer.Cost;
                                    Session.GetHabbo().UpdateCreditsBalance();

                                    WeaponManager.Weapons[weapon.Name].Stock--;
                                    RoleplayManager.AddWeapon(Session, weapon);
                                }
                                else
                                {
                                    Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra um(a) " + weapon.PublicName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
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
                                Offerer.SendWhisper("Você recebe uma cortesia de R$" + String.Format("{0:N0}", Bonus) + " por vender " + Session.GetHabbo().Username + " um(a) " + weapon.PublicName + "!");
                                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingWeapon", 1);
                                return;
                            }
                        }
                    }
                    else
                    {
                        Session.SendWhisper("Você não possui uma oferta de armas!", 1);
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a oferta de celular!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um Nokia Tijolão!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType > 0)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("atualizar", out Junk);
                            Session.SendWhisper("Você já possui um celular!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra um Nokia Tijolão por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType = 1;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por comprar um Nokia Tijolão " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("celular", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um Nokia Tijolão!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType > 0)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("atualizar", out Junk);
                            Session.SendWhisper("Você já possui um celular", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra um Nokia Tijolão por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType = 1;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("phone", out Junk);
                            Offerer.SendWhisper("Você recebe uma cortesia de R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " por vender para " + Session.GetHabbo().Username + " um(a) " + Offer.Type + "!");
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a atualização do celular!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um " + PhoneName + "!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType >= 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Você já possui o melhor celular possível!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra a atualização do celular para o " + PhoneName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType += 1;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por comprar uma atualização de celular " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um " + PhoneName + "!", 1);
                            return;
                        }
                        else if (Session.GetRoleplay().PhoneType >= 3)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Session.SendWhisper("Você já possui o melhor celular possível!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra a atualização do celular para o " + PhoneName + " por $" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().PhoneType += 1;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcel", out Junk);
                            Offerer.SendWhisper("Você recebeu uma cortesia de R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " por vender para " + Session.GetHabbo().Username + " uma atualização de celular!");
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar os créditos do celular!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 1)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar créditos de celular!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra " + String.Format("{0:N0}", Offer.Cost) + " créditos de celular por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)Offer.Cost / 1))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)Offer.Cost / 1));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetHabbo().Duckets += Offer.Cost;
                            Session.GetHabbo().UpdateDucketsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por comprar " + String.Format("{0:N0}", Offer.Cost) + " créditos de celular, " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 1)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("creditos", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar créditos de celular!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra " + String.Format("{0:N0}", Offer.Cost) + " créditos de celular por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)Offer.Cost / 1))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)Offer.Cost / 1));
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar as balas!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 1)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar essas balas!", 1);
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
                            Bot.GetRoomUser().Chat("Obrigado por comprar " + String.Format("{0:N0}", Offer.Cost) + " balas, " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)Offer.Cost / 1)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("balas", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar essas balas!", 1);
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para comprar as sementes!", 1);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sementes", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar " + Amount + " sementes " + Furni.PublicName + "!", 1);
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
                            Session.SendWhisper("Você já tem um Saco de Sementes!", 1);
                            return;
                        }
                        else if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a oferta do Saco de Sementes!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um Saco de Sementes!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra um Saco de Sementes por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasSeedSatchel = true;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por comprar um Saco de Sementes, " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Você já tem um Saco de Sementes!", 1);
                            return;
                        }
                        else if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar o Saco de Sementes!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra um Saco de Sementes por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasSeedSatchel = true;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodesementes", out Junk);
                            Offerer.SendWhisper("Você recebeu uma cortesia de R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " por vender um Saco de Sementes para " + Session.GetHabbo().Username + "!");
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
                            Session.SendWhisper("Você já tem um Saco de Plantas!", 1);
                            return;
                        }
                        else if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a oferta de Saco de Plantas!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um Saco de Plantas!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra um Saco de Plantas por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasPlantSatchel = true;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por comprar um Saco de Plantas, " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Você já tem um Saco de Plantas!", 1);
                            return;
                        }
                        else if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("sacodeplantas", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um Saco de Plantas!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra um Saco de Plantas por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().FarmingStats.HasPlantSatchel = true;

                            Offerer.GetHabbo().Credits += Offer.Cost / 10;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("plantsatchel", out Junk);
                            Offerer.SendWhisper("Você recebeu uma cortesia de R$" + String.Format("{0:N0}", (Offer.Cost / 10)) + " por vender um Saco de Plantas para " + Session.GetHabbo().Username + "!");
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a oferta de Carro!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um Toyota Corolla!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra um Toyota Corolla por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarType = 1;
                            Session.GetRoleplay().CarFuel = 300;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por comprar um Toyota Corolla " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("carro", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar um Toyota Corolla!", 1);
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
                            Offerer.SendWhisper("Você recebeu uma cortesia de R$" + String.Format("{0:N0}", (Offer.Cost / 20)) + " por vender para " + Session.GetHabbo().Username + " um " + Offer.Type + "!");
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
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra uma atualização de carro para o " + CarName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarType += 1;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por atualizar seu carro, " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
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
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra uma atualização de carro para o " + CarName + " por R$" + String.Format("{0:N0}", Offer.Cost) + "*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarType += 1;

                            Offerer.GetHabbo().Credits += Offer.Cost / 20;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("uparcarro", out Junk);
                            Offerer.SendWhisper("Você recebeu uma cortesia de R$" + String.Format("{0:N0}", (Offer.Cost / 20)) + " por vender para " + Session.GetHabbo().Username + " uma atualização de carro!");
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a gasolina!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar por esse combustível", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e compra " + String.Format("{0:N0}", Offer.Cost) + " galões de gasolina por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3))) + "*", 4);
                            Session.GetHabbo().Credits -= Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3));
                            Session.GetHabbo().UpdateCreditsBalance();
                            Session.GetRoleplay().CarFuel += Offer.Cost;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por comprar " + String.Format("{0:N0}", Offer.Cost) + " galões de gasolina " + Session.GetHabbo().Username + "!", true);
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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3)))
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gasolina", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar por esse combustível!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e compra " + String.Format("{0:N0}", Offer.Cost) + " galões de gasolina por R$" + String.Format("{0:N0}", Convert.ToInt32(Math.Floor((double)(Offer.Cost * 2) / 3))) + "*", 4);
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
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];
                    if (Offer.Params != null && Offer.Params.Length > 0)
                    {
                        RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                        if (Bot.GetRoomUser().RoomId != Room.Id)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("emprego", out Junk);
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar este emprego!", 1);
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
                                    Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e entra na empresa '" + Job.Name + "' no cargo '" + JobRank.Name + "'*", 4);
                                    Bot.GetRoomUser().Chat("Bem vindo a empresa " + Job.Name + ", " + Session.GetHabbo().Username + "!", true);

                                    Session.GetRoleplay().TimeWorked = 0;
                                    Session.GetRoleplay().JobId = Job.Id;
                                    Session.GetRoleplay().JobRank = 1;
                                    Session.GetRoleplay().JobRequest = 0;

                                    Job.AddNewMember(Session.GetHabbo().Id);
                                    Job.SendPackets(Session);
                                }
                                else
                                    Session.SendWhisper("Desculpe, mas esta empresa está atualmente cheia!", 1);
                            }
                            else
                                Session.SendWhisper("Por algum motivo, este trabalho não pôde ser encontrado!", 1);

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
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else
                        {
                            var Job = GroupManager.GetJob(Offer.Cost);
                            var JobRank = GroupManager.GetJobRank(Offer.Cost, 1);

                            if (Job != null)
                            {
                                Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e entra na empresa '" + Job.Name + "' no cargo '" + JobRank.Name + "'*", 4);
                                Offerer.SendWhisper(Session.GetHabbo().Username + " acabou de se juntar a sua empresa!", 1);

                                Session.GetRoleplay().TimeWorked = 0;
                                Session.GetRoleplay().JobId = Job.Id;
                                Session.GetRoleplay().JobRank = 1;
                                Session.GetRoleplay().JobRequest = 0;

                                Job.AddNewMember(Session.GetHabbo().Id);
                                Job.SendPackets(Session);
                            }
                            else
                                Session.SendWhisper("Por algum motivo, este trabalho não pôde ser encontrado!", 1);

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
                        Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
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
                            Session.SendWhisper("Por algum motivo estranho, essa gangue não pôde ser encontrada, pode ter sido excluída!", 1);

                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("gangue", out Junk);
                        Session.Shout("*Aceita o convite de " + Offerer.GetHabbo().Username + " e se junta a gangue " + Gang.Name + "*", 4);
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
                        Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                        return;
                    }
                    else if (Session.GetRoleplay().MarriedTo > 0)
                    {
                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("casamento", out Junk);
                        Session.SendWhisper("Desculpe, você já é casado(a)", 1);
                        return;
                    }
                    else if (Offerer.GetRoleplay().MarriedTo > 0)
                    {
                        RoleplayOffer Junk;
                        Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("casamento", out Junk);
                        Session.SendWhisper("Desculpe, este cidadão já é casado(a)!", 1);
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
                        Session.Shout("*Aceita se casar com " + Offerer.GetHabbo().Username + "*", 16);
                    }
                }
                #endregion

                #region Chequings Account
                if (Type.ToLower() == "cheques" || Type.ToLower() == "cheques")
                {
                    Type = "cheques";
                    var Offer = Session.GetRoleplay().OfferManager.ActiveOffers[Type.ToLower()];

                    if (Offer != null)
                    {
                        if (Offer.Params != null && Offer.Params.Length > 0)
                        {
                            RoleplayBotAI Bot = (RoleplayBotAI)Offer.Params[0];
                            if (Bot.GetRoomUser().RoomId != Room.Id)
                            {
                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("cheques", out Junk);
                                Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a conta de Cheques!", 1);
                                return;
                            }
                            else
                            {
                                Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e abre uma conta de Cheques*", 4);
                                Session.GetRoleplay().BankAccount = 1;

                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("cheques", out Junk);
                                Bot.GetRoomUser().Chat("Obrigado por abrir uma conta de Cheques " + Session.GetHabbo().Username + "!", true);
                                return;
                            }
                        }
                        else
                        {
                            GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                            if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                            {
                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("cheques", out Junk);
                                Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                                return;
                            }
                            else
                            {
                                Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e abre uma conta de Cheques*", 4);
                                Session.GetRoleplay().BankAccount = 1;

                                Offerer.GetHabbo().Credits += 10;
                                Offerer.GetHabbo().UpdateCreditsBalance();

                                RoleplayOffer Junk;
                                Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("cheques", out Junk);
                                Offerer.SendWhisper("Você recebeu uma cortesia de R$10 por abrir uma conta de Cheques para " + Session.GetHabbo().Username + "!");
                                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingBankAccount", 1);
                                return;
                            }
                        }
                    }
                    else
                    {
                        Session.SendWhisper("Você não possui uma oferta de conta de Cheques!", 1);
                        return;
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
                            Session.SendWhisper("Desculpe, você não está no mesmo quarto que " + Bot.GetBotRoleplay().Name + " para aceitar a conta Poupança!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar uma conta Poupança!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Bot.GetBotRoleplay().Name + " e abre uma conta Poupança*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankAccount = 2;

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Bot.GetRoomUser().Chat("Obrigado por abrir uma conta Poupança, " + Session.GetHabbo().Username + "!", true);
                        }
                    }
                    else
                    {
                        GameClient Offerer = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Offer.OffererId);
                        if (Offerer == null || Offerer.GetRoomUser().RoomId != Session.GetRoomUser().RoomId || Offerer.GetRoomUser() == null)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Session.SendWhisper("Desculpe, este usuário está offline ou não está na mesma sala que você!", 1);
                            return;
                        }
                        else if (Session.GetHabbo().Credits < Offer.Cost)
                        {
                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("poupanca", out Junk);
                            Session.SendWhisper("Desculpe, você não pode pagar uma conta Poupança!", 1);
                            return;
                        }
                        else
                        {
                            Session.Shout("*Aceita a oferta de " + Offerer.GetHabbo().Username + " e abre uma conta Poupança*", 4);
                            Session.GetHabbo().Credits -= Offer.Cost;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankAccount = 2;

                            Offerer.GetHabbo().Credits += Offer.Cost / 25;
                            Offerer.GetHabbo().UpdateCreditsBalance();

                            RoleplayOffer Junk;
                            Session.GetRoleplay().OfferManager.ActiveOffers.TryRemove("savings", out Junk);
                            Offerer.SendWhisper("Você recebeu uma cortesia de R$" + String.Format("{0:N0}", (Offer.Cost / 25)) + " por vender para " + Session.GetHabbo().Username + " uma " + Offer.Type + "!");
                            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Offerer, "ACH_SellingBankAccount", 1);
                            return;
                        }
                    }
                }
                #endregion
            }
            else
            {
                Session.SendWhisper("Você não tem uma oferta de " + Type + "!", 1);
                return;
            }
        }
    }
}