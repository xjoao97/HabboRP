using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Banking
{
    class DepositCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_banking_deposit"; }
        }

        public string Parameters
        {
            get { return "%quantidade% %conta_bancaria%"; }
        }

        public string Description
        {
            get { return "Deposita dinheiro na sua conta desejada."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor insira o valor que deseja depositar.", 1);
                return;
            }

            if (Room.BankEnabled == false)
            {
                Session.SendWhisper("Você deve estar no banco para depositar dinheiro em suas contas!", 1);
                return;
            }

            if (Session.GetRoleplay().BankAccount <= 0)
            {
                Session.SendWhisper("Você não possui contas bancárias! Peça a um trabalhador bancário para abrir uma conta para você.", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("depositar"))
                return;

            int Amount;
            if (int.TryParse(Params[1], out Amount))
            {
                if (Amount <= 0)
                {
                    Session.SendWhisper("Insira um valor válido para depositar!", 1);
                    return;
                }

                if (Session.GetHabbo().Credits < Amount)
                {
                    Session.SendWhisper("Você não tem R$" + Amount + " para depositar!", 1);
                    return;
                }
            }
            else
            {
                Session.SendWhisper("Por favor insira um número para depositar!", 1);
                return;
            }
            #endregion

            #region Execute
            if (Params.Length == 2)
            {
                if (Session.GetRoleplay().BankAccount < 1)
                {
                    Session.SendWhisper("Você não tem uma conta chequings! Peça um trabalhador bancário para abrir um para você!", 1);
                    return;
                }

                Session.Shout("*Puxa R$" + String.Format("{0:N0}", Amount) + " do bolso e deposita em minha conta de cheques*", 5);

                Session.GetHabbo().Credits -= Amount;
                Session.GetHabbo().UpdateCreditsBalance();

                Session.GetRoleplay().BankChequings += Amount;
                Session.GetRoleplay().CooldownManager.CreateCooldown("depositar", 1000, 5);
            }
            else
            {
                switch (Params[2].ToLower())
                {
                    case "1":
                    case "chequings":
					case "cheques":
                        {
                            if (Session.GetRoleplay().BankAccount < 1)
                            {
                                Session.SendWhisper("Você não tem uma conta de Cheques! Peça um trabalhador bancário para abrir um para você!", 1);
                                return;
                            }

                            Session.Shout("*Puxa R$" + String.Format("{0:N0}", Amount) + " do bolso e deposita em minha conta de cheques*", 5);

                            Session.GetHabbo().Credits -= Amount;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankChequings += Amount;
                            Session.GetRoleplay().CooldownManager.CreateCooldown("depositar", 1000, 5);
                            break;
                        }
                    case "2":
                    case "savings":
					case "poupanca":
                        {
                            if (Session.GetRoleplay().BankAccount < 2)
                            {
                                Session.SendWhisper("Você não tem uma conta poupança! Peça um trabalhador bancário para abrir um para você!", 1);
                                return;
                            }

                            Session.Shout("*Puxa R$" + String.Format("{0:N0}", Amount) + " do bolso e deposita em minha conta poupança*", 5);

                            Session.GetHabbo().Credits -= Amount;
                            Session.GetHabbo().UpdateCreditsBalance();

                            Session.GetRoleplay().BankSavings += Amount;
                            Session.GetRoleplay().CooldownManager.CreateCooldown("depositar", 1000, 5);
                            break;
                        }
                    default:
                        {
                            Session.SendWhisper("Por favor use 'cheques' ou 'poupanca' para depositar no tipo de conta!", 1);
                            break;
                        }
                }
            }
            #endregion
        }
    }
}