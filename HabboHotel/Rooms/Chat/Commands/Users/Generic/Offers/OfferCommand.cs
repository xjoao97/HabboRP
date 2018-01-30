using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboRoleplay.Farming;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class OfferCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_offers_offer"; }
        }

        public string Parameters
        {
            get { return "%usuário% %oferta%"; }
        }

        public string Description
        {
            get { return "Oferece o tipo desejado ao usuário alvo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Comando inválido, Use ':oferecer usuário item [e talvez preço]'.", 1);
                return;
            }

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("Opa, usuário não encontrado!", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            string Type = Params[2];

            if (CommandManager.MergeParams(Params, 2) == "Saco de Sementes")
                Type = "sacodesementes";
            else if (CommandManager.MergeParams(Params, 2) == "Saco de Plantas")
                Type = "sacodeplantas";

            #region Weapon Check
            Weapon weapon = null;
            foreach (Weapon Weapon in WeaponManager.Weapons.Values)
            {
                if (Type.ToLower() == Weapon.Name.ToLower())
                {
                    Type = "armas";
                    weapon = Weapon;
                }
            }
            #endregion

            #region Car/Phone Upgrade Check
            if (Type.ToLower() == "upar")
            {
                lock (GroupManager.Jobs)
                {
                    Group CarJob = GroupManager.Jobs.Values.FirstOrDefault(x => x.Ranks.Count > 0 && x.Ranks.Values.FirstOrDefault().HasCommand("carro"));
                    Group PhoneJob = GroupManager.Jobs.Values.FirstOrDefault(x => x.Ranks.Count > 0 && x.Ranks.Values.FirstOrDefault().HasCommand("celular"));

                    if (CarJob != null && CarJob.Ranks.Values.FirstOrDefault().CanWorkHere(Room.Id))
                        Type = "uparcarro";

                    if (PhoneJob != null && PhoneJob.Ranks.Values.FirstOrDefault().CanWorkHere(Room.Id))
                        Type = "uparcel";
                }
            }
            #endregion

            switch (Type.ToLower())
            {
                #region Weapon
                case "weapon":
				case "armas":
				case "arma":
				case "gun":
                    {
                        if (weapon == null)
                        {
                            Session.SendWhisper("'" + Type + "' não é um tipo de oferta válida!");
                            break;
                        }

                        if (weapon.Stock < 1)
                        {
                            Session.SendWhisper("Não há " + weapon.PublicName + " no estoque! Por favor, use o comando ':pedirentrega' para lotar o estoque!", 1);
                            return;
                        }

                        if (!GroupManager.HasJobCommand(Session, "weapon") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything") && Session.GetHabbo().VIPRank < 2)
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa de Armas!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name) && Target.GetRoleplay().OwnedWeapons[weapon.Name].CanUse)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão já possui um(a) " + weapon.PublicName + "!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything") && Session.GetHabbo().VIPRank < 2)
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém um(a) " + weapon.PublicName + "!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = (!Target.GetRoleplay().OwnedWeapons.ContainsKey(weapon.Name) ? weapon.Cost : weapon.CostFine);
                            bool HasOffer = false;
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (WeaponManager.Weapons.ContainsKey(Offer.Type.ToLower()))
                                        HasOffer = true;
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece um(a) " + weapon.PublicName + " para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer(weapon.Name.ToLower(), Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você recebeu uma oferta de um(a) " + weapon.PublicName + " por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar arma' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma arma!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar um(a) " + weapon.PublicName + "!", 1);
                                break;
                            }
                        }
                    }
                #endregion

                #region Phone
                case "phone":
				case "celular":
				case "cel":
				case "telefone":
                    {
                        if (!GroupManager.HasJobCommand(Session, "phone") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa de celular!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().PhoneType > 0)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão já tem um celular!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém um telefone!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = 50;
                            bool HasOffer = false;
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == Type.ToLower())
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece um Nokia Tijolao para " + Target.GetHabbo().Username + " por $50*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("celular", Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você recebeu uma oferta de um Nokia Tijolão! Digite ':aceitar celular' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma oferta de Celular!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar um telefone!", 1);
                                break;
                            }
                        }
                    }
                #endregion

                #region Phone Upgrade
                case "phoneupgrade":
				case "uparcel":
				case "upartelefone":
				case "uparcelular":
				case "attcel":
				case "atualizarcel":
				case "attcelular":
                    {
                        if (!GroupManager.HasJobCommand(Session, "phone") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa do telefone!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().PhoneType < 1)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão não possui um telefone para atualizar!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().PhoneType > 2)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão já tem o telefone mais alto que pode obter!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém um telefone!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = Target.GetRoleplay().PhoneType == 1 ? 400 : 1000;
                            bool HasOffer = false;
                            string PhoneName = RoleplayManager.GetPhoneName(Target, true);

                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == "uparcel")
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece uma atualização para o " + PhoneName + " de " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("uparcel", Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você recebeu uma oferta para atualizar o seu " + PhoneName + " por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar uparcel' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma atualização para o telefone!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar uma atualização por telefone!", 1);
                                break;
                            }
                        }
                    }
                #endregion

                #region Phone Credit
                case "credit":
                case "credits":
                case "phonecredit":
                case "phonecredits":
				case "creditos":
				case "celcreditos":
                    {
                        if (!GroupManager.HasJobCommand(Session, "phone") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa de telefone!", 1);
                            break;
                        }

                        if (Params.Length == 3)
                        {
                            Session.SendWhisper("Digite o valor do crédito do telefone que você gostaria de oferecer ao cidadão!", 1);
                            return;
                        }

                        int Amount;
                        if (!int.TryParse(Params[3], out Amount))
                        {
                            Session.SendWhisper("Digite uma quantidade válida de crédito do telefone que você gostaria de oferecer ao cidadão!", 1);
                            break;
                        }

                        if (Amount < 10)
                        {
                            Session.SendWhisper("Você precisa oferecer ao cidadão pelo menos 10 créditos de celular por vez!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().PhoneType < 1)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão não tem telefone!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer créditos para alguém!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = Convert.ToInt32(Math.Floor((double)Amount / 2));
                            bool HasOffer = false;

                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == "creditos")
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece "  + String.Format("{0:N0}", Amount) + " créditos de celular para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("creditos", Session.GetHabbo().Id, Amount);
                                    Target.SendWhisper("Você recebeu uma oferta de " + String.Format("{0:N0}", Amount) + " créditos de celular por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar creditos' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu créditos de telefone!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar créditos de telefone!", 1);
                                break;
                            }
                        }
                    }

                #endregion

                #region Bullets

                case "bullets":
                case "ammo":
				case "balas":
                    {
                        if (!GroupManager.HasJobCommand(Session, "weapon") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa de Armas!", 1);
                            break;
                        }

                        if (Params.Length == 3)
                        {
                            Session.SendWhisper("Digite a quantidade de balas que você gostaria de oferecer ao cidadão!", 1);
                            return;
                        }

                        int Amount;
                        if (!int.TryParse(Params[3], out Amount))
                        {
                            Session.SendWhisper("Digite uma quantidade válida de balas que você gostaria de oferecer ao cidadão!", 1);
                            break;
                        }

                        if (Amount < 10)
                        {
                            Session.SendWhisper("Você precisa oferecer ao cidadão pelo menos 10 balas por vez!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().PhoneType < 1)
                        {
                            Session.SendWhisper("Este Cidadão precisa de um Celular para ter uma Arma, em caso de Multas!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer balas de alguém!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = Convert.ToInt32(Math.Floor((double)Amount / 1));
                            bool HasOffer = false;

                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == "balas")
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece " + String.Format("{0:N0}", Amount) + " balas para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("balas", Session.GetHabbo().Id, Amount);
                                    Target.SendWhisper("Você recebeu uma oferta de " + String.Format("{0:N0}", Amount) + " balas por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar balas' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu balas!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar balas!", 1);
                                break;
                            }
                        }
                    }

                #endregion

                #region Seeds
                case "seed":
                case "seeds":
				case "sementes":
                    {
                        if (!GroupManager.HasJobCommand(Session, "supermarket") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa do supermercado!", 1);
                            break;
                        }

                        if (Params.Length < 5)
                        {
                            Session.SendWhisper("Digite o comando: ':oferecer <usuário> semente <id> <quantidade>'!", 1);
                            return;
                        }

                        int Id;
                        if (!int.TryParse(Params[3], out Id))
                        {
                            Session.SendWhisper("Digite um ID de semente válido para oferecer ao cidadão!", 1);
                            return;
                        }

                        FarmingItem Item = FarmingManager.GetFarmingItem(Id);

                        if (Item == null)
                        {
                            Session.SendWhisper("Desculpe, mas este ID de semente não pôde ser encontrado!", 1);
                            return;
                        }

                        ItemData Furni;
                        if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni))
                        {
                            Session.SendWhisper("Desculpe, mas esta semente não pôde ser encontrada!", 1);
                            return;
                        }

                        int Amount;
                        if (!int.TryParse(Params[4], out Amount))
                        {
                            Session.SendWhisper("Insira uma quantidade válida de sementes que você gostaria de oferecer ao cidadão!", 1);
                            break;
                        }

                        if (!Target.GetRoleplay().FarmingStats.HasSeedSatchel)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão não tem um Saco de Sementes!", 1);
                            break;
                        }

                        int Cost = (Amount * Item.BuyPrice);

                        if (Item.LevelRequired > Target.GetRoleplay().FarmingStats.Level)
                        {
                            Session.SendWhisper("Desculpe, mas esse cidadão não tem um nível de cultivo suficientemente alto para esta semente!", 1);
                            return;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer sementes", 1);
                            break;
                        }
                        else
                        {
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                if (Target.GetRoleplay().OfferManager.ActiveOffers.Values.Where(x => x.Type.ToLower() == "seeds").ToList().Count <= 0)
                                {
                                    Session.Shout("*Oferece " + String.Format("{0:N0}", Amount) + " sementes " + Furni.PublicName + " para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("sementes", Session.GetHabbo().Id, Amount, Item);
                                    Target.SendWhisper("Você recebeu uma oferta de Sementes " + String.Format("{0:N0}", Amount) + " " + Furni.PublicName + " por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar sementes' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma oferta de sementes!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar a oferta de sementes!", 1);
                                break;
                            }
                        }
                    }

                #endregion

                #region Seed Satchel
                case "seedsatchel":
				case "sacosementes":
				case "sacodesementes":
				case "sds":
                    {
                        if (!GroupManager.HasJobCommand(Session, "supermarket") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa de supermercados!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().FarmingStats.HasSeedSatchel)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão já possui um Saco de Sementes!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém um Saco de Sementes!", 1);
                            break;
                        }
                        else
                        {
                            int Cost = Convert.ToInt32(RoleplayData.GetData("farming", "seedsatchelcost"));

                            bool HasOffer = false;
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == Type.ToLower())
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece um Saco de Sementes para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("sacodesementes", Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você recebeu uma oferta de Saco de Sementes por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar sacodesementes' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já foi oferecido um Saco de Sementes!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar um Saco de Sementes!", 1);
                                break;
                            }
                        }
                    }
                #endregion

                #region Plant Satchel
                case "plantsatchel":
				case "sacodeplanta":
				case "sacoplanta":
				case "sp":
				case "sdp":
                    {
                        if (!GroupManager.HasJobCommand(Session, "supermarket") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa do supermercado!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().FarmingStats.HasPlantSatchel)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão já possui um Saco de Plantas!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém um Saco de Plantas!", 1);
                            break;
                        }
                        else
                        {
                            int Cost = Convert.ToInt32(RoleplayData.GetData("farming", "plantsatchelcost"));

                            bool HasOffer = false;
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == Type.ToLower())
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece um Saco de Plantas para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("sacodeplantas", Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você recebeu uma oferta de Saco de Plantas por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar sacodeplantas' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma oferta de Saco de Plantas!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar um Saco de Plantas!", 1);
                                break;
                            }
                        }
                    }
                #endregion

                #region Car
                case "car":
				case "carro":
                    {
                        if (!GroupManager.HasJobCommand(Session, "car") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa de automóveis!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().CarType > 0)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão já tem um carro!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém um carro!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = 800;
                            bool HasOffer = false;
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == Type.ToLower())
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece um Toyota Corolla para " + Target.GetHabbo().Username + " por R$800,00*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("carro", Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você recebeu uma oferta de um Toyota Corolla por R$800,00! Digite ':aceitar carro' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma oferta de carro!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar um carro!", 1);
                                break;
                            }
                        }
                    }
                #endregion

                #region Car Upgrade
                case "carupgrade":
				case "uparcarro":
				case "attcarro":
				case "atualizarcarro":
                    {
                        if (!GroupManager.HasJobCommand(Session, "car") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa de automóveis!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().CarType < 1)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão não possui carro para atualizar!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().CarType > 2)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão já tem o carro mais alto que pode comprar!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer uma atualização de carro!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = Target.GetRoleplay().CarType == 1 ? 800 : 1500;
                            bool HasOffer = false;
                            string CarName = RoleplayManager.GetCarName(Target, true);

                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == "uparcarro")
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece uma atualização do " + CarName + " para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("uparcarro", Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você recebeu uma oferta para atualizar o seu " + CarName + " por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar uparcarro' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma oferta de atualização de carro!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar uma atualização de carro!", 1);
                                break;
                            }
                        }
                    }
                #endregion

                #region Fuel
                case "fuel":
				case "gasolina":
				case "combustivel":
				case "diesel":
                    {
                        if (!GroupManager.HasJobCommand(Session, "car") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Desculpe, você não trabalha na empresa Carros!", 1);
                            break;
                        }

                        if (Params.Length == 3)
                        {
                            Session.SendWhisper("Insira a quantidade de combustível para oferecer ao cidadão!", 1);
                            return;
                        }

                        int Amount;
                        if (!int.TryParse(Params[3], out Amount))
                        {
                            Session.SendWhisper("Insira uma quantidade válida de combustível para oferecer ao cidadão!", 1);
                            break;
                        }

                        if (Amount < 10)
                        {
                            Session.SendWhisper("Você precisa oferecer ao cidadão pelo menos 10 galões de combustível por vez!", 1);
                            break;
                        }

                        if (Target.GetRoleplay().CarType < 1)
                        {
                            Session.SendWhisper("Desculpe, esse cidadão não tem um carro!", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para oferecer a alguém combustível!", 1);
                            break;
                        }

                        else
                        {
                            int Cost = Convert.ToInt32(Math.Floor((double)(Amount * 2) / 3));
                            bool HasOffer = false;

                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == "gasolina")
                                    {
                                        HasOffer = true;
                                    }
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Oferece " + String.Format("{0:N0}", Amount) + " galões de gasolina para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("gasolina", Session.GetHabbo().Id, Amount);
                                    Target.SendWhisper("Você recebeu uma oferta de " + String.Format("{0:N0}", Amount) + " galões de gasolina por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar gasolina' para comprar!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu combustível!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não pode pagar combustível!", 1);
                                break;
                            }
                        }
                    }

                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("'" + Type + "' não é um tipo de oferta válida!", 1);
                        break;
                    }
                #endregion
            }
        }
    }
}