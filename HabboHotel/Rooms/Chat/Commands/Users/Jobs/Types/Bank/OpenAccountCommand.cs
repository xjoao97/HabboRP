using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Bank
{
    class OpenAccountCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_jobs_open_account"; }
        }

        public string Parameters
        {
            get { return "%user% %type%"; }
        }

        public string Description
        {
            get { return "Ofertas para abrir o tipo de conta bancária para o usuário alvo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite um alvo e tipo de banco!");
                return;
            }

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("Opa, não encontrou esse usuário!");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Target.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online ou nesta sala.", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "openaccount") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
            {
                Session.SendWhisper("Desculpe, você não trabalha na corporação do banco!", 1);
                return;
            }

            #endregion

            #region Execute

            if (Params.Length < 3)
            {
                if (Target.GetRoleplay().BankAccount > 0)
                {
                    Session.SendWhisper("Desculpe, mas essa pessoa já possui uma Conta de Cheques!", 1);
                    return;
                }

                string offer = "cheques";
                bool HasOffer = false;
                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                {
                    if (Offer.Type.ToLower() == offer.ToLower())
                        HasOffer = true;
                }
                if (!HasOffer)
                {
                    Session.Shout("*Oferece uma conta de Cheques para " + Target.GetHabbo().Username + " de graça*", 4);
                    Target.GetRoleplay().OfferManager.CreateOffer("cheques", Session.GetHabbo().Id, 0);
                    Target.SendWhisper("Você acabou de receber gratuitamente uma conta de cheques! Digite ':aceitar cheques' para ativá-la!", 1);
                    return;
                }
                else
                {
                    Session.SendWhisper("Este usuário já recebeu uma conta de Cheques!", 1);
                    return;
                }
            }
            else
            {
                switch (Params[2].ToLower())
                {
                    case "chequings":
                    case "checkings":
					case "cheques":
                        {
                            if (Target.GetRoleplay().BankAccount > 0)
                            {
                                Session.SendWhisper("Desculpe, mas essa pessoa já possui uma Conta de Cheques!", 1);
                                break;
                            }

                            Params[2] = "cheques";
                            bool HasOffer = false;
                            foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                            {
                                if (Offer.Type.ToLower() == Params[2].ToLower())
                                    HasOffer = true;
                            }
                            if (!HasOffer)
                            {
                                Session.Shout("*Oferece uma conta Cheques para " + Target.GetHabbo().Username + " de graça*", 4);
                                Target.GetRoleplay().OfferManager.CreateOffer("cheques", Session.GetHabbo().Id, 0);
                                Target.SendWhisper("Você acabou de receber gratuitamente uma conta de Chques! Digite ':aceitar cheques' para ativá-la!", 1);
                                break;
                            }
                            else
                            {
                                Session.SendWhisper("Este usuário já recebeu uma conta de Cheques!", 1);
                                break;
                            }
                        }
                    case "savings":
					case "poupanca":
                        {
                            int Cost = 250;
                            if (Target.GetRoleplay().BankAccount > 1)
                            {
                                Session.SendWhisper("Desculpe, mas essa pessoa já possui uma Conta de Poupança!", 1);
                                break;
                            }

                            bool HasOffer = false;
                            if (Target.GetHabbo().Credits >= Cost)
                            {
                                foreach (var Offer in Target.GetRoleplay().OfferManager.ActiveOffers.Values)
                                {
                                    if (Offer.Type.ToLower() == Params[2].ToLower())
                                        HasOffer = true;
                                }
                                if (!HasOffer)
                                {
                                    Session.Shout("*Ofertas uma conta de poupança para " + Target.GetHabbo().Username + " por R$" + String.Format("{0:N0}", Cost) + "*", 4);
                                    Target.GetRoleplay().OfferManager.CreateOffer("poupanca", Session.GetHabbo().Id, Cost);
                                    Target.SendWhisper("Você acabou de oferecer uma conta de poupança por R$" + String.Format("{0:N0}", Cost) + "! Digite ':aceitar poupanca' para ativá-la!", 1);
                                    break;
                                }
                                else
                                {
                                    Session.SendWhisper("Este usuário já recebeu uma conta de poupança!", 1);
                                    break;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("Este cidadão não tem grana para pagar uma Conta de poupança!", 1);
                                break;
                            }
                        }
                    default:
                        {
                            Session.SendWhisper("Insira um tipo de conta bancária válida! (cheques, poupanca)");
                            break;
                        }
                }
            }

            #endregion
        }
    }
}